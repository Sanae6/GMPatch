using System.Reflection;
using HarmonyLib;
using YoYoStudio.Utils;

namespace IdeRunner.Patches;

/**
 * StartupPatches holds patches which are required for the IDE to boot.
 */
[HarmonyPatch]
public static class StartupPatches {
    [HarmonyPatch]
    public static class DisableVerifySecurityPatch {
        [HarmonyTargetMethod]
        public static MethodInfo TargetMethod() {
            // WinTrust is an internal class, can't use typeof(WinTrust) here.
            return AccessTools.Method(typeof(ManualRSSFeed).Assembly.GetType("YoYoStudio.Utils.WinTrust", true)!,
                "VerifySecurity");
        }

        public static bool Prefix(out bool __result) {
            __result = true;
            // Prevent GMS2 from detecting anything out of the ordinary.
            // This is used in plugin assembly loading, and the validation of all core assemblies (see SecurityChecks.VerifyDlls
            return false;
        }
    }

    [HarmonyPatch(typeof(Environment), nameof(Environment.CurrentDirectory), MethodType.Setter)]
    [HarmonyPrefix]
    public static bool PreventCurrentDirectorySet() {
        // Prevent the setter from ever being called.
        // Current directory will be set in Run.RunIde before patch is run.
        return false;
    }

    [HarmonyPatch(typeof(Assembly), nameof(Assembly.GetEntryAssembly))]
    [HarmonyPostfix]
    public static void GetEntryAssemblyOverride(out Assembly __result) =>
        __result = typeof(YoYoStudio.Program).Assembly;
}