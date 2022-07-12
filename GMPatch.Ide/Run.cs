using System.Reflection;
using System.Runtime.InteropServices;
using HarmonyLib;
using IdeRunner.Patches;
using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour;
using YoYoStudio;
using YoYoStudio.Core.Utils;
using YoYoStudio.Plugins;

namespace IdeRunner; 

internal class Run {
    public static Harmony Harmony = null!;
    internal static string RunnerLocation => Path.GetDirectoryName(typeof(Run).Assembly.Location)!; 

    public static void RunIde(string[] args) {
        Harmony = new Harmony("ca.sanae.ide");
        Harmony.PatchAll(typeof(PluginPatches));
        Harmony.PatchAll(typeof(LanguagePatches));
        Harmony.PatchAll(typeof(StartupPatches));
        // patchall should hit assembly later after plugins have loaded!
        Console.WriteLine($"Loading from {Environment.CurrentDirectory}, {Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!}, {Path.GetFullPath("defaults/configuration.json")}");

        SetDefaultDllDirectories();
        AddDllDirectory(Environment.CurrentDirectory);
        YoYoStudio.Program.Main(args);
    }

    [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool AddDllDirectory(string directory);

    [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool SetDefaultDllDirectories(uint directoryFlags = 0x1000);
}