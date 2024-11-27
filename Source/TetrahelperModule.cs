using System;

namespace Celeste.Mod.Tetrahelper;
using Entities;

public class TetrahelperModule : EverestModule {
    public static TetrahelperModule Instance { get; private set; }

    public override Type SettingsType => typeof(TetrahelperModuleSettings);
    public static TetrahelperModuleSettings Settings => (TetrahelperModuleSettings) Instance._Settings;

    public override Type SessionType => typeof(TetrahelperModuleSession);
    public static TetrahelperModuleSession Session => (TetrahelperModuleSession) Instance._Session;

    public override Type SaveDataType => typeof(TetrahelperModuleSaveData);
    public static TetrahelperModuleSaveData SaveData => (TetrahelperModuleSaveData) Instance._SaveData;

    public TetrahelperModule() {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(TetrahelperModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(srcModule), LogLevel.Info);
#endif
    }

    public override void Load()
    {
        On.Celeste.Solid.HasPlayerRider += Solid_HasPlayerRider;
        Glue.Load();
    }


    public override void Unload()
    {
        On.Celeste.Solid.HasPlayerRider -= Solid_HasPlayerRider;
        Glue.Unload();
    }

    private static bool Solid_HasPlayerRider(On.Celeste.Solid.orig_HasPlayerRider orig, Solid self)
    {
        if (self is ZipMover && ZipTrigger.ShouldTrigger()) return true;

        return orig(self);
    }
}