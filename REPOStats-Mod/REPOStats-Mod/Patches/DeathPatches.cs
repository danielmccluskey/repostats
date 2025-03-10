using HarmonyLib;
using REPOStats_Mod.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace REPOStats_Mod.Patches
{
    [HarmonyPatch]
    public class DeathPatches
    {
        private static readonly AccessTools.FieldRef<Enemy, EnemyParent> enemyRef =
            AccessTools.FieldRefAccess<Enemy, EnemyParent>("EnemyParent");

        [HarmonyPatch(typeof(PlayerAvatar), "PlayerDeathRPC")]
        [HarmonyPostfix]
        public static void PlayerDeathRPCPostfix(PlayerAvatar __instance, int enemyIndex)
        {
            var steamID = SemiFunc.PlayerGetSteamID(__instance);
            if (steamID == null)
            {
                return;
            }

            //We only care about my deaths
            if(!DanosUtils.GetMySteamID().Equals(steamID))
            {
                return;
            }


            string causeOfDeath = "Unknown";

            var Enemy = SemiFunc.EnemyGetFromIndex(enemyIndex);
            if (Enemy != null)
            {
                causeOfDeath = Enemy.name;

                EnemyParent parent = enemyRef(Enemy);
                if (parent != null)
                {
                    causeOfDeath = parent.enemyName;
                }
            }

            DanosDeathContainer death = new DanosDeathContainer();
            death.DeathTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            death.CauseOfDeath = causeOfDeath;

            DanosStaticStore.statsStore.Deaths.Add(death);


        }

    }
}
