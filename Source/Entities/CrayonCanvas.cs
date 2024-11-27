using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Tetrahelper.Entities;

[CustomEntity("Tetrahelper/Canvas")]
internal class Canvas : Entity
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

    public Canvas(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Add(sprite = GFX.SpriteBank.Create("lockdoor_wood"));
        Add(new PlayerCollider(OnPlayer, new Circle(40f)));
        //sprite.Play("offIdle");

        Depth = 1965;
        unlockSfxName = "event:/game/05_mirror_temple/key_unlock_dark";
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);

        var level = SceneAs<Level>();
        if (sprite != null && level != null)
        {
            // if (level.Session.GetFlag(Flag))
            //     sprite.Play("onIdle");
            // else
            //     sprite.Play("offIdle");
        }
    }


    private void OnPlayer(Player player)
    {
        var level = SceneAs<Level>();

        if (!opening)
            foreach (var follower in player.Leader.Followers)
                if (follower.Entity is Crayon crayon && !crayon.StartedUsing)
                {
                    TryOpen(player, follower);
                    break;
                }
    }


    private void TryOpen(Player player, Follower fol)
    {
        opening = true;
        (fol.Entity as Crayon).StartedUsing = true;
        Add(new Coroutine(ActivateRoutine(fol)));
    }


    private IEnumerator ActivateRoutine(Follower fol)
    {
        var emitter = SoundEmitter.Play(unlockSfxName, this);
        emitter.Source.DisposeOnTransition = true;
        var level = SceneAs<Level>();
        var key = fol.Entity as Crayon;
        Add(new Coroutine(key.UseRoutine(Center + new Vector2(0, -8))));
        yield return 0.1f;
        UnlockingRegistered = true;
        
        
        Tag |= Tags.TransitionUpdate;


        emitter.Source.DisposeOnTransition = false;
        yield return 0.5;
        level.Shake();
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);

        var blocks = level.Tracker.GetEntities<CrayonBlock>();
        foreach (var block in blocks){
            
            var crayonBlock = block as CrayonBlock;
            if (crayonBlock != null)
            {
                crayonBlock.Activate();
            }
        }


    }
}