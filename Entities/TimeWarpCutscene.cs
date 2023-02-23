using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Tetrahelper.Entities
{
	// Token: 0x020002A9 RID: 681
	public class TimeWarpCutscene : CutsceneEntity
	{
		// Token: 0x060013CC RID: 5068 RVA: 0x0004F09A File Offset: 0x0004D29A
		private string target;
		public TimeWarpCutscene(Player player, TimeWarpPortal portal, string targetRoom) : base(true, false)
		{
			this.player = player;
			this.portal = portal;
			this.target = targetRoom;
		}

		// Token: 0x060013CD RID: 5069 RVA: 0x0004F0B4 File Offset: 0x0004D2B4
		public override void OnBegin(Level level)
		{
			base.Add(new Coroutine(this.Cutscene(level), true));
			level.Add(this.fader = new TimeWarpCutscene.Fader());
		}

		// Token: 0x060013CE RID: 5070 RVA: 0x0004F0E8 File Offset: 0x0004D2E8
		private IEnumerator Cutscene(Level level)
		{
			this.player.StateMachine.State = 11;
			this.player.StateMachine.Locked = true;
			this.player.Dashes = 1;
			if (level.Session.Area.Mode == AreaMode.Normal)
			{
				Audio.SetMusic(null, true, true);
			}
			else
			{
				this.Add(new Coroutine(this.MusicFadeOutBSide(), true));
			}
			this.Add(this.sfx = new SoundSource());
			this.sfx.Position = this.portal.Center;
			this.sfx.Play("event:/music/lvl5/mirror_cutscene", null, 0f);
			this.Add(new Coroutine(this.CenterCamera(), true));
			yield return this.player.DummyWalkToExact((int)this.portal.X, false, 1f, false);
			yield return 0.25f;
			yield return this.player.DummyWalkToExact((int)this.portal.X - 16, false, 1f, false);
			yield return 0.5f;
			yield return this.player.DummyWalkToExact((int)this.portal.X + 16, false, 1f, false);
			yield return 0.25f;
			this.player.Facing = Facings.Left;
			yield return 0.25f;
			yield return this.player.DummyWalkToExact((int)this.portal.X, false, 1f, false);
			yield return 0.1f;
			Input.Rumble(RumbleStrength.Strong, RumbleLength.FullSecond);
			while (this.portal.portalSpeed < 8f)
			{
				this.Level.Shake(.5f);
				this.portal.portalSpeed += 0.1f;
				yield return null;
			}
			this.player.DummyAutoAnimate = false;
			this.player.Sprite.Play("lookUp", false, false);
			yield return 1f;
			this.player.DummyAutoAnimate = true;
			this.portal.Activate();
			this.Add(new Coroutine(level.ZoomTo(new Vector2(160f, 90f), 3f, 12f), true));
			yield return 0.25f;
			this.player.ForceStrongWindHair.X = -1f;
			yield return this.player.DummyWalkToExact((int)this.player.X + 12, true, 1f, false);
			yield return 0.5f;
			this.player.Facing = Facings.Right;
			this.player.DummyAutoAnimate = false;
			this.player.DummyGravity = false;
			this.player.Sprite.Play("runWind", false, false);
			while (this.player.Sprite.Rate > 0f)
			{
				this.player.MoveH(this.player.Sprite.Rate * 10f * Engine.DeltaTime, null, null);
				this.player.MoveV(-(1f - this.player.Sprite.Rate) * 6f * Engine.DeltaTime, null, null);
				this.player.Sprite.Rate -= Engine.DeltaTime * 0.15f;
				yield return null;
			}
			yield return 0.5f;
			this.player.Sprite.Play("fallFast", false, false);
			this.player.Sprite.Rate = 1f;
			Vector2 target = this.portal.Center + new Vector2(0f, 8f);
			Vector2 from = this.player.Position;
			for (float p = 0f; p < 1f; p += Engine.DeltaTime * 2f)
			{
				this.player.Position = from + (target - from) * Ease.SineInOut(p);
				yield return null;
			}
			this.player.ForceStrongWindHair.X = 0f;
			target = default(Vector2);
			from = default(Vector2);
			//this.fader.Target = 1f;
			yield return 2f;
			//this.player.Sprite.Play("sleep", false, false);
			yield return 1f;
			yield return level.ZoomBack(1f);
			
				level.Session.ColorGrade = "templevoid";
				for (float p = 0f; p < 1f; p += Engine.DeltaTime)
				{
					Glitch.Value = p * 0.05f;
					level.ScreenPadding = 32f * p;
					yield return null;
				}
			
			while ((this.portal.DistortionFade -= Engine.DeltaTime * 2f) > 0f)
			{
				yield return null;
			}
			this.EndCutscene(level, true);
			yield break;
		}

		// Token: 0x060013CF RID: 5071 RVA: 0x0004F0FE File Offset: 0x0004D2FE
		private IEnumerator CenterCamera()
		{
			Camera camera = this.Level.Camera;
			Vector2 target = this.portal.Center - new Vector2(160f, 90f);
			while ((camera.Position - target).Length() > 1f)
			{
				camera.Position += (target - camera.Position) * (1f - (float)Math.Pow(0.009999999776482582, (double)Engine.DeltaTime));
				yield return null;
			}
			yield break;
		}

		// Token: 0x060013D0 RID: 5072 RVA: 0x0004F10D File Offset: 0x0004D30D
		private IEnumerator MusicFadeOutBSide()
		{
			for (float p = 1f; p > 0f; p -= Engine.DeltaTime)
			{
				Audio.SetMusicParam("fade", p);
				yield return null;
			}
			Audio.SetMusicParam("fade", 0f);
			yield break;
		}

		// Token: 0x060013D1 RID: 5073 RVA: 0x0004F118 File Offset: 0x0004D318
		public override void OnEnd(Level level)
		{
			level.OnEndOfFrame += delegate ()
			{
				if (this.fader != null && !this.WasSkipped)
				{
					this.fader.Tag = Tags.Global;
					this.fader.Target = 0f;
					this.fader.Ended = true;
				}
				Leader.StoreStrawberries(this.player.Leader);
				level.Remove(this.player);
				level.UnloadLevel();
				
				level.Session.Keys.Clear();
				
	
					level.Session.Level = target;
					level.Session.RespawnPoint = new Vector2?(level.GetSpawnPoint(new Vector2((float)level.Bounds.Left, (float)level.Bounds.Top)));
					level.LoadLevel(Player.IntroTypes.WakeUp, false);
					Audio.SetMusicParam("fade", 1f);
				level.Session.ColorGrade = "none";
				Leader.RestoreStrawberries(level.Tracker.GetEntity<Player>().Leader);
				level.Camera.Y -= 8f;
				Glitch.Value = 0;
				if (!this.WasSkipped && level.Wipe != null)
				{
					level.Wipe.Cancel();
				}
				if (this.fader != null)
				{
					this.fader.RemoveTag(Tags.Global);
				}
			};
		}

		// Token: 0x04000D8F RID: 3471
		private Player player;

		// Token: 0x04000D90 RID: 3472
		private TimeWarpPortal portal;

		// Token: 0x04000D91 RID: 3473
		private TimeWarpCutscene.Fader fader;

		// Token: 0x04000D92 RID: 3474
		private SoundSource sfx;

		// Token: 0x020002AA RID: 682
		private class Fader : Entity
		{
			// Token: 0x060013D2 RID: 5074 RVA: 0x0004F150 File Offset: 0x0004D350
			public Fader()
			{
				base.Depth = -1000000;
			}

			// Token: 0x060013D3 RID: 5075 RVA: 0x0004F164 File Offset: 0x0004D364
			public override void Update()
			{
				this.fade = Calc.Approach(this.fade, this.Target, Engine.DeltaTime * 0.5f);
				if (this.Target <= 0f && this.fade <= 0f && this.Ended)
				{
					base.RemoveSelf();
				}
				base.Update();
			}

			// Token: 0x060013D4 RID: 5076 RVA: 0x0004F1C4 File Offset: 0x0004D3C4
			public override void Render()
			{
				Camera camera = (base.Scene as Level).Camera;
				if (this.fade > 0f)
				{
					Draw.Rect(camera.X - 10f, camera.Y - 10f, 340f, 200f, Color.Black * this.fade);
				}
				Player entity = base.Scene.Tracker.GetEntity<Player>();
				if (entity != null && !entity.OnGround(2))
				{
					entity.Render();
				}
			}

			// Token: 0x04000D93 RID: 3475
			public float Target;

			// Token: 0x04000D94 RID: 3476
			public bool Ended;

			// Token: 0x04000D95 RID: 3477
			private float fade;
		}
	}
}
