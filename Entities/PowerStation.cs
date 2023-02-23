using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework.Graphics;


namespace Celeste.Mod.Tetrahelper.Entities
{
	[CustomEntity("Tetrahelper/PowerStation")]
	class PowerStation : Entity
    {

		// Token: 0x06002D25 RID: 11557 RVA: 0x00109B3C File Offset: 0x00107D3C
		private string Flag;
		private VertexLight light;

		// Token: 0x0400211C RID: 8476
		private BloomPoint bloom;

		public PowerStation(EntityData data, Vector2 offset) : base(data.Position + offset)
		{



			

			base.Add(this.sprite = GFX.SpriteBank.Create("PowerStation"));
			base.Add(new PlayerCollider(new Action<Player>(this.OnPlayer), new Circle(40f, 0f, 0f), null));
            this.sprite.Play("offIdle", false, false);

            this.Depth = 1965;
            this.unlockSfxName = "event:/game/05_mirror_temple/key_unlock_dark";
            this.Flag = data.Attr("Flag", "PowerFlag");


        }

        public override void Added(Scene scene)

        {
            base.Added(scene);

            Level level = this.SceneAs<Level>();
            if (this.sprite != null && level != null)
            {
                if (level.Session.GetFlag(Flag))
                {

                    this.sprite.Play("onIdle", false, false);
                }
                else
                {

                    this.sprite.Play("offIdle", false, false);
                }
            }
        }

        // Token: 0x06002D26 RID: 11558 RVA: 0x00109C50 File Offset: 0x00107E50


        // Token: 0x06002D27 RID: 11559 RVA: 0x00109C8D File Offset: 0x00107E8D


        // Token: 0x06002D28 RID: 11560 RVA: 0x00109CC8 File Offset: 0x00107EC8
        private void OnPlayer(Player player)
		{
            Level level = this.SceneAs<Level>();

            if (!this.opening && !level.Session.GetFlag(Flag))
            {
                foreach (Follower follower in player.Leader.Followers)
                {
                    if (follower.Entity is Key && !(follower.Entity as Key).StartedUsing)
                    {
                        this.TryOpen(player, follower);
                        break;
                    }
                }
            }
        }

        // Token: 0x06002D29 RID: 11561 RVA: 0x00109D4C File Offset: 0x00107F4C
        private void TryOpen(Player player, Follower fol)
        {

            this.opening = true;
            (fol.Entity as Key).StartedUsing = true;
            base.Add(new Coroutine(this.ActivateRoutine(fol), true));


        }


        // Token: 0x06002D2A RID: 11562 RVA: 0x00109DAB File Offset: 0x00107FAB
        private IEnumerator ActivateRoutine(Follower fol)
        {
            SoundEmitter emitter = SoundEmitter.Play(this.unlockSfxName, this, null);
            emitter.Source.DisposeOnTransition = true;
            Level level = this.SceneAs<Level>();
            Key key = fol.Entity as Key;
            this.Add(new Coroutine(key.UseRoutine(this.Center + new Vector2(0, -8)), true));
            yield return 1.2f;
            this.UnlockingRegistered = true;

            key.RegisterUsed();
            while (key.Turning)
            {
                yield return null;
            }
            this.Tag |= Tags.TransitionUpdate;
            level.Session.SetFlag(Flag, true);


            emitter.Source.DisposeOnTransition = false;
            yield return this.sprite.PlayRoutine("anim", false);
            level.Shake(0.3f);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);



            yield break;
        }

        // Token: 0x040026BB RID: 9915
        public static ParticleType P_Appear;

		// Token: 0x040026BC RID: 9916
		public EntityID ID;

		// Token: 0x040026BD RID: 9917
		public bool UnlockingRegistered;

		// Token: 0x040026BE RID: 9918
		private Sprite sprite;

		// Token: 0x040026BF RID: 9919
		private bool opening;

		// Token: 0x040026C0 RID: 9920
		private bool on;

		// Token: 0x040026C1 RID: 9921
		private string unlockSfxName;
	}
}
