using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MonoMod.Utils;
using YoYoStudio;
using YoYoStudio.Plugins;

namespace IdeRunner.Patches;

[HarmonyPatch]
public static class PluginPatches {
    [HarmonyPatch(typeof(PluginManager), "InitialiseAvailablePlugins")]
    [HarmonyPrefix]
    public static void AddRunnerPlugin() {
        YoYoStudio.Plugins.Plugin plugin = new YoYoStudio.Plugins.Plugin {
            TheAssembly = typeof(RunnerPlugin).Assembly
        };
        Console.WriteLine($"Plugins: {string.Join(", ", plugin.TheAssembly.DefinedTypes.Where(type => typeof(IPlugin).IsAssignableFrom(type) && !type.IsAbstract).Select(type => type.FullName)!.ToList<string>())}");
        Console.WriteLine($"Modules: {string.Join(", ", plugin.TheAssembly.DefinedTypes.Where(type => typeof(IModule).IsAssignableFrom(type) && !type.IsAbstract).Select(type => type.FullName)!.ToList<string>())}");
        DynamicData plugDyn = new DynamicData(plugin);
        if (!plugDyn.Invoke<bool>("ScanAssembly") || !plugDyn.Invoke<bool>("InitaliseModules"))
            throw new Exception("Failed to load our assembly");
        DynamicData pm = new DynamicData(typeof(PluginManager));
        DynData<PluginDetails> pluginDetails2 = new DynData<PluginDetails>(new PluginDetails("run.dll", "run.dll", plugin.PluginConfigs[0].Description, plugin.PluginConfigs[0].Name));
        pluginDetails2.Set("active", true);
        pluginDetails2.Set("required", plugin.PluginConfigs[0].Required);
        pm.Get<Dictionary<string, PluginDetails>>("AvailablePlugins")
            .Add("run.dll", pluginDetails2.Target);
        pm.Get<List<YoYoStudio.Plugins.Plugin>>("Plugins").Add(plugin);
        object onPluginLoaded = pm.Get("OnPluginLoaded");
        if (onPluginLoaded != null)
            new DynamicData(onPluginLoaded).Invoke("Throw", plugin);
    }

    [HarmonyPatch]
    public static class BuildPluginListPatches {
        private static List<string> Plugins = new List<string>();

        [HarmonyTargetMethod]
        public static MethodInfo TargetMethod() =>
            AccessTools.Method(typeof(IDE).Assembly.GetType("YoYoStudio.SplashScreen", true), "BuildPluginLists");

        private static readonly (OpCode Op, int Value)[] IntOpcodes = {
            (OpCodes.Ldc_I4_0, 0),
            (OpCodes.Ldc_I4_1, 1),
            (OpCodes.Ldc_I4_2, 2),
            (OpCodes.Ldc_I4_3, 3),
            (OpCodes.Ldc_I4_4, 4),
            (OpCodes.Ldc_I4_5, 5),
            (OpCodes.Ldc_I4_6, 6),
            (OpCodes.Ldc_I4_7, 7),
            (OpCodes.Ldc_I4_8, 8),
        };

        private static int? IntConstValue(CodeInstruction? inst) {
            if (inst == null) return null;
            if (inst.opcode.StackBehaviourPush == StackBehaviour.Pushi) {
                if (inst.opcode == OpCodes.Ldc_I4 || inst.opcode == OpCodes.Ldc_I4_S) {
                    return inst.opcode == OpCodes.Ldc_I4 ? (int) inst.operand : (sbyte) inst.operand;
                }

                return IntOpcodes.Where(x => x.Op == inst.opcode).Select(x => x.Value).FirstOrDefault();
            }

            return null;
        }

        [HarmonyPrefix]
        private static void Prefix() {
            FieldInfo paths = AccessTools.Field(typeof(PluginManager), "PluginPaths");
            IList pathList = (IList) paths.GetValue(null)!;
            foreach (object value in pathList) {
                DynamicData details = DynamicData.Wrap(value);
                string path = details.Get<string>("Path");
                if (!path.EndsWith("Plugins") || !Directory.Exists(path))
                    continue;
                foreach (string file in Directory.EnumerateFiles(path, "*.dll")) {
                    string fileName = Path.GetFileName(file);
                    if (!Plugins.Contains(fileName))
                        Plugins.Add(fileName);
                }
            }
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            CodeInstruction? prev;
            using IEnumerator<CodeInstruction> enumerator = instructions.GetEnumerator();
            while (true) {
                prev = enumerator.Current;
                if (!enumerator.MoveNext()) break;
                CodeInstruction inst = enumerator.Current;
                if (IntConstValue(prev) is { } count && inst.Is(OpCodes.Newarr, typeof(string))) {
                    yield return new CodeInstruction(OpCodes.Pop);
                    for (int i = 0; i < count; i++) {
                        enumerator.MoveNext(); // dup
                        enumerator.MoveNext(); // ldc_i4(_n)
                        int offset = IntConstValue(enumerator.Current)!.Value;
                        enumerator.MoveNext(); // ldstr
                        Plugins.Insert(offset, (string) enumerator.Current.operand);
                        enumerator.MoveNext(); // stelem.ref
                    }

                    yield return CodeInstruction.LoadField(typeof(BuildPluginListPatches), nameof(Plugins));
                    yield return CodeInstruction.Call(typeof(List<string>), nameof(List<string>.ToArray));
                    continue;
                }

                yield return inst;
            }
        }
    }

    // private static bool ObfuscateLocation;
    // [HarmonyPatch(typeof(YoYoStudio.Plugins.Plugin), "LoadAssembly")]
    // [HarmonyPrefix]
    // [HarmonyPostfix]
    // public static void PluginLoadAssemblyLocationToggle() {
    //     ObfuscateLocation = !ObfuscateLocation;
    // }
    //
    // [HarmonyPatch(typeof(Assembly), nameof(Assembly.Location), MethodType.Getter)]
    // [HarmonyPrefix]
    // public static bool ChangeLocation(ref string __result) {
    //     __result = string.Empty;
    //     return ObfuscateLocation;
    // }
}