using HarmonyLib;
using MonoMod.Utils;
using YoYoStudio.Core.Utils;

namespace IdeRunner.Patches;

[HarmonyPatch]
public class LanguagePatches {
    private static Dictionary<string, string>? StartUpDefaults;
    [HarmonyPatch(typeof(Language), nameof(Language.GetString), typeof(LanguagePack), typeof(string))]
    [HarmonyPrefix]
    public static bool GetStringLpPatch([HarmonyArgument(0)] LanguagePack? pack, [HarmonyArgument(1)] string? id, out string? __result) {
        if (!Config.Instance.PatchLanguages) {
            __result = null;
            return true;
        }

        if (pack == null) {
            __result = "<error>";
            StartUpDefaults ??= new DynamicData(typeof(Language)).Get<Dictionary<string, string>>("StartUpDefaults");
            StartUpDefaults.TryGetValue(id, out __result);
            return false;
        }

        __result = id ?? string.Empty;
        if (!string.IsNullOrEmpty(id)) {
            if (pack.Lookups.TryGetValue(id, out __result))
                return false;

            if (Language.EnglishPack.Lookups.TryGetValue(id, out __result))
                return false;
        }

        return false;
    }
}