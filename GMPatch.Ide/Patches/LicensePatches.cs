using System.Reflection;
using System.Text;
using HarmonyLib;
using YoYoStudio;

namespace IdeRunner.Patches;

[HarmonyPatch]
public static class LicensePatches {
    [HarmonyPatch]
    public static class EnableDebugInternalMode {
        [HarmonyTargetMethod]
        public static MethodInfo TargetMethod() {
            // targeting A.A.A(string, out Dictionary<string, string>)
            return typeof(IDE).Assembly.GetTypes().Where(x => x.Namespace?.Length == 1 && x.Name.Length == 1)
                .SelectMany(x => x.GetMethods(BindingFlags.NonPublic | BindingFlags.Static)).First(x => {
                    ParameterInfo[] infos = x.GetParameters();
                    if (infos.Length != 2) return false;
                    if (infos[0].ParameterType != typeof(string) || !infos[1].IsOut) return false;
                    if (infos[1].ParameterType != typeof(Dictionary<string, string>).MakeByRefType()) return false;
                    return true;
                });
        }

        [HarmonyPostfix]
        public static void Postfix([HarmonyArgument(1)] ref Dictionary<string, string> licenseFile) {
            if (!Config.Instance.EnableDebugMode) return;
            bool isAddons = licenseFile.ContainsKey("Addons");
            string[] modules = {
                "debug.internal"
            };
            string addonsKey = isAddons ? "Addons" : "components";
            char addonChar = isAddons ? '.' : ';';
            StringBuilder builder = new StringBuilder(licenseFile[addonsKey]);
            foreach (string module in modules) {
                builder.Append(addonChar)
                    .Append(module)
                    .Append(addonChar)
                    .Append(module)
                    .Append(".build_module");
            }
            licenseFile[addonsKey] = builder.ToString();
        }
    }

    [HarmonyPatch]
    public static class AlwaysEnableVerification {
        [HarmonyTargetMethod]
        public static MethodInfo TargetMethod() {
            // targeting bool A.A.something's setter, it's not a valid property due to the obfuscation messing with it
            return typeof(IDE).Assembly.GetTypes().Where(x => x.Namespace?.Length == 1 && x.Name.Length == 1)
                .SelectMany(x => x.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)).First(x => {
                    ParameterInfo[] infos = x.GetParameters();
                    if (infos.Length != 1) return false;
                    return infos[0].ParameterType == typeof(bool);
                });
        }

        [HarmonyPrefix]
        public static void Prefix(out bool __0) {
            __0 = true;
        }
    }
}