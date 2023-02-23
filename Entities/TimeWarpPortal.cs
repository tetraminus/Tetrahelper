using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework.Graphics;

namespace Celeste.Mod.Tetrahelper.Entities
{
	// Token: 0x0200061E RID: 1566
	[CustomEntity("Tetrahelper/TimeWarpPortal")]
	public class TimeWarpPortal : Entity
	{
		
		private string targetroom;
		private string onFlag;
		private List<Particle> particles = new List<Particle>();

		private int particlerange = 64;
		public class Particle {
			public Vector2 pos;
			public Vector2 vel;
			public Color col;

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
		public TimeWarpPortal(EntityData data, Vector2 offset) : base(data.Position + offset)
		{
			
			onFlag = data.Attr("Flag");
			
			this.targetroom = data.Attr("Target");
			Everest.Events.Level.OnTransitionTo += CheckEvent;
			base.Position = Position;
			base.Depth = 2000;
			base.Collider = new Circle(23);
			base.Add(new PlayerCollider(new Action<Player>(this.OnPlayer), null, null));
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

			this.Add(new VertexLight(Vector2.Zero, Color.White*0.5f, 1f, 1,1));
			this.Add(new BloomPoint(new Vector2(0, 0), .2f, 32));
			//this.Add(new BloomPoint(new Vector2(0,30), 3f, 15));
			//this.Add(new BloomPoint(new Vector2(0,-30), 3f, 15));

			this.Add(new BeforeRenderHook(new Action(this.BeforeRender)));
			this.bufferAlpha = 1f;
			for ( int i = 0; i < 50; i++)
            {
			
				
				Vector2 TempPos = new Vector2(Position.X + Calc.Random.Next(-particlerange, particlerange), Position.Y + Calc.Random.Next(-particlerange, particlerange));

				Particle p = new Particle(TempPos, Color.White);
				float dir = Calc.Random.Next(360);
				dir = Calc.ToRad(dir);

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
			this.bufferTimer += portalSpeed * Engine.DeltaTime;
			StencilMaker.SwapToStencilTarget();
			Vector2 position = new Vector2(StencilMaker.GetStencilTexture().Width, StencilMaker.GetStencilTexture().Height) / 2f;
            MTexture mtexture = GFX.Game["objects/temple/portal/portal"];

            StencilMaker.StartMask();
            GFX.Game["objects/common/warpportal/mask"].DrawCentered(position, MultAlpha(Color.DarkBlue, .1f), new Vector2(1.5f));
            StencilMaker.EndMask();
            StencilMaker.StartContent();
			int num = 0;
			while ((float)num < 10f)
			{
				float num2 = this.bufferTimer % 1f * 0.1f + (float)num / 10f;
				Color color = Color.Lerp(MultAlpha(Color.DarkBlue,(.2f)*num2), MultAlpha(Color.SkyBlue, (.2f) * num2), num2);
				float scale = num2 / 2;
				float rotation = 6.2831855f * num2;

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
			return (onFlag == "" || (base.Scene as Level).Session.GetFlag(onFlag));
        }
		public override void Render()
		{
			
			base.Render();

			if (StencilMaker.GetStencilTexture() != null && Activated() )
			{
				Texture2D tex = StencilMaker.GetStencilTexture();
				Draw.SpriteBatch.Draw(tex, this.Position - new Vector2(tex.Width/2, tex.Height/2),
					Color.White * this.bufferAlpha);
			}

			GFX.Game["objects/common/warpportal/frame"].DrawCentered(this.Position);
			foreach(Particle p in particles)
            {
				if (Vector2.Distance(p.pos, Position) <= 2)
                {
					float dir = Calc.Random.Next(360);
					dir = Calc.ToRad(dir);
					p.pos = new Vector2(Position.X + Calc.Random.Next(-particlerange, particlerange),
						Position.Y + Calc.Random.Next(-particlerange, particlerange));
					p.vel = Vector2.Normalize(Vector2.Normalize(new Vector2((float)Math.Cos(dir), (float)Math.Sin(dir))))*6;
				}

				if (Vector2.Distance(p.pos, Position) <= 16)
				{
					p.vel = Vector2.Normalize(new Vector2(Position.X - p.pos.X, Position.Y - p.pos.Y));
                }else{
					p.vel = Vector2.Normalize(((
						new Vector2(Position.X - p.pos.X, Position.Y - p.pos.Y)
						* (portalSpeed / 20)))
						/ Vector2.Distance(Position, p.pos)
						+ (p.vel * (2 / portalSpeed)));
					
				}
				p.pos += p.vel * (portalSpeed / 6);

				if (Activated())
				{
					p.Render();
				}
            }
			
		}

		// Token: 0x06002706 RID: 9990 RVA: 0x000D9BC9 File Offset: 0x000D7DC9
		private void OnPlayer(Player player)
		{
			
			if (this.canTrigger && Activated())
			{	

				this.canTrigger = false;
				base.Scene.Add(new TimeWarpCutscene(player, this, targetroom));
			}
		}

		// Token: 0x06002707 RID: 9991 RVA: 0x000D9BEC File Offset: 0x000D7DEC
		public override void Removed(Scene scene)
		{
			this.Dispose();
			base.Removed(scene);
		}

		// Token: 0x06002708 RID: 9992 RVA: 0x000D9BFB File Offset: 0x000D7DFB
		public override void SceneEnd(Scene scene)
		{
			this.Dispose();
			base.SceneEnd(scene);
		}

		// Token: 0x06002709 RID: 9993 RVA: 0x000D9C0A File Offset: 0x000D7E0A
		private void Dispose()
		{
			
		}

		


		// Token: 0x040020B0 RID: 8368
		public static ParticleType P_CurtainDrop;

		// Token: 0x040020B1 RID: 8369
		public float DistortionFade;

		// Token: 0x040020B2 RID: 8370
		private bool canTrigger;

		public float portalSpeed;

		// Token: 0x040020B3 RID: 8371
		private int switchCounter;

		// Token: 0x040020B4 RID: 8372
		private VirtualRenderTarget portalbuffer;
		private VirtualRenderTarget circlebuffer;

		// Token: 0x040020B5 RID: 8373
		private float bufferAlpha;

		// Token: 0x040020B6 RID: 8374
		private float bufferTimer;

		// Token: 0x040020B7 RID: 8375
		private TimeWarpPortal.Debris[] debris;

		// Token: 0x040020B8 RID: 8376
		private Color debrisColorFrom;

		// Token: 0x040020B9 RID: 8377
		private Color debrisColorTo;

		// Token: 0x040020BA RID: 8378
		private MTexture debrisTexture;

		private Texture2D tex;




		// Token: 0x040020BC RID: 8380
		private TemplePortalTorch leftTorch;

		// Token: 0x040020BD RID: 8381
		private TemplePortalTorch rightTorch;

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
}
