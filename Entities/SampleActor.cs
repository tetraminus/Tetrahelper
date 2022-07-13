using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.Tetrahelper.Entities {
    [CustomEntity("Tetrahelper/SampleActor")]
    public class SampleActor : Actor {
        public SampleActor(EntityData data, Vector2 offset)
            : base(data.Position + offset) {
            // TODO: read properties from data
        }
    }
}
