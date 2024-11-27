using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Tetrahelper.Entities;

[CustomEntity("Tetrahelper/CrayonBlock")]
[Tracked]
public class CrayonBlock : Solid
{
    public bool Drawn = false;

    public CrayonBlock(EntityData data, Vector2 offset)
        : base(data.Position + offset, data.Width, data.Height, true)
    {
    }

    private List<CrayonBlock> adjacent = new();
    private float wobbleFrom = Calc.Random.NextFloat(6.2831855f);
    private float wobbleTo = Calc.Random.NextFloat(6.2831855f);
    private float wobbleEase;
    private float animTimer;


    public override void Added(Scene scene)
    {
        scene.CollideInto(new Rectangle((int)X, (int)Y - 2, (int)Width, (int)Height + 4), adjacent);
        scene.CollideInto(new Rectangle((int)X - 2, (int)Y, (int)Width + 4, (int)Height), adjacent);


        base.Added(scene);
        if (Drawn)
        {
            Activate();
        }
        else
        {
            Deactivate();
        }
    }

    public void Activate()
    {
        Drawn = true;
        Collidable = true;
    }

    public void Deactivate()
    {
        Drawn = false;
        Collidable = false;
        this.animTimer += 6f * Engine.DeltaTime;
        this.wobbleEase += Engine.DeltaTime * 2f;
        if ((double)this.wobbleEase > 1.0)
        {
            this.wobbleEase = 0.0f;
            this.wobbleFrom = this.wobbleTo;
            this.wobbleTo = Calc.Random.NextFloat(6.2831855f);
        }
    }

    public override void Update()
    {
        base.Update();
        this.animTimer += 6f * Engine.DeltaTime;
        this.wobbleEase += Engine.DeltaTime * 2f;
        if ((double)this.wobbleEase > 1.0)
        {
            this.wobbleEase = 0.0f;
            this.wobbleFrom = this.wobbleTo;
            this.wobbleTo = Calc.Random.NextFloat(6.2831855f);
        }
    }

    public override void Render()
    {
        if (Drawn)
        {
            Draw.Rect(this.X, this.Y, this.Width, this.Height, Color.Red * 0.3f);
            WobbleLine(new Vector2(this.X, this.Y), new Vector2(this.X + this.Width, this.Y), 14f);
            WobbleLine(new Vector2(this.X, this.Y + Height), new Vector2(this.X + this.Width, this.Y + Height), 3f);
            WobbleLine(new Vector2(this.X, this.Y), new Vector2(this.X, this.Y + Height), 2f);
            WobbleLine(new Vector2(this.X + Width, this.Y), new Vector2(this.X + this.Width, this.Y + Height), 5234f);
        }
    }

    private void WobbleLine(Vector2 from, Vector2 to, float offset)
    {
        float lineLength = (to - from).Length();
        Vector2 direction = Vector2.Normalize(to - from);
        Vector2 perpendicular = new Vector2(direction.Y, -direction.X);
        Color lineColor = Color.Red;

        float previousAmplitude = 0.0f;
        int stepSize = 16;
        for (int i = 0; i < lineLength; i += stepSize)
        {
            float currentAmplitude = this.Lerp(this.LineAmplitude(this.wobbleFrom + offset, i),
                this.LineAmplitude(this.wobbleTo + offset, i), this.wobbleEase);
            if (i + stepSize >= lineLength)
                currentAmplitude = 0.0f;
            float segmentLength = Math.Min(stepSize, lineLength - i);
            Vector2 start = from + direction * i + perpendicular * previousAmplitude;
            Vector2 end = from + direction * (i + segmentLength) + perpendicular * currentAmplitude;
            Draw.Line(start, end, lineColor);
            previousAmplitude = currentAmplitude;
        }
    }


    private float Lerp(float a, float b, float percent) => a + (b - a) * percent;

    private float LineAmplitude(float seed, float index) => (float)(Math.Sin((double)seed + (double)index / 16.0 +
                                                                             Math.Sin((double)seed * 2.0 +
                                                                                 (double)index / 32.0) *
                                                                             6.2831854820251465) + 1.0) * 1.5f;
}