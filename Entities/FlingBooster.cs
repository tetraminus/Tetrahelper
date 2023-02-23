using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;

namespace Celeste
{
	// Token: 0x020005BD RID: 1469
	[CustomEntity("Tetrahelper/FlingBooster")]
	public class FlingBooster : Entity
	{
		// Token: 0x170005A0 RID: 1440
		// (get) Token: 0x060024C3 RID: 9411 RVA: 0x000C79E4 File Offset: 0x000C5BE4
		// (set) Token: 0x060024C4 RID: 9412 RVA: 0x000C79EC File Offset: 0x000C5BEC
		public bool BoostingPlayer { get; private set; }

		public int Direction;
		public int SprDirectionDeg;
		public int DirectionDeg;
		public int Speed;

		// Token: 0x060024C5 RID: 9413 RVA: 0x000C79F8 File Offset: 0x000C5BF8
		public FlingBooster(EntityData data, Vector2 offset) : base(data.Position + offset)
		{




			base.Depth = -8500;
			base.Collider = new Circle(10f, 0f, 2f);

			base.Add(this.sprite = GFX.SpriteBank.Create("FlingBooster"));
			base.Add(new PlayerCollider(new Action<Player>(this.OnPlayer), null, null));
			base.Add(this.light = new VertexLight(Color.White, 1f, 16, 32));
			base.Add(this.bloom = new BloomPoint(0.1f, 16f));
			base.Add(this.wiggler = Wiggler.Create(0.5f, 4f, delegate (float f)
			{
				this.sprite.Scale = Vector2.One * (1f + f * 0.25f);
			}, false, false));
			base.Add(this.dashRoutine = new Coroutine(false));
			base.Add(this.dashListener = new DashListener());
			base.Add(new MirrorReflection());
			base.Add(this.loopingSfx = new SoundSource());
			this.dashListener.OnDash = new Action<Vector2>(this.OnPlayerDashed);
			this.particleType = (Booster.P_Burst);


			this.Speed = data.Int("Speed", 500);
			switch (data.Attr("Direction", "R"))
            {
			
				case "R":
					Direction = 0;
					break;
				case "DR":
					Direction = 1;
					break;
				case "D":
					Direction = 2;
					break;
				case "DL":
					Direction = 3;
					break;
				case "L":
					Direction = 4;
					break;
				case "UL":
					Direction = 5;
					break;
				case "U":
					Direction = 6;
					break;
				case "UR":
					Direction = 7;
					break;
				default:
					Direction = 0;
					break;
		}

			this.DirectionDeg = (this.Direction) * 45;
			this.SprDirectionDeg = (Direction / 2) * 90;



	}

		// Token: 0x060024C6 RID: 9414 RVA: 0x000C7B5F File Offset: 0x000C5D5F
		

		// Token: 0x060024C7 RID: 9415 RVA: 0x000C7B94 File Offset: 0x000C5D94
		public override void Added(Scene scene)
		{
			base.Added(scene);
			Image image = new Image(GFX.Game["objects/FlingBooster/outline"]);
			image.CenterOrigin();
			image.Color = Color.White * 0.75f;
			this.outline = new Entity(this.Position);
			this.outline.Depth = 8999;
			this.outline.Visible = false;
			this.outline.Add(image);
			this.outline.Add(new MirrorReflection());
            scene.Add(this.outline);
			

			sprite.Play(((this.Direction % 2) == 0) ? "idle" : "upidle");
			
			sprite.Rotation = Calc.ToRad(this.SprDirectionDeg);


			//sprite.Visible = false;
		}

        // Token: 0x060024C8 RID: 9416 RVA: 0x000C7C30 File Offset: 0x000C5E30
        public void Appear()
		{
			Audio.Play("event:/game/04_cliffside/greenBooster_reappear", this.Position);
			//this.sprite.Play("appear", false, false);
			this.wiggler.Start();
			this.Visible = true;
			this.AppearParticles();
		}

		// Token: 0x060024C9 RID: 9417 RVA: 0x000C7C88 File Offset: 0x000C5E88
		private void AppearParticles()
		{
			ParticleSystem particlesBG = base.SceneAs<Level>().ParticlesBG;
			for (int i = 0; i < 360; i += 30)
			{
				particlesBG.Emit(this.red ? Booster.P_RedAppear : Booster.P_Appear, 1, base.Center, Vector2.One * 2f, (float)i * 0.017453292f);
			}
		}

		// Token: 0x060024CA RID: 9418 RVA: 0x000C7CEC File Offset: 0x000C5EEC
		private void OnPlayer(Player player)
		{
			if (this.respawnTimer <= 0f && this.cannotUseTimer <= 0f && !this.BoostingPlayer)
			{
				this.cannotUseTimer = 0.45f;
				
				Audio.Play("event:/game/04_cliffside/greenBooster_enter", this.Position);
				this.wiggler.Start();
			    sprite.Play((this.Direction % 2 == 0 )? "Boost": "upBoost");
				
				//this.sprite.FlipX = (player.Facing == Facings.Left);
				base.Add(new Coroutine(this.BoostRoutine(player), true));
			}
		}

		// Token: 0x060024CB RID: 9419 RVA: 0x000C7D98 File Offset: 0x000C5F98


		// Token: 0x060024CC RID: 9420 RVA: 0x000C7F08 File Offset: 0x000C6108
		private IEnumerator BoostRoutine(Player player)
		{

			
			
			for (int i = 0; i < 10; i++)
			{
				player.StateMachine.State = 6;
				player.Speed = Vector2.Zero;
				Vector2 vector = Calc.Approach(player.ExactPosition, this.Position - new Vector2(0,-8), 100f * Engine.DeltaTime);
				player.MoveToX(vector.X, null);
				player.MoveToY(vector.Y, null);
				yield return null;
			}




			Audio.Play("event:/game/03_resort/forcefield_bump", this.Position);


			player.Speed = new Vector2(400, 0).Rotate(Calc.ToRad(DirectionDeg));
			//Console.WriteLine(new Vector2(400, 0).Rotate(Calc.ToRad(DirectionDeg)));
			//Console.WriteLine("deg: " + DirectionDeg);
			//Console.WriteLine("---");


			//player.Speed.Y += -120f;


			Celeste.Freeze(0.1f);
			SlashFx.Burst(base.Center, player.Speed.Angle());
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);
			base.SceneAs<Level>().DirectionalShake(player.Speed, 0.15f);
			base.SceneAs<Level>().Displacement.AddBurst(base.Center, 0.3f, 8f, 32f, 0.8f, null, null);
			base.SceneAs<Level>().Particles.Emit(Bumper.P_Launch, 10, base.Center * 12f, Vector2.One * 3f, player.Speed.Angle());

			player.RefillDash();
			player.RefillStamina();

			yield break;
		}

		// Token: 0x060024CD RID: 9421 RVA: 0x000C7F25 File Offset: 0x000C6125
		public void OnPlayerDashed(Vector2 direction)
		{
			if (this.BoostingPlayer)
			{
				this.BoostingPlayer = false;
			}
		}

		// Token: 0x060024CE RID: 9422 RVA: 0x000C7F38 File Offset: 0x000C6138
		public void PlayerReleased()
		{
			Audio.Play(this.red ? "event:/game/05_mirror_temple/redFlingBooster_end" : "event:/game/04_cliffside/greenFlingBooster_end", this.sprite.RenderPosition);
			//this.sprite.Play("pop", false, false);
			this.cannotUseTimer = 0f;
			this.respawnTimer = 1f;
			this.BoostingPlayer = false;
			this.wiggler.Stop();
			this.loopingSfx.Stop(true);
		}

		// Token: 0x060024CF RID: 9423 RVA: 0x000C7FB1 File Offset: 0x000C61B1
		public void PlayerDied()
		{
			if (this.BoostingPlayer)
			{
				this.PlayerReleased();
				this.dashRoutine.Active = false;
				base.Tag = 0;
			}
		}

		// Token: 0x060024D0 RID: 9424 RVA: 0x000C7FD4 File Offset: 0x000C61D4
		public void Respawn()
		{
			Audio.Play(this.red ? "event:/game/05_mirror_temple/redFlingBooster_reappear" : "event:/game/04_cliffside/greenFlingBooster_reappear", this.Position);
			this.sprite.Position = Vector2.Zero;
			this.sprite.Play("idle", true, false);
			this.wiggler.Start();
			this.sprite.Visible = true;
			this.outline.Visible = false;
			this.AppearParticles();
		}

		// Token: 0x060024D1 RID: 9425 RVA: 0x000C804C File Offset: 0x000C624C
		public override void Update()
		{
			base.Update();
			if (this.cannotUseTimer > 0f)
			{
				this.cannotUseTimer -= Engine.DeltaTime;
			}
			if (this.respawnTimer > 0f)
			{
				this.respawnTimer -= Engine.DeltaTime;
				if (this.respawnTimer <= 0f)
				{
					this.Respawn();
				}
			}
			if (!this.dashRoutine.Active && this.respawnTimer <= 0f)
			{
				Vector2 target = Vector2.Zero;
				Player entity = base.Scene.Tracker.GetEntity<Player>();
				if (entity != null && base.CollideCheck(entity))
				{
					target = entity.Center + FlingBooster.playerOffset - this.Position;
				}
				this.sprite.Position = Calc.Approach(this.sprite.Position, target, 80f * Engine.DeltaTime);
			}
			if (this.sprite.CurrentAnimationID == "inside" && !this.BoostingPlayer && !base.CollideCheck<Player>())
			{
				this.sprite.Play("loop", false, false);
			}
		}

		// Token: 0x060024D2 RID: 9426 RVA: 0x000C816C File Offset: 0x000C636C
		public override void Render()
		{
			Vector2 position = this.sprite.Position;
			this.sprite.Position = position.Floor();
			if (this.sprite.CurrentAnimationID != "pop" && this.sprite.Visible)
			{
				this.sprite.DrawOutline(1);
			}
			base.Render();
			this.sprite.Position = position;
		}

		// Token: 0x060024D3 RID: 9427 RVA: 0x000C81D8 File Offset: 0x000C63D8
		public override void Removed(Scene scene)
		{
			if (this.Ch9HubTransition)
			{
				Level level = scene as Level;
				foreach (Backdrop backdrop in level.Background.GetEach<Backdrop>("bright"))
				{
					backdrop.ForceVisible = false;
					backdrop.FadeAlphaMultiplier = 1f;
				}
				level.Bloom.Base = AreaData.Get(level).BloomBase + 0.25f;
				level.Session.BloomBaseAdd = 0.25f;
			}
			base.Removed(scene);
		}

		// Token: 0x060024D4 RID: 9428 RVA: 0x000C8280 File Offset: 0x000C6480
		// Note: this type is marked as 'beforefieldinit'.
		static FlingBooster()
		{
			FlingBooster.playerOffset = new Vector2(0f, -2f);
		}

		// Token: 0x04001DE9 RID: 7657
		private const float RespawnTime = 1f;

		// Token: 0x04001DEA RID: 7658
		public static ParticleType P_Burst;

		// Token: 0x04001DEB RID: 7659
		public static ParticleType P_BurstRed;

		// Token: 0x04001DEC RID: 7660
		public static ParticleType P_Appear;

		// Token: 0x04001DED RID: 7661
		public static ParticleType P_RedAppear;

		// Token: 0x04001DEE RID: 7662
		public static readonly Vector2 playerOffset;

		// Token: 0x04001DEF RID: 7663
		private Sprite sprite;

		// Token: 0x04001DF0 RID: 7664
		private Entity outline;

		// Token: 0x04001DF1 RID: 7665
		private Wiggler wiggler;

		// Token: 0x04001DF2 RID: 7666
		private BloomPoint bloom;

		// Token: 0x04001DF3 RID: 7667
		private VertexLight light;

		// Token: 0x04001DF4 RID: 7668
		private Coroutine dashRoutine;

		// Token: 0x04001DF5 RID: 7669
		private DashListener dashListener;

		// Token: 0x04001DF6 RID: 7670
		private ParticleType particleType;

		// Token: 0x04001DF8 RID: 7672
		private float respawnTimer;

		// Token: 0x04001DF9 RID: 7673
		private float cannotUseTimer;

		// Token: 0x04001DFA RID: 7674
		private bool red;

		// Token: 0x04001DFB RID: 7675
		private SoundSource loopingSfx;

		// Token: 0x04001DFC RID: 7676
		public bool Ch9HubFlingBooster;

		// Token: 0x04001DFD RID: 7677
		public bool Ch9HubTransition;
	}
}
