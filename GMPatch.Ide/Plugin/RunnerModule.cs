using YoYoStudio.GUI.Gadgets;
using YoYoStudio.Plugins;
using YoYoStudio.Plugins.Attributes;

namespace IdeRunner.Plugin;

[ModuleName("ide_runner", "Create test window")]
internal class RunnerModule : IModule {
    public void Dispose() {
        
    }

    public void Initialise(ModulePackage _ide) {
        Package = _ide;
        Window window = new Window("Among", 10, 10, 200, 200, eWindowFlags.None, this);
        _ide.WindowManager.AddWindow(window);
    }

    public ModulePackage Package { get; private set; }
}