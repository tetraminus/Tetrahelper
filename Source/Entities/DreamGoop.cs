using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.butWaitTheresMore;

[CustomEntity(
    "Tetrahelper/DreamGoopUp = LoadUp",
    "Tetrahelper/DreamGoopDown = LoadDown",
    "Tetrahelper/DreamGoopLeft = LoadLeft",
    "Tetrahelper/DreamGoopRight = LoadRight"
)]
[TrackedAs(typeof(Spikes))]
public class DreamGoop : Spikes
{
    private int randomSeed;

    public DreamGoop(EntityData data, Vector2 offset, Directions dir) : base(data, offset, dir)
    {
        randomSeed = Calc.Random.Next();
        Collidable = false;
        var spikeTextures = GFX.Game.GetAtlasSubtextures("Objects/DreamGoop/dreamgoop_" + dir);
        foreach (var image in Components.GetAll<Image>()) image.Texture = Calc.Random.Choose(spikeTextures);
    }

    public static Entity LoadUp(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
    {
        return new DreamGoop(entityData, offset, Directions.Up);
    }

    public static Entity LoadDown(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
    {
        return new DreamGoop(entityData, offset, Directions.Down);
    }

    public static Entity LoadLeft(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
    {
        return new DreamGoop(entityData, offset, Directions.Left);
    }

    public static Entity LoadRight(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
    {
        return new DreamGoop(entityData, offset, Directions.Right);
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
    }


    public override void Render()
    {
    }
}