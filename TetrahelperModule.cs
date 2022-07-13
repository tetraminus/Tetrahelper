using System;
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
            // TODO: apply any hooks that should always be active
        }

        public override void Unload() {
            // TODO: unapply any hooks applied in Load()
        }
    }
}