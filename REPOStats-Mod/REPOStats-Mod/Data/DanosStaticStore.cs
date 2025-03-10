using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace REPOStats_Mod.Data
{
    public static class DanosStaticStore
    {
        public static long lastSentStats = 0;
        public static DanosStatsStore statsStore = new DanosStatsStore();

        

        public static void SendStatsToAPI()
        {
            if (CheckStatsStore())
            {
                Debug.Log("Sending stats to API");
                DanosStatSender.Instance.SendStats(statsStore);
            }

            Debug.Log("Resetting stats store");
            ResetStatsStore();
            lastSentStats = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public static bool CheckStatsStore()
        {
            if (statsStore == null) return false;
            if (statsStore.MySteamId <= 1000) return false;
            if (statsStore.RoundStarted <= 0) return false;
            if (statsStore.RoundEnded <= 0) return false;
            if (statsStore.RoundEnded < statsStore.RoundStarted) return false;

            return true;
        }

        public static void ResetStatsStore()
        {
            statsStore = new DanosStatsStore();
        }

        public static void InitializeStatsStore()
        {
            ResetStatsStore();
            string MySteamId = DanosUtils.GetMySteamID();
            if (string.IsNullOrEmpty(MySteamId))
            {
                Debug.LogError("Could not get my steam id");
                return;
            }

            //Try parse the steam id as a long
            if (!long.TryParse(MySteamId, out long steamId))
            {
                Debug.LogError("Could not parse steam id as long");
                return;
            }

            //Do the same for the host id
            string HostSteamId = DanosUtils.GetHostSteamID();
            Debug.Log("Host: " + HostSteamId);
            if (string.IsNullOrEmpty(HostSteamId))
            {
                Debug.LogError("Could not get host steam id");
                return;
            }

            if (!long.TryParse(HostSteamId, out long hostSteamId))
            {
                Debug.LogError("Could not parse host steam id as long");
                return;
            }

            statsStore.MySteamId = steamId;
            statsStore.LobbyHash = DanosUtils.GetLobbyHash();
            statsStore.isHost = steamId == hostSteamId;



            statsStore.RoundStarted = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            statsStore.Deaths = new List<DanosDeathContainer>();
            statsStore.RunStats = new();

            RunManager runManager = RunManager.instance;
            if (runManager != null)
            {
                statsStore.LevelName = runManager.levelCurrent.name;
            }



        }
    }

    
}
