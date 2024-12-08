using System;
using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Tetrahelper.Entities;

[CustomEntity("Tetrahelper/PowerStation")]
internal class PowerStation : Entity
{
    public static ParticleType P_Appear;


    private readonly string Flag;


    private readonly Sprite sprite;


    private readonly string unlockSfxName;


    private BloomPoint bloom;


    public EntityID ID;
    private VertexLight light;


    private bool on;


    private bool opening;


    public bool UnlockingRegistered;

    public PowerStation(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Add(sprite = GFX.SpriteBank.Create("PowerStation"));
        Add(new PlayerCollider(OnPlayer, new Circle(40f)));
        sprite.Play("offIdle");
        
        Depth = 1965;
        unlockSfxName = "event:/game/05_mirror_temple/key_unlock_dark";
        Flag = data.Attr("Flag", "PowerFlag");
    }

    public override void Added(Scene scene)

    {
        base.Added(scene);

        var level = SceneAs<Level>();
        if (sprite != null && level != null)
        {
            if (level.Session.GetFlag(Flag))
                sprite.Play("onIdle");
            else
                sprite.Play("offIdle");
        }
    }


    private void OnPlayer(Player player)
    {
        var level = SceneAs<Level>();
        
        if (!opening && !level.Session.GetFlag(Flag))
            foreach (var follower in player.Leader.Followers)
                if (follower.Entity is Key && !(follower.Entity as Key).StartedUsing)
                {
                    TryOpen(player, follower);
                    break;
                }
    }


    private void TryOpen(Player player, Follower fol)
    {
        opening = true;
        (fol.Entity as Key).StartedUsing = true;
        Add(new Coroutine(ActivateRoutine(fol)));
    }


    private IEnumerator ActivateRoutine(Follower fol)
    {
        var emitter = SoundEmitter.Play(unlockSfxName, this);
        emitter.Source.DisposeOnTransition = true;
        var level = SceneAs<Level>();
        var key = fol.Entity as Key;
        Add(new Coroutine(key.UseRoutine(Center + new Vector2(0, -8))));
        yield return 1.2f;
        UnlockingRegistered = true;

        key.RegisterUsed();
        while (key.Turning) yield return null;
        Tag |= Tags.TransitionUpdate;
        level.Session.SetFlag(Flag);


        emitter.Source.DisposeOnTransition = false;
        yield return sprite.PlayRoutine("anim");
        level.Shake();
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
    }
}