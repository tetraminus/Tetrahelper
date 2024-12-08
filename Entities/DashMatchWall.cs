using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;


namespace Celeste.Mod.TetraHelper.Entities

{
    public class Particle
    {
        public Vector2 position;
        public Color Col;
        public Particle(float X,float Y, Color color)
        {
            position.X = X;
            position.Y = Y;
            Col = color;
        }

    }
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

        private List<Particle> particles = new List<Particle>();

        private float[] speeds = new float[6] { 12f, 20f, 40f, -12f, -20f, -40f };
        public Color BaseParticleColor;
        public Color AltParticleColor;

        public DashMatchWall(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height, data.Int("dashes", 1), data.Bool("GT", false)) { }

        public DashMatchWall(Vector2 position, int width, int height, int dashes, bool greaterthan)
            : base(position)
        {
            BaseParticleColor = GetParticleCol(dashes);
            AltParticleColor = GetParticleCol(dashes+1);
            Depth = Depths.CrystalSpinners - 1;
            AllowGreater = greaterthan;
            Amount = dashes;
            for (int i = 0; (float)i < width * height / 16f; i++)
            {
                particles.Add(new Particle(Calc.Random.NextFloat(width - 1f), Calc.Random.NextFloat(height - 1f), (((i%2 == 1) && AllowGreater)? AltParticleColor: BaseParticleColor)));
            }

            
           
            

            Collider = new Hitbox(width, height);
            Add(new PlayerCollider(OnPlayer));
        }
        public Color GetParticleCol(int d)
        {
            if (SaveData.Instance != null && SaveData.Instance.Assists.PlayAsBadeline)
            {
                return d switch
                {
                    0 => Player.UsedBadelineHairColor,
                    1 => Player.NormalBadelineHairColor,
                    2 => Player.TwoDashesBadelineHairColor,
                    _ => Color.Green,
                };
            }
            else
            {
                return d switch
                {
                    0 => Player.UsedHairColor,
                    1 => Player.NormalHairColor,
                    2 => Player.TwoDashesHairColor,
                    _ => Color.Green,
                };
            }
        }
        private void WobbleLine(Vector2 from, Vector2 to, float offset)
        {
            float num1 = (to - from).Length();
            Vector2 vector2_1 = Vector2.Normalize(to - from);
            Vector2 vector2_2 = new Vector2(vector2_1.Y, -vector2_1.X);
            Color color1 = this.playerHasDreamDash ? DreamBlock.activeLineColor : DreamBlock.disabledLineColor;
            Color color2 = this.playerHasDreamDash ? DreamBlock.activeBackColor : DreamBlock.disabledBackColor;
            if ((double) this.whiteFill > 0.0)
            {
                color1 = Color.Lerp(color1, Color.White, this.whiteFill);
                color2 = Color.Lerp(color2, Color.White, this.whiteFill);
            }
            float num2 = 0.0f;
            int val1 = 16;
            for (int index = 2; (double) index < (double) num1 - 2.0; index += val1)
            {
                float num3 = this.Lerp(this.LineAmplitude(this.wobbleFrom + offset, (float) index), this.LineAmplitude(this.wobbleTo + offset, (float) index), this.wobbleEase);
                if ((double) (index + val1) >= (double) num1)
                    num3 = 0.0f;
                float num4 = Math.Min((float) val1, num1 - 2f - (float) index);
                Vector2 start = from + vector2_1 * (float) index + vector2_2 * num2;
                Vector2 end = from + vector2_1 * ((float) index + num4) + vector2_2 * num3;
                Draw.Line(start - vector2_2, end - vector2_2, color2);
                Draw.Line(start - vector2_2 * 2f, end - vector2_2 * 2f, color2);
                Draw.Line(start, end, color1);
                num2 = num3;
            }
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
            
            float height = base.Height;
            int i = 0;
            for (int count = particles.Count; i < count; i++)
            {

                float speed = speeds[i % speeds.Length];
                Vector2 value = particles[i].position;
                value.Y -= speed * Engine.DeltaTime;
                if (value.Y <= 0){ value.Y = height - 2f; }
                value.Y %= height -1f ;
                 particles[i].position = value;
            }
            base.Update();
        }


        private void OnPlayer(Player player)

        {

            if (!player.Dead && player.StateMachine.State != Player.StCassetteFly)
            {
                if (AllowGreater)
                {
                   
                    if (!(player.Dashes >= Amount))
                    {
                        player.Die(Vector2.Zero);
                        OnKillEffects();
                    }
                }
                else
                {
                    if (!(player.Dashes == Amount))
                    {
                        player.Die(Vector2.Zero);
                        OnKillEffects();
                    }
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
            
            
            foreach (Particle particle in particles)
            {
                if (AllowGreater) { }
                Draw.Pixel.Draw(Position + particle.position, Vector2.Zero, particle.Col);
            }
            if (Flashing)
            {
                Draw.Rect(base.Collider, BaseParticleColor * Calc.Max(Flash,0.35f) * 0.5f);
            }
            else
            { Draw.Rect(base.Collider, BaseParticleColor * 0.35f * 0.5f); }
            base.Render();
        }
    }
}