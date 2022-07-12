using System.Reflection;
using HarmonyLib;
using MonoMod.RuntimeDetour;
using YoYoStudio;
using YoYoStudio.Core.Utils;
using YoYoStudio.User.UserPrefs;

namespace IdeRunner.Patches;

// TODO: Build up a compiler plugin framework.
// Not moving in my old compiler runner since it's just a testbed for any compiler related stuff I wanted to try.
// It will likely be used as the base though.
// [HarmonyPatch]
public class BuildPatches {
    // [HarmonyPatch(typeof(DefaultMacros), nameof(DefaultMacros.RegisterDefaultMacros))]
    [HarmonyPostfix]
    public static void SetMacros() {
        MacroExpansion.Remove("asset_compiler_path");
        MacroExpansion.Add("asset_compiler_path", $"{Run.RunnerLocation}/GMPatch.AssetCompiler.dll");
    }

    // [HarmonyPatch(typeof(GeneralSettings), "UpdateAssetCompilerPath")]
    [HarmonyPrefix]
    public static bool DisableUpdateAssetComp() {
        MacroExpansion.Add("asset_compiler_path_runner", YoYoPath.GetDirectoryName(DefaultMacros.DefaultAssetCompilerPath));
        return false;
    }

    // [HarmonyPatch(typeof(StableHash), nameof(StableHash.GetHashCode))]
    [HarmonyPrefix]
    public static bool GetHashCode(out int __result) {
        __result = 0;
        return false;
    }

    [HarmonyPatch]
    public static class PredictableDestFolderPatch {
        // We don't have access to CorePlugins in this specific scenario. 
        private static readonly Type SplashScreen = typeof(IDE).Assembly.GetType("YoYoStudio.SplashScreen", true)!;
        private static bool WasPlugins = false;
        private static MethodInfo GetState => AccessTools.PropertyGetter(SplashScreen, "State");

        [HarmonyTargetMethod]
        public static MethodInfo TargetMethod() =>
            AccessTools.Method(SplashScreen, "SplashTick");

        [HarmonyPostfix]
        public static void SplashTickPluginsEnd(object __instance) {
            int state = (int) GetState.Invoke(__instance, Array.Empty<object>())!;
            if (!WasPlugins && state == 14) WasPlugins = true;
            if (WasPlugins && state == 7) {
                WasPlugins = false;
                Patch();
            }
        }

        private static void Patch() {
            Type type = AccessTools.TypeByName("YoYoStudio.Plugins.CorePlugins.Build.Common");
            MethodInfo? destFolder = AccessTools.Method(type, "DestFolder");
            Hook hook = new Hook(destFolder, DestFolder);
            if (!hook.IsApplied) hook.Apply();
        }

        private static string DestFolder(string runtime) {
            return MacroExpansion.Expand(YoYoPath.Combine(MacroExpansion.Expand(DefaultMacros.TempDirectory), "${project_name}"));
        }
    }
}