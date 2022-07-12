using IdeRunner.Plugin;
using YoYoStudio.Core.Utils.Preferences;
using YoYoStudio.Plugins;

namespace IdeRunner;

internal class RunnerPlugin : IPlugin {
    public PluginConfig Initialise() {
        PreferencesManager.Register(typeof(RunnerPreferences));
        return new PluginConfig("agogus", "Agringus", true);
    }
}