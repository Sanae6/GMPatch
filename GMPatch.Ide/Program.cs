using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using IdeRunner;

// This AssemblyResolve handler is used to load the GameMaker Studio binaries from the process's current directory.
// The intent is to prevent Run.RunIde from attempting (and failing) to load the binaries from the IdeRunner's folder.
AppDomain.CurrentDomain.AssemblyResolve += (_, args) => {
    try {
        return Assembly.LoadFile(Path.Combine(Environment.CurrentDirectory, new AssemblyName(args.Name).Name + ".dll"));
    }
    catch (Exception e) {
        return null;
    }
};

if (!File.Exists(Config.BasePath)) {
    File.WriteAllText(Config.BasePath, JsonSerializer.Serialize(new Config(), new JsonSerializerOptions {
        WriteIndented = true
    }));
    // the only thing that matters here is the gamemaker path, a good idea would be to do a search,
    // and then open the file if the search fails.
    if (OperatingSystem.IsWindows())
        Process.Start("explorer.exe", Config.BasePath);
    // else {cross platform way to open config folder}
    return;
}

Config.Instance = JsonSerializer.Deserialize<Config>(File.ReadAllText(Config.BasePath))!;
// rewrite it in case new config options are added
File.WriteAllText(Config.BasePath, JsonSerializer.Serialize(Config.Instance, new JsonSerializerOptions {
    WriteIndented = true
}));
Environment.CurrentDirectory = Config.Instance.GameMakerPath;

// Start IDE process
Run.RunIde(args);

class Config {
    public static Config Instance = null!;
    public const string BasePath = "config.json";
    public string GameMakerPath { get; set; } = string.Empty;
    public bool PatchLanguages { get; set; } = true;
    public bool EnableDebugMode { get; set; } = false;
}