using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Celeste.Mod.Entities;

namespace Celeste.Mod.Tetrahelper.Entities
{
    [CustomEntity("Tetrahelper/Crayon")]
    public class Crayon : Entity
    {
        public static ParticleType P_Shimmer;
        public static ParticleType P_Insert;
        public static ParticleType P_Collect;
        public EntityID ID;
        public bool IsUsed;
        public bool StartedUsing;
        private Follower follower;
        private Sprite sprite;
        private Wiggler wiggler;
        private VertexLight light;
        private ParticleEmitter shimmerParticles;
        private float wobble;
        private bool wobbleActive;
        private Tween tween;
        private Alarm alarm;
        private Vector2[] nodes;

        public bool Turning { get; private set; }

        public Crayon(Vector2 position, EntityID id, Vector2[] nodes)
            : base(position)
        {
            this.ID = id;
            this.Collider = (Collider)new Hitbox(12f, 12f, -6f, -6f);
            this.nodes = nodes;
            this.Add((Component)(this.follower = new Follower(id)));
            this.Add((Component)new PlayerCollider(new Action<Player>(this.OnPlayer)));
            this.Add((Component)new MirrorReflection());
            this.Add((Component)(this.sprite = GFX.SpriteBank.Create("Key")));
            this.sprite.CenterOrigin();
            this.sprite.Play("idle");
            this.Add((Component)new TransitionListener()
            {
                OnOut = (Action<float>)(f =>
                {
                    this.StartedUsing = false;
                    if (this.IsUsed)
                        return;
                    if (this.tween != null)
                    {
                        this.tween.RemoveSelf();
                        this.tween = (Tween)null;
                    }

                    if (this.alarm != null)
                    {
                        this.alarm.RemoveSelf();
                        this.alarm = (Alarm)null;
                    }

                    this.Turning = false;
                    this.Visible = true;
                    this.sprite.Visible = true;
                    this.sprite.Rate = 1f;
                    this.sprite.Scale = Vector2.One;
                    this.sprite.Play("idle");
                    this.sprite.Rotation = 0.0f;
                    this.wiggler.Stop();
                    this.follower.MoveTowardsLeader = true;
                })
            });
            this.Add((Component)(this.wiggler = Wiggler.Create(0.4f, 4f,
                (Action<float>)(v =>
                    this.sprite.Scale = Vector2.One * (float)(1.0 + (double)v * 0.3499999940395355)))));
            this.Add((Component)(this.light = new VertexLight(Color.White, 1f, 32, 48)));
        }

        public Crayon(EntityData data, Vector2 offset, EntityID id)
            : this(data.Position + offset, id, data.NodesOffset(offset))
        {
        }

        public Crayon(Player player, EntityID id)
            : this(player.Position + new Vector2((float)(-12 * (int)player.Facing), -8f), id, (Vector2[])null)
        {
            player.Leader.GainFollower(this.follower);
            this.Collidable = false;
            this.Depth = -1000000;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            
        }

        public override void Update()
        {
            if (this.wobbleActive)
            {
                this.wobble += Engine.DeltaTime * 4f;
                this.sprite.Y = (float)Math.Sin((double)this.wobble);
            }

            base.Update();
        }


        private void OnPlayer(Player player)
        {
           
            Audio.Play("event:/game/general/Key_get", this.Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            player.Leader.GainFollower(this.follower);
            this.Collidable = false;
            Session session = this.SceneAs<Level>().Session;
            session.UpdateLevelStartDashes();
            this.wiggler.Start();
            this.Depth = -1000000;
            if (this.nodes == null || this.nodes.Length < 2)
                return;
            this.Add((Component)new Coroutine(this.NodeRoutine(player)));
        }

        private IEnumerator NodeRoutine(Player player)
        {
            Crayon Crayon = this;
            yield return (object)0.3f;
            if (!player.Dead)
            {
                Audio.Play("event:/game/general/cassette_bubblereturn",
                    Crayon.SceneAs<Level>().Camera.Position + new Vector2(160f, 90f));
                player.StartCassetteFly(Crayon.nodes[1], Crayon.nodes[0]);
            }
        }

        public void RegisterUsed()
        {
            this.IsUsed = true;
            if (this.follower.Leader != null)
                this.follower.Leader.LoseFollower(this.follower);
        }

        public IEnumerator UseRoutine(Vector2 target)
        {
            Crayon Crayon = this;
            Crayon.Turning = true;
            Crayon.follower.MoveTowardsLeader = false;
            Crayon.wiggler.Start();
            Crayon.wobbleActive = false;
            Crayon.sprite.Y = 0.0f;
            Vector2 position = Crayon.Position;
            SimpleCurve curve = new SimpleCurve(position, target, (target + position) / 2f + new Vector2(0.0f, -48f));
            Crayon.tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, start: true);
            Crayon.tween.OnUpdate = (Action<Tween>)(t =>
            {
                this.Position = curve.GetPoint(t.Eased);
                this.sprite.Rate = (float)(1.0 + (double)t.Eased * 2.0);
            });
            Crayon.Add((Component)Crayon.tween);
            yield return (object)Crayon.tween.Wait();
            Crayon.tween = (Tween)null;
            while (Crayon.sprite.CurrentAnimationFrame != 4)
                yield return (object)null;
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            Crayon.sprite.Play("enter");
            yield return (object)0.3f;
            Crayon.tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, 0.3f, true);
            Crayon.tween.OnUpdate = (Action<Tween>)(t => this.sprite.Rotation = t.Eased * 1.5707964f);
            Crayon.Add((Component)Crayon.tween);
            yield return (object)Crayon.tween.Wait();
            Crayon.tween = (Tween)null;
            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            Crayon.alarm = Alarm.Set((Entity)Crayon, 1f, (Action)(() =>
            {
                this.alarm = (Alarm)null;
                this.tween = Tween.Create(Tween.TweenMode.Oneshot, start: true);
                this.tween.OnUpdate = (Action<Tween>)(t => this.light.Alpha = 1f - t.Eased);
                this.tween.OnComplete = (Action<Tween>)(t => this.RemoveSelf());
                this.Add((Component)this.tween);
            }));
            yield return (object)0.2f;
            Crayon.sprite.Visible = false;
            Crayon.Turning = false;
        }
    }
}