using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.Tetrahelper.Entities
{
    [CustomEntity("TetraHelper/MomentumMirror")]
    [Tracked(false)]
	public class MomentumMirror : Entity
	{

		private float lastplayerspeed;
		private float lastplayerdir;
		private bool wasactivatedlastframe;
		private bool activatedthisframe;
		// Token: 0x0600169E RID: 5790 RVA: 0x0005F6D8 File Offset: 0x0005D8D8
		public MomentumMirror(Vector2 position, float height, bool left) : base(position)
		{
			base.Tag = Tags.TransitionUpdate;
			base.Depth = 1999;
			
			if (left)
			{
				this.Facing = Facings.Left;
				base.Collider = new Hitbox(6f, height, 0f, 0f);
				
			}
			else
			{
				this.Facing = Facings.Right;
				base.Collider = new Hitbox(6f, height, 2f, 0f);
			}
			Add(new PlayerCollider(OnPlayer));

			base.Add(this.staticMover = new StaticMover());
			base.Add(this.climbBlocker = new ClimbBlocker(false));
			base.Add(this.idleSfx = new SoundSource());
			this.tiles = this.BuildSprite(left);
		}

		private bool checkDirection(float dir)
        {
			return ((this.Facing == Facings.Left &&
					(dir >= (100 + 180) % 360 ||
					 dir <= (260 + 180) % 360))

				||
					(this.Facing == Facings.Right &&
					(dir % 360 >= 100 % 360 &&
					 dir % 360 <= 260 % 360)
				));

		}

        private void OnPlayer(Player obj)
        {
			float playerdir = obj.Speed.Angle().ToDeg() + 180;
			activatedthisframe = true;

			if ((checkDirection(playerdir) && ((Math.Abs(obj.Speed.X) > 12) || Math.Abs(lastplayerspeed) > 12))
					||
					(checkDirection(lastplayerdir) && ((Math.Abs(obj.Speed.X) > 12) || Math.Abs(lastplayerspeed) > 12))
					&& !wasactivatedlastframe
					) {

					obj.StateMachine.State = 0;
					if ((lastplayerspeed > obj.Speed.X && obj.Speed.X >= 0) || (lastplayerspeed < obj.Speed.X && obj.Speed.X <= 0))
					{
						obj.Speed.X = -1 * lastplayerspeed;
					}
					else
					{
						obj.Speed.X *= -1;
					}



					//obj.Speed.Y -= 10f;

					if (obj.Facing == Facings.Right)
					{
						obj.Facing = Facings.Left;
					} else {
						obj.Facing = Facings.Right;
					}
					Audio.Play("event:/game/03_resort/forcefield_bump", this.Position);
					base.SceneAs<Level>().Particles.Emit(HeartGem.P_BlueShine, 8, obj.Position, Vector2.One * 8f);
					base.SceneAs<Level>().Displacement.AddBurst(obj.Position, 0.2f, 10f, 28f, 0.2f, null, null);
			}
		}

        // Token: 0x0600169F RID: 5791 RVA: 0x0005F7B9 File Offset: 0x0005D9B9
        public MomentumMirror(EntityData data, Vector2 offset) : this(data.Position + offset, (float)data.Height, data.Bool("left", false))
		{
		}



     

        private List<Sprite> BuildSprite(bool left)
        {
            List<Sprite> list = new List<Sprite>();
            int num = 0;
            while ((float)num < base.Height)
            {
                string id;
                if (num == 0)
                {
                    id = "MomentumMirrorTop";
                }
                else if ((float)(num + 16) > base.Height)
                {
                    id = "MomentumMirrorBottom";
                }
                else
                {
                    id = "MomentumMirrorMid";
                }
                Sprite sprite = GFX.SpriteBank.Create(id);
                if (!left)
                {
                    sprite.FlipX = true;
                    sprite.Position = new Vector2(0f, (float)num);
                }
                else
                {
                    sprite.Position = new Vector2(0f, (float)num);
                }
                list.Add(sprite);
                base.Add(sprite);
                num += 8;
            }
            return list;
        }

        // Token: 0x060016A1 RID: 5793 RVA: 0x0005F888 File Offset: 0x0005DA88
        public override void Added(Scene scene)
		{
			base.Added(scene);
			
		}

        // Token: 0x060016A2 RID: 5794 RVA: 0x0005F8C0 File Offset: 0x0005DAC0
        public override void Render()
        {
			//Draw.Rect(base.Collider, Color.White);
            base.Render();
        }

        // Token: 0x060016A3 RID: 5795 RVA: 0x0005F939 File Offset: 0x0005DB39
        public override void Update()
		{
			this.PositionIdleSfx();
			if ((base.Scene as Level).Transitioning)
			{
				return;
			}
			base.Update();

			lastplayerspeed = base.Scene.Tracker.GetEntity<Player>().Speed.X;
			lastplayerdir = base.Scene.Tracker.GetEntity<Player>().Speed.Angle().ToDeg() + 180;

			wasactivatedlastframe = activatedthisframe;
			activatedthisframe = false;

			
		}

		// Token: 0x060016A4 RID: 5796 RVA: 0x0005F95C File Offset: 0x0005DB5C
		private void PositionIdleSfx()
		{
			Player entity = base.Scene.Tracker.GetEntity<Player>();
			if (entity != null)
			{
				this.idleSfx.Position = Calc.ClosestPointOnLine(this.Position, this.Position + new Vector2(0f, base.Height), entity.Center) - this.Position;
				this.idleSfx.UpdateSfxPosition();
			}
		}

		// Token: 0x04000FFD RID: 4093
		public Facings Facing;

		// Token: 0x04000FFE RID: 4094
		private StaticMover staticMover;

		// Token: 0x04000FFF RID: 4095
		private ClimbBlocker climbBlocker;

		// Token: 0x04001000 RID: 4096
		private SoundSource idleSfx;

		// Token: 0x04001001 RID: 4097
		public bool IceMode;

		// Token: 0x04001002 RID: 4098
		private bool notCoreMode;

		// Token: 0x04001003 RID: 4099
		private List<Sprite> tiles;
	}
}
