using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Celeste.Mod.Tetrahelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

    public float SmallWaveAmplitude = 1f;
    public float BigWaveAmplitude = 4f;
    public float CurveAmplitude = 12f;

    private VertexPositionColor[] verts;

    public DashMatchWall(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Width, data.Height, data.Int("dashes", 1), data.Bool("GT"),
            data.Bool("over"))
    {
    }

    public DashMatchWall(Vector2 position, int width, int height, int dashes, bool greaterthan, bool over)
        : base(position)
    {
        BaseParticleColor = GetParticleCol(dashes);
        AltParticleColor = GetParticleCol(dashes + 1);
        if (over)
        {
            Depth = Depths.DreamBlocks - 10;
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

    private float wobbleFrom = Calc.Random.NextFloat(6.2831855f);
    private float wobbleTo = Calc.Random.NextFloat(6.2831855f);
    private float wobbleEase;

    private void WobbleLine(Vector2 from, Vector2 to, float offset)
    {
        float num1 = (to - from).Length();
        Vector2 vector2_1 = Vector2.Normalize(to - from);
        Vector2 vector2_2 = new Vector2(vector2_1.Y, -vector2_1.X);
        Color color1 = this.BaseParticleColor;
        Color color2 = this.BaseParticleColor * Calc.Max(Flash, 0.35f) * 0.5f;


        float num2 = 0.0f;
        int val1 = 16;
        for (int index = 2; (double)index < (double)num1 - 2.0; index += val1)
        {
            float num3 = this.Lerp(this.LineAmplitude(this.wobbleFrom + offset, (float)index),
                this.LineAmplitude(this.wobbleTo + offset, (float)index), this.wobbleEase);
            if ((double)(index + val1) >= (double)num1)
                num3 = 0.0f;
            float num4 = Math.Min((float)val1, num1 - 2f - (float)index);
            Vector2 start = from + vector2_1 * (float)index + vector2_2 * num2;
            Vector2 end = from + vector2_1 * ((float)index + num4) + vector2_2 * num3;
            Draw.Line(start - vector2_2, end - vector2_2, color2);
            Draw.Line(start - vector2_2 * 2f, end - vector2_2 * 2f, color2);
            Draw.Line(start, end, color1);
            num2 = num3;
        }
    }

    private float LineAmplitude(float seed, float index)
    {
        return (float)(Math.Sin((double)seed + (double)index / 16.0 +
                                Math.Sin((double)seed * 2.0 + (double)index / 32.0) * 6.2831854820251465) + 1.0) * 1.5f;
    }

    private float Lerp(float a, float b, float percent) => a + (b - a) * percent;

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

        this.wobbleEase += Engine.DeltaTime * 2f;
        // if ((double)this.wobbleEase > 1.0)
        // {
        //     this.wobbleEase = 0.0f;
        //     this.wobbleFrom = this.wobbleTo;
        //     this.wobbleTo = Calc.Random.NextFloat(6.2831855f);
        // }

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
        //     
        //     // this.WobbleLine(this.shake + new Vector2(this.X, this.Y), this.shake + new Vector2(this.X + this.Width, this.Y), 0.0f);
        // //     this.WobbleLine(this.shake + new Vector2(this.X + this.Width, this.Y), this.shake + new Vector2(this.X + this.Width, this.Y + this.Height), 0.7f);
        // //     this.WobbleLine(this.shake + new Vector2(this.X + this.Width, this.Y + this.Height), this.shake + new Vector2(this.X, this.Y + this.Height), 1.5f);
        // //     this.WobbleLine(this.shake + new Vector2(this.X, this.Y + this.Height), this.shake + new Vector2(this.X, this.Y), 2.5f);
        // //     Draw.Rect(this.shake + new Vector2(this.X, this.Y), 2f, 2f, this.playerHasDreamDash ? DreamBlock.activeLineColor : DreamBlock.disabledLineColor);
        // //     Draw.Rect(this.shake + new Vector2((float) ((double) this.X + (double) this.Width - 2.0), this.Y), 2f, 2f, this.playerHasDreamDash ? DreamBlock.activeLineColor : DreamBlock.disabledLineColor);
        // //     Draw.Rect(this.shake + new Vector2(this.X, (float) ((double) this.Y + (double) this.Height - 2.0)), 2f, 2f, this.playerHasDreamDash ? DreamBlock.activeLineColor : DreamBlock.disabledLineColor);
        // //     Draw.Rect(this.shake + new Vector2((float) ((double) this.X + (double) this.Width - 2.0), (float) ((double) this.Y + (double) this.Height - 2.0)), 2f, 2f, this.playerHasDreamDash ? DreamBlock.activeLineColor : DreamBlock.disabledLineColor);
        // // }
        // //     
        //     
        //     WobbleLine(Position, Position + new Vector2(Width, 0), 0f);
        //     WobbleLine(Position, Position + new Vector2(0, Height), 0.7f);
        //     WobbleLine(Position + new Vector2(Width, 0), Position + new Vector2(Width, Height), 1.5f);
        //     WobbleLine(Position + new Vector2(Width, Height), Position + new Vector2(0, Height), 2.5f);
        //     Draw.Rect(Position, 2f, 2f, BaseParticleColor);
        //     Draw.Rect(Position + new Vector2(Width - 2f, 0), 2f, 2f, BaseParticleColor);
        //     Draw.Rect(Position + new Vector2(0, Height - 2f), 2f, 2f, BaseParticleColor);
        //     Draw.Rect(Position + new Vector2(Width - 2f, Height - 2f), 2f, 2f, BaseParticleColor);
        // DrawWobblyRect(Position + new Vector2(1, 1), new Vector2(Width - 2, Height - 2), true,
        //     BaseParticleColor); //* Calc.Max(Flash, 0.35f) * 0.5f);
        //DrawWobblyRect(Position, new Vector2(Width, Height), false, BaseParticleColor);


        base.Render();
    }

//     private float Wave(int step, float length)
//     {
//         int val = step;
//         float num1 = 1f;
//         float num2 = this.Sin((float)((double)val * 0.25 + (double)wobbleEase * 4.0)) * this.SmallWaveAmplitude +
//                      this.Sin((float)((double)val * 0.05000000074505806 + (double)this.wobbleEase * 0.5)) *
//                      this.BigWaveAmplitude;
//
//         num2 += (1f - Calc.YoYo((float)val / length)) * this.CurveAmplitude;
//         return num2 * num1;
//     }
//
//     private float Sin(float value) => (float)((1.0 + Math.Sin((double)value)) / 2.0);
//
//     private void Quad(ref int vert, Vector2 va, Vector2 vb, Vector2 vc, Vector2 vd, Color color)
//     {
//         this.Quad(ref vert, va, color, vb, color, vc, color, vd, color);
//     }
//
//
//     private void Quad(
//         ref int vert,
//         Vector2 va,
//         Color ca,
//         Vector2 vb,
//         Color cb,
//         Vector2 vc,
//         Color cc,
//         Vector2 vd,
//         Color cd)
//     {
//         this.verts[vert].Position.X = va.X;
//         this.verts[vert].Position.Y = va.Y;
//         this.verts[vert++].Color = ca;
//         this.verts[vert].Position.X = vb.X;
//         this.verts[vert].Position.Y = vb.Y;
//         this.verts[vert++].Color = cb;
//         this.verts[vert].Position.X = vc.X;
//         this.verts[vert].Position.Y = vc.Y;
//         this.verts[vert++].Color = cc;
//         this.verts[vert].Position.X = va.X;
//         this.verts[vert].Position.Y = va.Y;
//         this.verts[vert++].Color = ca;
//         this.verts[vert].Position.X = vc.X;
//         this.verts[vert].Position.Y = vc.Y;
//         this.verts[vert++].Color = cc;
//         this.verts[vert].Position.X = vd.X;
//         this.verts[vert].Position.Y = vd.Y;
//         this.verts[vert++].Color = cd;
//     }
//
//
//    private void Edge(ref int vertexIndex, Vector2 startPoint, Vector2 endPoint, Color col)
// {
//     float edgeLength = (startPoint - endPoint).Length();
//     float minimumInset = 0.0f;
//     float stepCount = edgeLength / 1f;
//     float fadeAmount = 16f;
//     Vector2 perpendicularVector = (endPoint - startPoint).SafeNormalize().Perpendicular();
//     for (int step = 1; step <= stepCount; ++step)
//     {
//         Vector2 interpolatedStart = Vector2.Lerp(startPoint, endPoint, (float)(step - 1) / stepCount);
//         float waveOffsetStart = this.Wave(step - 1, edgeLength);
//         Vector2 offsetStart = perpendicularVector * waveOffsetStart;
//         Vector2 vertexA = interpolatedStart - offsetStart;
//         
//         Vector2 interpolatedEnd = Vector2.Lerp(startPoint, endPoint, (float)step / stepCount);
//         float waveOffsetEnd = this.Wave(step, edgeLength);
//         Vector2 offsetEnd = perpendicularVector * waveOffsetEnd;
//         Vector2 vertexB = interpolatedEnd - offsetEnd;
//         
//         Vector2 clampedStart = Vector2.Lerp(startPoint, endPoint, Calc.ClampedMap((float)(step - 1) / stepCount, 0.0f, 1f, minimumInset, 1f - minimumInset));
//         Vector2 clampedEnd = Vector2.Lerp(startPoint, endPoint, Calc.ClampedMap((float)step / stepCount, 0.0f, 1f, minimumInset, 1f - minimumInset));
//         
//         this.Quad(ref vertexIndex, vertexA + perpendicularVector, col, vertexB + perpendicularVector, col, clampedEnd + perpendicularVector * (fadeAmount - waveOffsetEnd), col, clampedStart + perpendicularVector * (fadeAmount - waveOffsetStart), col);
//         this.Quad(ref vertexIndex, clampedStart + perpendicularVector * (fadeAmount - waveOffsetStart), clampedEnd + perpendicularVector * (fadeAmount - waveOffsetEnd), clampedEnd + perpendicularVector * fadeAmount, clampedStart + perpendicularVector * fadeAmount, col);
//         this.Quad(ref vertexIndex, vertexA, vertexB, vertexB + perpendicularVector * 1f, vertexA + perpendicularVector * 1f, col);
//     }
// }

    // private void DrawWobblyRect(Vector2 pos, Vector2 size, bool fill, Color col)
    // {
    //     //Draw.Rect(pos.X, pos.Y, size.X, size.Y, Color.White);
    //     if (fill)
    //     {
    //         //this.verts = new VertexPositionColor[(int) ((double) size.X / (double)  2.0 + (double) size.Y / (double)  2.0 + 4.0) * 3 * 6 + 6];
    //
    //         this.verts =
    //             new VertexPositionColor[(int)((double)size.X / (double)2.0 + (double)size.Y / (double)2.0 + 4.0) * 3 *
    //                 9 + 6];
    //
    //         //match  with the wobble line
    //
    //
    //         // build using vertices
    //         var level = SceneAs<Level>();
    //
    //
    //         Vector2 vector2_1 = Vector2.Zero;
    //         Vector2 vector2_2 = new Vector2(size.X, 0f);
    //         Vector2 vector2_3 = new Vector2(0f, size.Y);
    //         Vector2 vector2_4 = new Vector2(size.X, size.Y);
    //         Vector2 vector2_5 = new Vector2(size.X / 2f, size.Y / 2f);
    //         var vertCount = 0;
    //
    //         //this.Edge(ref vertCount, Vector2.Zero, new Vector2(size.X, 0f), col);
    //        
    //         this.Edge(ref vertCount, new Vector2(size.X, 0f), new Vector2(size.X, size.Y), col);
    //        
    //         //this.Edge(ref vertCount, vector2_4, vector2_3, col);
    //         
    //         // this.Edge(ref vertCount, vector2_3, Vector2.Zero, vector2_5.X, vector2_5.Y, col);
    //         //this.Quad(ref vertCount, Vector2.Zero + vector2_5, vector2_2 + new Vector2(-vector2_5.X, vector2_5.Y), vector2_4 - vector2_5, vector2_3 + new Vector2(vector2_5.X, -vector2_5.Y), col);
    //
    //         // debug quad
    //        //this.Quad(ref vertCount, Vector2.Zero, col, vector2_2, col, vector2_4, col, vector2_3, col);
    //         
    //         GameplayRenderer.End();
    //         GFX.DrawVertices<VertexPositionColor>(
    //             Matrix.CreateTranslation(new Vector3(this.Position + new Vector2(0.5f), 0.0f)) * level.Camera.Matrix,
    //             verts,
    //             vertCount
    //         );
    //         GameplayRenderer.Begin();
    //     }
    //     else
    //     {
    //         WobbleLine(pos, pos + new Vector2(size.X, 0), 0f);
    //         WobbleLine(pos, pos + new Vector2(0, size.Y), 0.7f);
    //         WobbleLine(pos + new Vector2(size.X, 0), pos + new Vector2(size.X, size.Y), 1.5f);
    //         WobbleLine(pos + new Vector2(size.X, size.Y), pos + new Vector2(0, size.Y), 2.5f);
    //     }
    // }
}