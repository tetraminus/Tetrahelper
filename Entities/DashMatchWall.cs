using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;


namespace Celeste.Mod.TetraHelper.Entities

{
    
	[CustomEntity("TetraHelper/DashMatchWall")]
	[Tracked(false)]
	public class DashMatchWall : Entity
	{
        private bool AllowGreater;
        private int Amount;


        private List<DashMatchWall> adjacent = new List<DashMatchWall>();
        public float Flash;

        public float Solidify;

        public bool Flashing;

        private float solidifyDelay;

        private List<Vector2> particles = new List<Vector2>();

        private float[] speeds = new float[3] { 12f, 20f, 40f };
        public Color ParticleColor;

        public DashMatchWall(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height, data.Int("dashes", 1), data.Bool("GT", false)) { }

            public DashMatchWall(Vector2 position, int width, int height, int dashes, bool greaterthan)
                : base(position)
            {
            //Collidable = false;
            for (int i = 0; (float)i < width * height / 16f; i++)
            {
                particles.Add(new Vector2(Calc.Random.NextFloat(width - 1f), Calc.Random.NextFloat(height - 1f)));
            }

            Depth = Depths.DreamBlocks - 1;
                AllowGreater = greaterthan;
                Amount = dashes;
            switch (dashes) {
                case 0:
                    ParticleColor = Color.LightBlue;
                    break;
                case 1:
                    ParticleColor = Color.Red;
                    break;
                case 2:
                    ParticleColor = Color.Pink;
                    break;

            }
                    
                Collider = new Hitbox(width, height);
                Add(new PlayerCollider(OnPlayer));
            }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Tracker.GetEntity<DashMatchWallRenderer>().Track(this);
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            scene.Tracker.GetEntity<DashMatchWallRenderer>().Untrack(this);
        }

        public override void Update()
        {
            if (Flashing)
            {
                Flash = Calc.Approach(Flash, 0f, Engine.DeltaTime * 4f);
                if (Flash <= 0f)
                {
                    Flashing = false;
                }
            }
            else if (solidifyDelay > 0f)
            {
                solidifyDelay -= Engine.DeltaTime;
            }
            else if (Solidify > 0f)
            {
                Solidify = Calc.Approach(Solidify, 0f, Engine.DeltaTime);
            }
            int num = speeds.Length;
            float height = base.Height;
            int i = 0;
            for (int count = particles.Count; i < count; i++)
            {
                Vector2 value = particles[i] + Vector2.UnitY * speeds[i % num] * Engine.DeltaTime;
                value.Y %= height - 1f;
                particles[i] = value;
            }
            base.Update();
        }


        private void OnPlayer(Player player)

            {
            OnKillEffects();
            if (!player.Dead && player.StateMachine.State != Player.StCassetteFly)
                {
                    if (!AllowGreater)
                    {
                        if (!(player.Dashes == Amount)) { player.Die(Vector2.Zero); }
                    
                    }
                    else
                    {
                        if (!(player.Dashes >= Amount)) { player.Die(Vector2.Zero); }
                    }
                }
            foreach (DashMatchWall item in adjacent)
            {
                if (!item.Flashing)
                {
                    item.OnKillEffects();
                }
            }
            adjacent.Clear();
        }
        public void OnKillEffects()
        {
            Flash = 1f;
            Solidify = 1f;
            solidifyDelay = 1f;
            Flashing = true;
        }

        public override void Render()
        {

            Color color = Color.Pink * 0.5f;
            foreach (Vector2 particle in particles)
            {
                Draw.Pixel.Draw(Position + particle, Vector2.Zero, ParticleColor);
            }
            if (Flashing)
            {
                Draw.Rect(base.Collider, ParticleColor * Flash * 0.5f);
            }
            base.Render();
        }
    }
    }