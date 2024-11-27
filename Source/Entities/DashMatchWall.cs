using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.TetraHelper.Entities;

public class Particle
{
    public Color Col;
    public Vector2 position;

    public Particle(float X, float Y, Color color)
    {
        position.X = X;
        position.Y = Y;
        Col = color;
    }
}

[CustomEntity("TetraHelper/DashMatchWall")]
[Tracked]
public class DashMatchWall : Entity
{
    private readonly List<DashMatchWall> adjacent = new();
    private readonly bool AllowGreater;
    private readonly int Amount;

    private readonly List<Particle> particles = new();

    private readonly float[] speeds = new float[6] { 12f, 20f, 40f, -12f, -20f, -40f };
    public Color AltParticleColor;
    public Color BaseParticleColor;

    public float Flash;

    public bool Flashing;

    public float Solidify;

    private float solidifyDelay;

    public DashMatchWall(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Width, data.Height, data.Int("dashes", 1), data.Bool("GT"), data.Bool("over"))
    {
    }

    public DashMatchWall(Vector2 position, int width, int height, int dashes, bool greaterthan, bool over)
        : base(position)
    {
        BaseParticleColor = GetParticleCol(dashes);
        AltParticleColor = GetParticleCol(dashes + 1);
        if (over)
        {
            Depth = Depths.DreamBlocks-10;
        }
        else
        {
            Depth = Depths.Solids;
        }
        AllowGreater = greaterthan;
        Amount = dashes;
        for (var i = 0; i < width * height / 16f; i++)
            particles.Add(new Particle(Calc.Random.NextFloat(width - 1f), Calc.Random.NextFloat(height - 1f),
                i % 2 == 1 && AllowGreater ? AltParticleColor : BaseParticleColor));


        Collider = new Hitbox(width, height);
        Add(new PlayerCollider(OnPlayer));
    }

    public Color GetParticleCol(int d)
    {
        if (SaveData.Instance != null && SaveData.Instance.Assists.PlayAsBadeline)
            return d switch
            {
                0 => Player.UsedBadelineHairColor,
                1 => Player.NormalBadelineHairColor,
                2 => Player.TwoDashesBadelineHairColor,
                _ => Color.Green
            };
        return d switch
        {
            0 => Player.UsedHairColor,
            1 => Player.NormalHairColor,
            2 => Player.TwoDashesHairColor,
            _ => Color.Green
        };
    }
    //public override void Added(Scene scene)
    //{
    //    base.Added(scene);
    //    scene.Tracker.GetEntity<DashMatchWallRenderer>().Track(this);
    //}

    //public override void Removed(Scene scene)
    //{
    //    base.Removed(scene);
    //    scene.Tracker.GetEntity<DashMatchWallRenderer>().Untrack(this);
    //}

    public override void Update()
    {
        if (Flashing)
        {
            Flash = Calc.Approach(Flash, 0f, Engine.DeltaTime * 4f);
            if (Flash <= 0f) Flashing = false;
        }
        else if (solidifyDelay > 0f)
        {
            solidifyDelay -= Engine.DeltaTime;
        }
        else if (Solidify > 0f)
        {
            Solidify = Calc.Approach(Solidify, 0f, Engine.DeltaTime);
        }

        var height = Height;
        var i = 0;
        for (var count = particles.Count; i < count; i++)
        {
            var speed = speeds[i % speeds.Length];
            var value = particles[i].position;
            value.Y -= speed * Engine.DeltaTime;
            if (value.Y <= 0) value.Y = height - 2f;
            value.Y %= height - 1f;
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

        foreach (var item in adjacent)
            if (!item.Flashing)
                item.OnKillEffects();
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
        foreach (var particle in particles)
        {
            if (AllowGreater)
            {
            }

            Draw.Pixel.Draw(Position + particle.position, Vector2.Zero, particle.Col);
        }

        if (Flashing)
            Draw.Rect(Collider, BaseParticleColor * Calc.Max(Flash, 0.35f) * 0.5f);
        else
            Draw.Rect(Collider, BaseParticleColor * 0.35f * 0.5f);
        base.Render();
    }
}