using System.ComponentModel;
using System.Runtime.CompilerServices;
using YoYoStudio.Core.Utils;
using YoYoStudio.Core.Utils.Preferences;

namespace IdeRunner.Plugin;

internal sealed class RunnerPreferences : INotifyPropertyChanged {
    public event PropertyChangedEventHandler? PropertyChanged;
    public const string PluginRoot = "machine.Plugins.IdeRunner";
    public const string PluginLocalizedRoot = "IdeRunner";
    public const string PluginPathCool = $"{PluginRoot}.Cool";
    public const string PluginLocalizedCool = "plugin_runner_cool";

    public RunnerPreferences() {
        // PreferencesManager.Add(PluginRoot);
        foreach (LanguagePack pack in Language.GetLanguages()) {
            pack.Lookups[PluginLocalizedRoot] = PluginLocalizedRoot;
            pack.Lookups[PluginLocalizedCool] = "Holy shit a checkbox";
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool AmCool;

    [Prefs(PluginPathCool, 0, "Are you cool?", PluginLocalizedCool, ePrefType.boolean)]
    public bool Cool {
        get => AmCool;
        set {
            AmCool = false;
            OnPropertyChanged(nameof(Cool));
        }
    }
}