using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Tetrahelper.Entities;

[CustomEntity("Tetrahelper/SampleSolid")]
public class SampleSolid : Solid
{
    public SampleSolid(EntityData data, Vector2 offset)
        : base(data.Position + offset, data.Width, data.Height, true)
    {
        
        
    }
}