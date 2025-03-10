using HarmonyLib;
using REPOStats_Mod.Data;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace REPOStats_Mod.Patches
{
    [HarmonyPatch]
    public class RoundDirectorPatches
    {
        private static readonly AccessTools.FieldRef<RoundDirector, int> extractionPointsCompletedRef =
            AccessTools.FieldRefAccess<RoundDirector, int>("extractionPointsCompleted");
        private static readonly AccessTools.FieldRef<RoundDirector, int> extractionPointsRef =
            AccessTools.FieldRefAccess<RoundDirector, int>("extractionPoints");


        [HarmonyPatch(typeof(RoundDirector), "ExtractionCompleted")]
        [HarmonyPostfix]
        public static void ExtractionCompletedPostfix(RoundDirector __instance)
        {
            DanosStaticStore.statsStore.RunStats.extractions_completed = extractionPointsCompletedRef(__instance);
        }

        [HarmonyPatch(typeof(RoundDirector), "StartRoundLogic")]
        [HarmonyPostfix]
        public static void StartRoundLogicPostfix(RoundDirector __instance, int value)
        {
            DanosStaticStore.statsStore.RunStats.extractions_on_map = extractionPointsRef(__instance);
        }
        [HarmonyPatch(typeof(ExtractionPoint), "HaulGoalSetRPC")]
        [HarmonyPostfix]
        public static void StartRoundLogicPostfix(ExtractionPoint __instance, int value)
        {
            DanosStaticStore.statsStore.RunStats.extraction_goals_csv += value + ",";
        }

    }
}
