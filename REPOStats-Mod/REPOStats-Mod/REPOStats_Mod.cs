using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using REPOStats_Mod.Data;
using REPOStats_Mod.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace REPOStats_Mod;
public static class DanosRepoStatsPluginInfo
{
    public const string PLUGIN_GUID = "com.danos.repostats";
    public const string PLUGIN_NAME = "repostats";
    public const string PLUGIN_VERSION = "0.5.2";
}
[BepInPlugin(DanosRepoStatsPluginInfo.PLUGIN_GUID, DanosRepoStatsPluginInfo.PLUGIN_NAME, DanosRepoStatsPluginInfo.PLUGIN_VERSION)]
public class REPOStats_Mod : BaseUnityPlugin
{
    public static REPOStats_Mod Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static Harmony? Harmony { get; set; }
    private static readonly HashSet<Type> AppliedPatches = new HashSet<Type>();
    private static readonly HashSet<string> AppliedDynamicPatches = new HashSet<string>();
    private static bool _additionalPatchesApplied = false;

    private void Awake()
    {
        Logger = base.Logger;
        Instance = this;

        Patch();

        Logger.LogInfo($"{DanosRepoStatsPluginInfo.PLUGIN_GUID} v{DanosRepoStatsPluginInfo.PLUGIN_VERSION} has loaded!");
    }

    internal static void Patch()
    {
        Harmony ??= new Harmony(DanosRepoStatsPluginInfo.PLUGIN_GUID);

        Logger.LogDebug("Patching...");

        Harmony.PatchAll(typeof(PrivacyPolicyPatch));

        Logger.LogDebug("Finished patching!");
    }

    internal static void Unpatch()
    {
        Logger.LogDebug("Unpatching...");

        Harmony?.UnpatchSelf();

        Logger.LogDebug("Finished unpatching!");
    }

    //We use this method after we have verified the user has accepted the privacy policy
    public static void ApplyAdditionalPatches()
    {
        if (_additionalPatchesApplied)
        {
            Logger.LogDebug("Additional patches already applied.");
            return;
        }

        ApplyPatch(typeof(RunPatches));
        ApplyPatch(typeof(RoundDirectorPatches));
        ApplyPatch(typeof(DeathPatches));

        // Define patches dynamically
        var patchConfigs = DanosPatchManager.GetPatchConfigurations();

        foreach (var patchConfig in patchConfigs)
        {
            ApplyDynamicPatch(patchConfig);
        }

        _additionalPatchesApplied = true;
        Logger.LogDebug("RepoStats is loaded!");

    }
    private static void ApplyDynamicPatch(DanosPatchConfiguration patchConfig)
    {
        try
        {
            string patchIdentifier = $"{patchConfig.TargetClass}.{patchConfig.TargetMethod}.{patchConfig.PostfixMethod}";
            if (AppliedDynamicPatches.Contains(patchIdentifier))
            {
                Logger.LogDebug($"Patch already applied: {patchIdentifier}");
                return;
            }

            Type targetType = AccessTools.TypeByName(patchConfig.TargetClass);
            if (targetType == null)
            {
                Logger.LogDebug($"Target class '{patchConfig.TargetClass}' not found. Skipping patch.");
                return;
            }

            MethodInfo targetMethod = AccessTools.Method(targetType, patchConfig.TargetMethod);
            if (targetMethod == null)
            {
                Logger.LogDebug($"Target method '{patchConfig.TargetMethod}' not found in class '{patchConfig.TargetClass}'. Skipping patch.");
                return;
            }

            // Get the postfix method using its full name (Namespace.Class.Method)
            string[] methodParts = patchConfig.PostfixMethod.Split('.');
            string methodName = methodParts.Last();
            string className = string.Join('.', methodParts.Take(methodParts.Length - 1));
            Type postfixClass = Type.GetType(className);
            if (postfixClass == null)
            {
                Logger.LogDebug($"Postfix class '{className}' not found. Skipping patch.");
                return;
            }

            MethodInfo postfixMethod = postfixClass.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (postfixMethod == null)
            {
                Logger.LogDebug($"Postfix method '{patchConfig.PostfixMethod}' not found. Skipping patch.");
                return;
            }

            Harmony.Patch(targetMethod, postfix: new HarmonyMethod(postfixMethod));
            AppliedDynamicPatches.Add(patchIdentifier); // Mark this patch as applied
            Logger.LogDebug($"Successfully patched {patchConfig.TargetClass}.{patchConfig.TargetMethod}");
        }
        catch (Exception ex)
        {
            Logger.LogDebug($"Failed to apply patch for {patchConfig.TargetClass}.{patchConfig.TargetMethod}: {ex.Message}");
        }
    }
    private static void ApplyPatch(Type patchType)
    {
        if (!AppliedPatches.Contains(patchType))
        {
            Harmony.PatchAll(patchType);
            AppliedPatches.Add(patchType);
            Logger.LogDebug($"Patch applied: {patchType.Name}");
        }
        else
        {
            Logger.LogDebug($"Patch already applied: {patchType.Name}");
        }
    }
}
