using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Tetrahelper.Entities;

// Token: 0x020002A9 RID: 681
public class TimeWarpCutscene : CutsceneEntity
{
    // Token: 0x04000D8F RID: 3471
    private readonly Player player;

    // Token: 0x04000D90 RID: 3472
    private readonly TimeWarpPortal portal;

    // Token: 0x060013CC RID: 5068 RVA: 0x0004F09A File Offset: 0x0004D29A
    private readonly string target;

    // Token: 0x04000D91 RID: 3473
    private Fader fader;

    // Token: 0x04000D92 RID: 3474
    private SoundSource sfx;

    public TimeWarpCutscene(Player player, TimeWarpPortal portal, string targetRoom)
    {
        this.player = player;
        this.portal = portal;
        target = targetRoom;
    }

    // Token: 0x060013CD RID: 5069 RVA: 0x0004F0B4 File Offset: 0x0004D2B4
    public override void OnBegin(Level level)
    {
        Add(new Coroutine(Cutscene(level)));
        level.Add(fader = new Fader());
    }

    // Token: 0x060013CE RID: 5070 RVA: 0x0004F0E8 File Offset: 0x0004D2E8
    private IEnumerator Cutscene(Level level)
    {
        player.StateMachine.State = 11;
        player.StateMachine.Locked = true;
        player.Dashes = 1;
        if (level.Session.Area.Mode == AreaMode.Normal)
            Audio.SetMusic(null);
        else
            Add(new Coroutine(MusicFadeOutBSide()));
        Add(sfx = new SoundSource());
        sfx.Position = portal.Center;
        sfx.Play("event:/music/lvl5/mirror_cutscene");
        Add(new Coroutine(CenterCamera()));
        yield return player.DummyWalkToExact((int)portal.X);
        yield return 0.25f;
        yield return player.DummyWalkToExact((int)portal.X - 16);
        yield return 0.5f;
        yield return player.DummyWalkToExact((int)portal.X + 16);
        yield return 0.25f;
        player.Facing = Facings.Left;
        yield return 0.25f;
        yield return player.DummyWalkToExact((int)portal.X);
        yield return 0.1f;
        Input.Rumble(RumbleStrength.Strong, RumbleLength.FullSecond);
        while (portal.portalSpeed < 8f)
        {
            Level.Shake(.5f);
            portal.portalSpeed += 0.1f;
            yield return null;
        }

        player.DummyAutoAnimate = false;
        player.Sprite.Play("lookUp");
        yield return 1f;
        player.DummyAutoAnimate = true;
        portal.Activate();
        Add(new Coroutine(level.ZoomTo(new Vector2(160f, 90f), 3f, 12f)));
        yield return 0.25f;
        player.ForceStrongWindHair.X = -1f;
        yield return player.DummyWalkToExact((int)player.X + 12, true);
        yield return 0.5f;
        player.Facing = Facings.Right;
        player.DummyAutoAnimate = false;
        player.DummyGravity = false;
        player.Sprite.Play("runWind");
        while (player.Sprite.Rate > 0f)
        {
            player.MoveH(player.Sprite.Rate * 10f * Engine.DeltaTime);
            player.MoveV(-(1f - player.Sprite.Rate) * 6f * Engine.DeltaTime);
            player.Sprite.Rate -= Engine.DeltaTime * 0.15f;
            yield return null;
        }

        yield return 0.5f;
        player.Sprite.Play("fallFast");
        player.Sprite.Rate = 1f;
        var target = portal.Center + new Vector2(0f, 8f);
        var from = player.Position;
        for (var p = 0f; p < 1f; p += Engine.DeltaTime * 2f)
        {
            player.Position = from + (target - from) * Ease.SineInOut(p);
            yield return null;
        }

        player.ForceStrongWindHair.X = 0f;
        target = default;
        from = default;
        //this.fader.Target = 1f;
        yield return 2f;
        //this.player.Sprite.Play("sleep", false, false);
        yield return 1f;
        yield return level.ZoomBack(1f);

        level.Session.ColorGrade = "templevoid";
        for (var p = 0f; p < 1f; p += Engine.DeltaTime)
        {
            Glitch.Value = p * 0.05f;
            level.ScreenPadding = 32f * p;
            yield return null;
        }

        while ((portal.DistortionFade -= Engine.DeltaTime * 2f) > 0f) yield return null;
        EndCutscene(level);
    }

    // Token: 0x060013CF RID: 5071 RVA: 0x0004F0FE File Offset: 0x0004D2FE
    private IEnumerator CenterCamera()
    {
        var camera = Level.Camera;
        var target = portal.Center - new Vector2(160f, 90f);
        while ((camera.Position - target).Length() > 1f)
        {
            camera.Position += (target - camera.Position) *
                               (1f - (float)Math.Pow(0.009999999776482582, Engine.DeltaTime));
            yield return null;
        }
    }

    // Token: 0x060013D0 RID: 5072 RVA: 0x0004F10D File Offset: 0x0004D30D
    private IEnumerator MusicFadeOutBSide()
    {
        for (var p = 1f; p > 0f; p -= Engine.DeltaTime)
        {
            Audio.SetMusicParam("fade", p);
            yield return null;
        }

        Audio.SetMusicParam("fade", 0f);
    }

    // Token: 0x060013D1 RID: 5073 RVA: 0x0004F118 File Offset: 0x0004D318
    public override void OnEnd(Level level)
    {
        level.OnEndOfFrame += delegate
        {
            if (fader != null && !WasSkipped)
            {
                fader.Tag = Tags.Global;
                fader.Target = 0f;
                fader.Ended = true;
            }

            Leader.StoreStrawberries(player.Leader);
            level.Remove(player);
            level.UnloadLevel();

            level.Session.Keys.Clear();


            level.Session.Level = target;
            level.Session.RespawnPoint = level.GetSpawnPoint(new Vector2(level.Bounds.Left, level.Bounds.Top));
            level.LoadLevel(Player.IntroTypes.WakeUp);
            Audio.SetMusicParam("fade", 1f);
            level.Session.ColorGrade = "none";
            Leader.RestoreStrawberries(level.Tracker.GetEntity<Player>().Leader);
            level.Camera.Y -= 8f;
            Glitch.Value = 0;
            if (!WasSkipped && level.Wipe != null) level.Wipe.Cancel();
            if (fader != null) fader.RemoveTag(Tags.Global);
        };
    }

    // Token: 0x020002AA RID: 682
    private class Fader : Entity
    {
        // Token: 0x04000D94 RID: 3476
        public bool Ended;

        // Token: 0x04000D95 RID: 3477
        private float fade;

        // Token: 0x04000D93 RID: 3475
        public float Target;

        // Token: 0x060013D2 RID: 5074 RVA: 0x0004F150 File Offset: 0x0004D350
        public Fader()
        {
            Depth = -1000000;
        }

        // Token: 0x060013D3 RID: 5075 RVA: 0x0004F164 File Offset: 0x0004D364
        public override void Update()
        {
            fade = Calc.Approach(fade, Target, Engine.DeltaTime * 0.5f);
            if (Target <= 0f && fade <= 0f && Ended) RemoveSelf();
            base.Update();
        }

        // Token: 0x060013D4 RID: 5076 RVA: 0x0004F1C4 File Offset: 0x0004D3C4
        public override void Render()
        {
            var camera = (Scene as Level).Camera;
            if (fade > 0f) Draw.Rect(camera.X - 10f, camera.Y - 10f, 340f, 200f, Color.Black * fade);
            var entity = Scene.Tracker.GetEntity<Player>();
            if (entity != null && !entity.OnGround(2)) entity.Render();
        }
    }
}