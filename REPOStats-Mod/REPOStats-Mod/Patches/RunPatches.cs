using HarmonyLib;
using REPOStats_Mod.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static RunManager;

namespace REPOStats_Mod.Patches
{
    [HarmonyPatch]
    public class RunPatches
    {
        [HarmonyPatch(typeof(RunManager), "UpdateLevel")]
        [HarmonyPostfix]
        public static void UpdateLevelPostFix(string _levelName, int _levelsCompleted, bool _gameOver)
        {
            //Safety dont run this one if you are the host
            if (DanosUtils.GetHostSteamID().Equals(DanosUtils.GetMySteamID()))
            {
                return;
            }
            CombinedLevelLogic(_levelName, _levelsCompleted, _gameOver);
            return;
        }

        [HarmonyPatch(typeof(RunManager), "ChangeLevel")]
        [HarmonyPostfix]
        public static void ChangeLevelPostFix(bool _completedLevel, bool _levelFailed, ChangeLevelType _changeLevelType = ChangeLevelType.Normal)
        {
            //Safety dont run this one if you are not the host
            if (!DanosUtils.GetHostSteamID().Equals(DanosUtils.GetMySteamID()))
            {
                return;
            }
            RunManager _instance = RunManager.instance;
            if (_instance != null) {
                CombinedLevelLogic(_instance.levelCurrent.name, _instance.levelsCompleted, ((instance.levelCurrent == instance.levelArena) && _levelFailed && (instance.levelCurrent != instance.levelShop) && (instance.levelCurrent != instance.levelLobby)));
            }
            return;
        }


        [HarmonyPatch(typeof(TruckScreenText), "Start")]
        [HarmonyPostfix]
        public static void Postfix(TruckScreenText __instance)
        {
            
            string MySteamId = DanosUtils.GetMySteamID();
            if (string.IsNullOrEmpty(MySteamId))
            {
                Debug.LogError("Could not get my steam id");
                return;
            }

            if (!long.TryParse(MySteamId, out long steamId))
            {
                Debug.LogError("Could not parse steam id as long");
                return;
            }

            string HostSteamId = DanosUtils.GetHostSteamID();
            if (string.IsNullOrEmpty(HostSteamId))
            {
                Debug.LogError("Could not get host steam id");
                return;
            }

            if (!long.TryParse(HostSteamId, out long hostId))
            {
                Debug.LogError("Could not parse host steam id as long");
                return;
            }


            // Ensuring the store is initialized after LateStart coroutine ends
            DanosStaticStore.InitializeStatsStore();

        }


        public static void CombinedLevelLogic(string _levelName, int _levelsCompleted, bool _gameOver)
        {

            if (RunManager.instance == null)
            {
                Debug.LogError("RunManager instance is null");
                return;
            }

            if (RunManager.instance.levels == null)
            {
                Debug.LogError("RunManager levels is null");
                return;
            }

            if (_levelName == RunManager.instance.levelLobbyMenu.name)
            {
                
                DanosStaticStore.ResetStatsStore();
            }
            
            else if (_levelName == RunManager.instance.levelShop.name)
            {
                //They got to the shop, they just completed a level, send stats
                RoundEnded("Success");

            }
            else if (_levelName == RunManager.instance.levelArena.name)
            {
                RoundEnded("GameOver");
            }
            else if (_levelName == RunManager.instance.levelRecording.name)
            {
            }
            else
            {
                Debug.Log("Level: " + _levelName);
                foreach (Level level in RunManager.instance.levels)
                {
                    if (level.name == _levelName)
                    {
                        //They just started a level
                        DanosStaticStore.ResetStatsStore();   
                        break;
                    }
                }


            }
        }

        public static void RoundEnded(string a_reason)
        {
            DanosStaticStore.statsStore.RoundEnded = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            DanosStaticStore.statsStore.RunEndReason = a_reason;





            DanosStaticStore.SendStatsToAPI();
            DanosStaticStore.ResetStatsStore();
        }
    }
}
