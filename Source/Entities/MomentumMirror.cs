using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Tetrahelper.Entities;

[CustomEntity("TetraHelper/MomentumMirror")]
[Tracked]
public class MomentumMirror : Entity
{
    // Token: 0x04001000 RID: 4096
    private readonly SoundSource idleSfx;
    private bool activatedthisframe;

    // Token: 0x04000FFF RID: 4095
    private ClimbBlocker climbBlocker;

    // Token: 0x04000FFD RID: 4093
    public Facings Facing;
    private float lastplayerdir;

    private float lastplayerspeed;

    // Token: 0x04001002 RID: 4098
    private bool notCoreMode;

    // Token: 0x04000FFE RID: 4094
    private StaticMover staticMover;

    // Token: 0x04001003 RID: 4099
    private List<Sprite> tiles;

    private bool wasactivatedlastframe;

    // Token: 0x0600169E RID: 5790 RVA: 0x0005F6D8 File Offset: 0x0005D8D8
    public MomentumMirror(Vector2 position, float height, bool left) : base(position)
    {
        Tag = Tags.TransitionUpdate;
        Depth = 1999;

        if (left)
        {
            Facing = Facings.Left;
            Collider = new Hitbox(6f, height);
        }
        else
        {
            Facing = Facings.Right;
            Collider = new Hitbox(6f, height, 2f);
        }

        Add(new PlayerCollider(OnPlayer));

        Add(staticMover = new StaticMover());
        Add(climbBlocker = new ClimbBlocker(false));
        Add(idleSfx = new SoundSource());
        tiles = BuildSprite(left);
    }

    // Token: 0x0600169F RID: 5791 RVA: 0x0005F7B9 File Offset: 0x0005D9B9
    public MomentumMirror(EntityData data, Vector2 offset) : this(data.Position + offset, data.Height,
        data.Bool("left"))
    {
    }

    private bool checkDirection(float dir)
    {
        return (Facing == Facings.Left &&
                (dir >= (100 + 180) % 360 ||
                 dir <= (260 + 180) % 360))
               ||
               (Facing == Facings.Right &&
                dir % 360 >= 100 % 360 &&
                dir % 360 <= 260 % 360
               );
    }

    private void OnPlayer(Player obj)
    {
        var playerdir = obj.Speed.Angle().ToDeg() + 180;
        activatedthisframe = true;

        if ((checkDirection(playerdir) && (Math.Abs(obj.Speed.X) > 12 || Math.Abs(lastplayerspeed) > 12))
            ||
            (checkDirection(lastplayerdir) && (Math.Abs(obj.Speed.X) > 12 || Math.Abs(lastplayerspeed) > 12)
                                           && !wasactivatedlastframe)
           )
        {
            obj.StateMachine.State = 0;
            if ((lastplayerspeed > obj.Speed.X && obj.Speed.X >= 0) ||
                (lastplayerspeed < obj.Speed.X && obj.Speed.X <= 0))
                obj.Speed.X = -1 * lastplayerspeed;
            else
                obj.Speed.X *= -1;


            //obj.Speed.Y -= 10f;

            if (obj.Facing == Facings.Right)
                obj.Facing = Facings.Left;
            else
                obj.Facing = Facings.Right;
            Audio.Play("event:/game/03_resort/forcefield_bump", Position);
            SceneAs<Level>().Particles.Emit(HeartGem.P_BlueShine, 8, obj.Position, Vector2.One * 8f);
            SceneAs<Level>().Displacement.AddBurst(obj.Position, 0.2f, 10f, 28f, 0.2f);
        }
    }


    private List<Sprite> BuildSprite(bool left)
    {
        var list = new List<Sprite>();
        var num = 0;
        while (num < Height)
        {
            string id;
            if (num == 0)
                id = "MomentumMirrorTop";
            else if (num + 16 > Height)
                id = "MomentumMirrorBottom";
            else
                id = "MomentumMirrorMid";
            var sprite = GFX.SpriteBank.Create(id);
            if (!left)
            {
                sprite.FlipX = true;
                sprite.Position = new Vector2(0f, num);
            }
            else
            {
                sprite.Position = new Vector2(0f, num);
            }

            list.Add(sprite);
            Add(sprite);
            num += 8;
        }

        return list;
    }

    // Token: 0x060016A1 RID: 5793 RVA: 0x0005F888 File Offset: 0x0005DA88
    public override void Added(Scene scene)
    {
        base.Added(scene);
    }

    // Token: 0x060016A2 RID: 5794 RVA: 0x0005F8C0 File Offset: 0x0005DAC0
    public override void Render()
    {
        //Draw.Rect(base.Collider, Color.White);
        base.Render();
    }

    // Token: 0x060016A3 RID: 5795 RVA: 0x0005F939 File Offset: 0x0005DB39
    public override void Update()
    {
        PositionIdleSfx();
        if ((Scene as Level).Transitioning) return;
        base.Update();

        lastplayerspeed = Scene.Tracker.GetEntity<Player>().Speed.X;
        lastplayerdir = Scene.Tracker.GetEntity<Player>().Speed.Angle().ToDeg() + 180;

        wasactivatedlastframe = activatedthisframe;
        activatedthisframe = false;
    }

    // Token: 0x060016A4 RID: 5796 RVA: 0x0005F95C File Offset: 0x0005DB5C
    private void PositionIdleSfx()
    {
        var entity = Scene.Tracker.GetEntity<Player>();
        if (entity != null)
        {
            idleSfx.Position = Calc.ClosestPointOnLine(Position, Position + new Vector2(0f, Height), entity.Center) -
                               Position;
            idleSfx.UpdateSfxPosition();
        }
    }
}