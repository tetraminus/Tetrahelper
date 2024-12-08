using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Celeste.Mod.Tetrahelper.helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.Tetrahelper.Entities;

// Token: 0x0200061E RID: 1566
[CustomEntity("Tetrahelper/TimeWarpPortal")]
public class TimeWarpPortal : Entity
{
    private readonly string onFlag;

    private readonly int particlerange = 64;
    private readonly List<Particle> particles = new();

    private readonly string targetroom;

    // Token: 0x040020B5 RID: 8373
    private float bufferAlpha;

    // Token: 0x040020B6 RID: 8374
    private float bufferTimer;

    // Token: 0x040020B2 RID: 8370
    private bool canTrigger;
    private VirtualRenderTarget circlebuffer;

    // Token: 0x040020B7 RID: 8375
    private Debris[] debris;

    // Token: 0x040020B8 RID: 8376
    private Color debrisColorFrom;

    // Token: 0x040020B9 RID: 8377
    private Color debrisColorTo;

    // Token: 0x040020BA RID: 8378
    private MTexture debrisTexture;

    // Token: 0x040020B1 RID: 8369
    public float DistortionFade;


    // Token: 0x040020BC RID: 8380
    private TemplePortalTorch leftTorch;

    // Token: 0x040020B4 RID: 8372
    private VirtualRenderTarget portalbuffer;

    public float portalSpeed;

    // Token: 0x040020BD RID: 8381
    private TemplePortalTorch rightTorch;

    // Token: 0x040020B3 RID: 8371
    private int switchCounter;

    private Texture2D tex;
    

    public TimeWarpPortal(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        onFlag = data.Attr("Flag");

        targetroom = data.Attr("Target");
        Everest.Events.Level.OnTransitionTo += CheckEvent;
        Position = Position;
        Depth = 2000;
        Collider = new Circle(23);
        Add(new PlayerCollider(OnPlayer));
        canTrigger = true;
        portalSpeed = 1f;
    }


    private void CheckEvent(Level level, LevelData next, Vector2 direction)

    {
    }

    // Token: 0x060026FE RID: 9982 RVA: 0x000D9830 File Offset: 0x000D7A30
    public override void Added(Scene scene)
    {
        base.Added(scene);

        Add(new VertexLight(Vector2.Zero, Color.White * 0.5f, 1f, 1, 1));
        Add(new BloomPoint(new Vector2(0, 0), .2f, 32));
        //this.Add(new BloomPoint(new Vector2(0,30), 3f, 15));
        //this.Add(new BloomPoint(new Vector2(0,-30), 3f, 15));

        Add(new BeforeRenderHook(BeforeRender));
        bufferAlpha = 1f;
        for (var i = 0; i < 50; i++)
        {
            var TempPos = new Vector2(Position.X + Calc.Random.Next(-particlerange, particlerange),
                Position.Y + Calc.Random.Next(-particlerange, particlerange));

            var p = new Particle(TempPos, Color.White);
            float dir = Calc.Random.Next(360);
            dir = dir.ToRad();

            p.vel = Vector2.Normalize(Vector2.Normalize(new Vector2((float)Math.Cos(dir), (float)Math.Sin(dir)))) * 6;


            particles.Add(p);
        }
    }

    // Token: 0x060026FF RID: 9983 RVA: 0x000D98CB File Offset: 0x000D7ACB


    // Token: 0x06002701 RID: 9985 RVA: 0x000D98F6 File Offset: 0x000D7AF6

    public void Activate()
    {
    }

    
    
    // this is only possible because of horizon_wings#4385 on the celeste discord
    private void BeforeRender()
    {
        bufferTimer += portalSpeed * Engine.DeltaTime;
        StencilMaker.SwapToStencilTarget();
        var position = new Vector2(StencilMaker.GetStencilTexture().Width, StencilMaker.GetStencilTexture().Height) /
                       2f;
        var mtexture = GFX.Game["objects/temple/portal/portal"];
        
        StencilMaker.StartMask();
        // GFX.Game["objects/common/warpportal/mask"]
        //     .DrawCentered(position, MultAlpha(Color.DarkBlue, .1f), new Vector2(1.5f));
    
        CircleShape.RawDraw(position, 26, Color.Transparent, 0f);
        
        StencilMaker.EndMask();
        StencilMaker.StartContent();
        
        var num = 0;
        while (num < 10f)
        {
            var num2 = bufferTimer % 1f * 0.1f + num / 10f;
            var color = Color.Lerp(MultAlpha(Color.DarkBlue, .2f * num2), MultAlpha(Color.SkyBlue, .2f * num2), num2);
            var scale = num2 / 2;
            var rotation = 6.2831855f * num2;

            mtexture.DrawCentered(position, color, scale, rotation);
            num++;
        }
        
        StencilMaker.EndContent();
        StencilMaker.SwapFromStencilTarget();
    }

    private Color MultAlpha(Color c, float f)
    {
        return new Color(c.ToVector4() * new Vector4(1, 1, 1, f));
    }

    public bool Activated()
    {
        return onFlag == "" || (Scene as Level).Session.GetFlag(onFlag);
    }

    public override void Render()
    {
        base.Render();
        
        if (StencilMaker.GetStencilTexture() != null && Activated())
        {
            var tex = StencilMaker.GetStencilTexture();
            Draw.SpriteBatch.Draw(tex, Position - new Vector2(tex.Width / 2, tex.Height / 2),Color.White * bufferAlpha);
        }

        GFX.Game["objects/common/warpportal/frame"].DrawCentered(Position);
        
        foreach (var p in particles)
        {
            if (Vector2.Distance(p.pos, Position) <= 2)
            {
                float dir = Calc.Random.Next(360);
                dir = dir.ToRad();
                p.pos = new Vector2(Position.X + Calc.Random.Next(-particlerange, particlerange),
                    Position.Y + Calc.Random.Next(-particlerange, particlerange));
                p.vel = Vector2.Normalize(Vector2.Normalize(new Vector2((float)Math.Cos(dir), (float)Math.Sin(dir)))) *
                        6;
            }

            if (Vector2.Distance(p.pos, Position) <= 16)
                p.vel = Vector2.Normalize(new Vector2(Position.X - p.pos.X, Position.Y - p.pos.Y));
            else
                p.vel = Vector2.Normalize(new Vector2(Position.X - p.pos.X, Position.Y - p.pos.Y)
                                          * (portalSpeed / 20)
                                          / Vector2.Distance(Position, p.pos)
                                          + p.vel * (2 / portalSpeed));
            p.pos += p.vel * (portalSpeed / 6);

            if (Activated()) p.Render();
        }
        
        
        
    }

    // Token: 0x06002706 RID: 9990 RVA: 0x000D9BC9 File Offset: 0x000D7DC9
    private void OnPlayer(Player player)
    {
        if (canTrigger && Activated())
        {
            canTrigger = false;
            Scene.Add(new TimeWarpCutscene(player, this, targetroom));
        }
    }

    // Token: 0x06002707 RID: 9991 RVA: 0x000D9BEC File Offset: 0x000D7DEC
    public override void Removed(Scene scene)
    {
        Dispose();
        base.Removed(scene);
    }

    // Token: 0x06002708 RID: 9992 RVA: 0x000D9BFB File Offset: 0x000D7DFB
    public override void SceneEnd(Scene scene)
    {
        Dispose();
        base.SceneEnd(scene);
    }

    // Token: 0x06002709 RID: 9993 RVA: 0x000D9C0A File Offset: 0x000D7E0A
    private void Dispose()
    {
    }

    public class Particle
    {
        public Color col;
        public Vector2 pos;
        public Vector2 vel;

        public Particle(Vector2 position, Color c)
        {
            pos = position;
            col = c;
            vel = new Vector2(0, 0);
        }

        public void Render()
        {
            Draw.Point(pos, col);
        }
    }

    // Token: 0x0200061F RID: 1567
    private struct Debris
    {
        // Token: 0x040020BE RID: 8382
        public Vector2 Direction;

        // Token: 0x040020BF RID: 8383
        public float Percent;

        // Token: 0x040020C0 RID: 8384
        public float Duration;

        // Token: 0x040020C1 RID: 8385
        public bool Enabled;
    }

    // Token: 0x02000620 RID: 1568
}