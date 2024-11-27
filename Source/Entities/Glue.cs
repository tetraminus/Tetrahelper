using System.Collections.Generic;
using System.Reflection;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace Celeste.Mod.Tetrahelper.Entities;

[Tracked]
[CustomEntity("tetrahelper/Glue")]
public class Glue : Entity
{
    private static bool playerStuck;

    private static ILHook dashCoroutineHook;
    private readonly ClimbBlocker climbBlocker;
    private readonly SoundSource idleSfx;
    private readonly List<Sprite> tiles;
    public Facings Facing;
    private StaticMover staticMover;

    public Glue(Vector2 position, float height, bool left)
        : base(position)
    {
        Tag = (int)Tags.TransitionUpdate;
        Depth = 1999;

        if (left)
        {
            Facing = Facings.Left;
            Collider = new Hitbox(2f, height);
        }
        else
        {
            Facing = Facings.Right;
            Collider = new Hitbox(2f, height, 6f);
        }

        Add(staticMover = new StaticMover());
        Add(idleSfx = new SoundSource());
        Add(new PlayerCollider(OnPlayer, Collider));


        tiles = BuildSprite(left);
    }


    public Glue(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Height, data.Bool("left"))
    {
    }

    private void OnPlayer(Player p)
    {
        playerStuck = true;
    }

    private List<Sprite> BuildSprite(bool left)
    {
        var spriteList = new List<Sprite>();
        for (var y = 0; y < (double)Height; y += 8)
        {
            var id = y != 0
                ? y + 16 <= (double)Height ? "glueMid" : "glueBottom"
                : "glueTop";
            var sprite = GFX.SpriteBank.Create(id);
            if (!left)
            {
                sprite.FlipX = true;
                sprite.Position = new Vector2(4f, y);
            }
            else
            {
                sprite.Position = new Vector2(0.0f, y);
            }

            spriteList.Add(sprite);
            Add(sprite);
        }

        return spriteList;
    }

    public override void Update()
    {
        PositionIdleSfx();
        if ((Scene as Level).Transitioning)
            return;
        var entity = Scene.Tracker.GetEntity<Player>();
        if (entity != null)
        {
            playerStuck = entity.CollideCheck(this);
            if (playerStuck) entity.Stamina = 110.0f;
        }


        base.Update();
    }

    private void PositionIdleSfx()
    {
        var entity = Scene.Tracker.GetEntity<Player>();
        if (entity == null)
            return;
        idleSfx.Position =
            Calc.ClosestPointOnLine(Position, Position + new Vector2(0.0f, Height), entity.Center) -
            Position;
        idleSfx.UpdateSfxPosition();
    }

    public static void Load()
    {
        IL.Celeste.Player.NormalUpdate += modInputGrabCheck;
        IL.Celeste.Player.ClimbUpdate += modInputGrabCheck;
        IL.Celeste.Player.DashUpdate += modInputGrabCheck;
        IL.Celeste.Player.DashCoroutine += modInputGrabCheck;
        IL.Celeste.Player.SwimUpdate += modInputGrabCheck;
        IL.Celeste.Player.RedDashUpdate += modInputGrabCheck;
        IL.Celeste.Player.HitSquashUpdate += modInputGrabCheck;
        IL.Celeste.Player.LaunchUpdate += modInputGrabCheck;
        IL.Celeste.Player.DreamDashUpdate += modInputGrabCheck;
        IL.Celeste.Player.StarFlyUpdate += modInputGrabCheck;

        dashCoroutineHook =
            new ILHook(
                typeof(Player).GetMethod("DashCoroutine", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetStateMachineTarget(), modInputGrabCheck);
    }

    public static void Unload()
    {
        IL.Celeste.Player.NormalUpdate -= modInputGrabCheck;
        IL.Celeste.Player.ClimbUpdate -= modInputGrabCheck;
        IL.Celeste.Player.DashUpdate -= modInputGrabCheck;
        IL.Celeste.Player.DashCoroutine -= modInputGrabCheck;
        IL.Celeste.Player.SwimUpdate -= modInputGrabCheck;
        IL.Celeste.Player.RedDashUpdate -= modInputGrabCheck;
        IL.Celeste.Player.HitSquashUpdate -= modInputGrabCheck;
        IL.Celeste.Player.LaunchUpdate -= modInputGrabCheck;
        IL.Celeste.Player.DreamDashUpdate -= modInputGrabCheck;
        IL.Celeste.Player.StarFlyUpdate -= modInputGrabCheck;

        if (dashCoroutineHook != null) dashCoroutineHook.Dispose();
    }

    private static void modInputGrabCheck(ILContext il)
    {
        var cursor = new ILCursor(il);

        // mod all Input.Grab.Check
        while (cursor.TryGotoNext(MoveType.Before,
                   instr => instr.MatchLdsfld(typeof(Input), "Grab"),
                   instr => instr.MatchCallvirt<VirtualButton>("get_Check")
               ))
            cursor.GotoNext().Remove()
                .EmitDelegate<Everest.LuaLoader.HookHelper.Func<VirtualButton, bool>>(invertButtonCheck);

        cursor.Index = 0;
        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall(typeof(Input), "get_GrabCheck")))
            cursor.GotoNext().EmitDelegate<Everest.LuaLoader.HookHelper.Func<bool, bool>>(invertButtonCheck);
    }

    private static bool invertButtonCheck(VirtualButton button)
    {
        return playerStuck || button.Check;
    }

    private static bool invertButtonCheck(bool buttonCheck)
    {
        return playerStuck || buttonCheck;
    }
}