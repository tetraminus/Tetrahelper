using System;
using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Tetrahelper.Entities;

[CustomEntity("Tetrahelper/WhiteBerry")]
[RegisterStrawberry(false, false)]
internal class WhiteBerry : Entity, IStrawberry
{
    // Particle references. These are the glittery bits that trail behind you when you're carrying a Strawberry.
    // You can set these during your EverestModule.Load() or LoadContent() command.
    public static ParticleType P_Glow = Strawberry.P_Glow;
    public static ParticleType P_GhostGlow = Strawberry.P_GhostGlow;

    private static SpriteBank spriteBank;
    private readonly bool isOwned;
    private readonly Vector2 start;
    private BloomPoint bloom;
    private bool collected;
    private float collectTimer;
    public Follower Follower;

    // Common to every strawberry, that may need to be read from other sources.
    public EntityID ID;
    private VertexLight light;
    private Tween lightTween;

    // Internals common to every strawberry. It's not likely that you'll need these outside of your custom berry.
    private Sprite sprite;
    private Wiggler wiggler;
    private float wobble;

    public WhiteBerry(EntityData data, Vector2 offset, EntityID gid)
    {
        ID = gid;
        Position = start = data.Position + offset;

        isOwned = SaveData.Instance.CheckStrawberry(ID);
        Depth = -100;
        Collider = new Hitbox(14f, 14f, -7f, -7f);
        Add(new PlayerCollider(OnPlayer));
        Add(new MirrorReflection());
        Add(Follower = new Follower(ID, null, OnLoseLeader));
        Follower.FollowDelay = 0.3f;
    }


    // Routine to "break" the berry and then respawn it


    // Requested implementation for using IStrawberry.
    public void OnCollect()
    {
        // Bail if we're already "collected".
        if (collected)
            return;

        collected = true;

        // This is used for the ascending "score" effect and even leads to 1UPs!
        var collectIndex = 0;

        if (Follower.Leader != null)
        {
            var player = Follower.Leader.Entity as Player;
            collectIndex = player.StrawberryCollectIndex;
            player.StrawberryCollectIndex++;
            player.StrawberryCollectResetTimer = 2.5f;
            Follower.Leader.LoseFollower(Follower);
        }

        // Save the Strawberry. It's not a "secret" berry, so let's not say it's "golden" for the savedata.
        SaveData.Instance.AddStrawberry(ID, false);

        // Make the Strawberry not load any more in this Session.
        var session = SceneAs<Level>().Session;
        session.DoNotLoad.Add(ID);
        session.Strawberries.Add(ID);
        session.UpdateLevelStartDashes();

        // Coroutines allow certain processes to run independently, so to speak.
        Add(new Coroutine(CollectRoutine(collectIndex)));
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);

        sprite = GFX.SpriteBank.Create("WhiteBerry");
        Add(sprite);

        sprite.Play("idle");

        // Strawberries have certain special effects during their animation sequence.
        // This adds a handler to enable this.
        sprite.OnFrameChange = OnAnimate;

        // A Wiggler is capable of "shaking" and "pulsing" sprites.
        // This Wiggler adjusts the sprite's Scale when triggered.
        wiggler = Wiggler.Create
        (
            0.4f,
            4f,
            delegate(float v) { sprite.Scale = Vector2.One * (1f + v * 0.35f); }
        );
        Add(wiggler);

        // Bloom makes bright things brighter!
        // The default BloomPoint for a vanilla Strawberry is
        // alpha = (this.Golden || this.Moon || this.isGhostBerry) ? 0.5f : 1f
        // radius = 12f
        bloom = new BloomPoint(isOwned ? 0.25f : 0.5f, 12f);
        Add(bloom);

        // Strawberries give off light. This is the vanilla VertexLight.
        light = new VertexLight(Color.White, 1f, 16, 24);
        lightTween = light.CreatePulseTween();
        Add(light);
        Add(lightTween);

        // While we're here, a seeded Strawberry must be allowed to initialize its Seeds.


        // Let's be polite and turn down the bloom a little bit if the base level has bloom.
        if (SceneAs<Level>().Session.BloomBaseAdd > 0.1f)
            bloom.Alpha *= 0.5f;
    }

    // Every Entity needs an Update sequence. This is where we do the bulk of our checking for things.
    public override void Update()
    {
        // Let's not do anything if the Strawberry is waiting for seeds.


        if (!collected)
        {
            // Subtle up-and-down movement sequence.
            wobble += Engine.DeltaTime * 4f;

            if (sprite != null)
            {
                sprite.Y = (float)Math.Sin(wobble) * 2f;
                sprite.X = (float)Math.Cos(wobble * .2f);
            }

            // We'll check collection rules for our strawberry here. It's standard collection rules, so...
            if (Follower.Leader != null)
            {
                var player = Follower.Leader.Entity as Player;

                // First in line of the normal-collection train?
                if (Follower.DelayTimer <= 0f && StrawberryRegistry.IsFirstStrawberry(this))
                {
                    if (player != null && player.Scene != null &&
                        !player.StrawberriesBlocked && player.OnSafeGround &&
                        player.StateMachine.State != 13)
                    {
                        // lot of checks!
                        collectTimer += Engine.DeltaTime;
                        if (collectTimer > 0.15f)
                            OnCollect();
                    }
                    else
                    {
                        collectTimer = Math.Min(collectTimer, 0f);
                    }
                }
                // Not first in line?
                else if (Follower.FollowIndex > 0)
                {
                    collectTimer = -0.15f;
                }
            }
        }

        // This spawns glittery particles if we're carrying the berry!
        if (Follower.Leader != null && Scene.OnInterval(0.08f))
        {
            ParticleType type;
            if (!isOwned)
                type = P_Glow;
            else
                type = P_GhostGlow;

            SceneAs<Level>().ParticlesFG.Emit(type, Position + Calc.Random.Range(-Vector2.One * 6f, Vector2.One * 6f));
        }

        base.Update();
    }

    // From our OnFrameChange handler assigned in Added, this routine allows us to run additional effects based on the animation frame.
    private void OnAnimate(string id)
    {
        // Strawberries play a sound and a little extra "burst" on a specific animation frame.
        // Since this is a unique animation, we'll targeting the middle frame of the "sheen" animation.
        var numFrames = 35; // 4 loops of 0-6, 1 loop of 7-13.

        if (sprite.CurrentAnimationFrame == numFrames - 4)
        {
            lightTween.Start();

            // Use a different pulse effect if the berry is visually obstructed.
            // Don't play it if OnCollect was successfully called.
            var visuallyObstructed = CollideCheck<FakeWall>() || CollideCheck<Solid>();
            if (!collected && visuallyObstructed)
            {
                Audio.Play("event:/game/general/strawberry_pulse", Position);
                SceneAs<Level>().Displacement.AddBurst(Position, 0.6f, 4f, 28f, 0.1f);
            }
            else
            {
                Audio.Play("event:/game/general/strawberry_pulse", Position);
                SceneAs<Level>().Displacement.AddBurst(Position, 0.6f, 4f, 28f, 0.2f);
            }
        }
    }

    // We added a PlayerCollider handler in the constructor. This routine is called when it is triggered.
    public void OnPlayer(Player player)
    {
        // Bail if we're not ready to be picked up, or are already picked up.
        if (Follower.Leader != null || collected)
            return;

        // Let's play a pickup sound and trigger the Wiggler to make the strawberry temporarily change size when we touch it.
        Audio.Play(isOwned ? "event:/game/general/strawberry_blue_touch" : "event:/game/general/strawberry_touch",
            Position);
        player.Leader.GainFollower(Follower);
        wiggler.Start();
        Depth = -1000000;
    }

    private IEnumerator CollectRoutine(int collectIndex)
    {
        var level = SceneAs<Level>();
        Tag = Tags.TransitionUpdate;
        Depth = -2000010;

        // Use "yellow" text for a new berry, "blue" for an owned berry.
        // Plays the appropriate sounds, too.
        var color = !isOwned ? 0 : 1;
        Audio.Play("event:/game/general/strawberry_get", Position, "colour", color, "count", collectIndex);
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);

        if (!isOwned)
            sprite.Play("collect");
        else
            sprite.Play("collect");

        while (sprite.Animating) yield return null;
        Scene.Add(new StrawberryPoints(Position, isOwned, collectIndex, false));
        RemoveSelf();
    }

    // When the Strawberry's Follower loses its Leader (basically, when you die),
    // the Strawberry performs this action to smoothly return home.
    // If you detach a Strawberry and need it to be collectable again,
    // make sure to re-enable its collision in tween.OnComplete's delegate.
    private void OnLoseLeader()
    {
        if (collected)
            return;

        Alarm.Set(this, 0.15f, delegate
        {
            var vector = (start - Position).SafeNormalize();
            var num = Vector2.Distance(Position, start);
            var scaleFactor = Calc.ClampedMap(num, 16f, 120f, 16f, 96f);
            var control = start + vector * 16f + vector.Perpendicular() * scaleFactor * Calc.Random.Choose(1, -1);
            var curve = new SimpleCurve(Position, start, control);
            var tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineOut, MathHelper.Max(num / 100f, 0.4f), true);
            tween.OnUpdate = delegate(Tween f) { Position = curve.GetPoint(f.Eased); };
            tween.OnComplete = delegate { Depth = 0; };
            Add(tween);
        });
    }

    // Requested implementation for using IStrawberrySeeded.
    // You'll generally want to make a seeded berry "visible" here.
    // Additionally, set the current Session's flag for the seeds to true.


    // Initialize the spritebank.


    public static void Unload()
    {
    }
}