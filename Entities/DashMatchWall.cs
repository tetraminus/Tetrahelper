using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;


// Celeste.ChaserBarrie
namespace Celeste.Mod.TetraHelper.Entities

{
    
	[CustomEntity("TetraHelper/DashMatchWall")]
	[Tracked(false)]
	public class DashMatchWall : Entity
	{
        private bool AllowGreater;
        private int Amount;

            private Vector2 control, end;

        public DashMatchWall(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height, data.Int("dashes", 1), data.Bool("GT", false)) { }

            public DashMatchWall(Vector2 position, int width, int height, int dashes, bool greaterthan)
                : base(position)
            {
               
                
                Depth = Depths.DreamBlocks - 1;
                AllowGreater = greaterthan;
                Amount = dashes;
                Collider = new Hitbox(width, height);
                Add(new PlayerCollider(OnPlayer));
            }

            private void OnPlayer(Player player)
            {
                if (!player.Dead && player.StateMachine.State != Player.StCassetteFly)
                {
                    if (AllowGreater)
                    {
                        if (!(player.Dashes == Amount)) { player.Die(Vector2.Zero); }
                    }
                    else
                    {
                        if (!(player.Dashes >= Amount)) { player.Die(Vector2.Zero); }
                    }
                }
            }
        }
    }