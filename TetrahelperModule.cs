using System;
using Celeste.Mod.TetraHelper.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.Tetrahelper {
    public class TetrahelperModule : EverestModule {
        public static TetrahelperModule Instance { get; private set; }

        public override Type SettingsType => typeof(TetrahelperModuleSettings);
        public static TetrahelperModuleSettings Settings => (TetrahelperModuleSettings) Instance._Settings;

        public override Type SessionType => typeof(TetrahelperModuleSession);
        public static TetrahelperModuleSession Session => (TetrahelperModuleSession) Instance._Session;

        public TetrahelperModule() {
            Instance = this;
        }

        public override void Load() {
            DashMatchWallRenderer.Load();
        }

        public override void Unload() {
            DashMatchWallRenderer.Unload();
        }
    }
}