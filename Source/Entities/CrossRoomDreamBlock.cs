
using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.Tetrahelper.Entities;

[CustomEntity("Tetrahelper/CrossRoomDreamBlock")]
[TrackedAs(typeof(DreamBlock))]
public class CrossRoomDreamBlock : DreamBlock
{
    
    public CrossRoomDreamBlock(EntityData data, Vector2 offset) : base(data, offset)
    {
        
    }
    
    public static void ApplyIlHooks()
    {
        Logger.Log(nameof(TetrahelperModule), $"Injecting into Player.DreamDashUpdate in IL code for DreamBlock");
        IL.Celeste.Player.DreamDashUpdate += Player_DreamDashUpdate;
    }
    
    public static void UnapplyIlHooks()
    {
        IL.Celeste.Player.DreamDashUpdate -= Player_DreamDashUpdate;
    }
    
    private static CrossRoomDreamBlock lastCrossRoomDreamBlock;


    public static void Player_DreamDashUpdate(ILContext il)
    {
        
        // after
        // DreamBlock dreamBlock = this.CollideFirst<DreamBlock> 
        // IL
           // IL_0043: ldarg.0      // this
           //  IL_0044: call         instance !!0/*class Celeste.DreamBlock*/ Monocle.Entity::CollideFirst<class Celeste.DreamBlock>()
           //  IL_0049: stloc.1      // dreamBlock
        ILCursor cursor = new ILCursor(il);
        
        
        
        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall<Entity>("CollideFirst")))
        {
            Logger.Log(nameof(TetrahelperModule), $"Injecting into Player.DreamDashUpdate at {cursor.Index} in IL code for DreamBlock");
            
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Action<DreamBlock, Player>>(Insert);
            
            
        } else
        {
            Logger.Log(nameof(TetrahelperModule), $"Failed to inject into Player.DreamDashUpdate at {cursor.Index} in IL code for DreamBlock");
        }
        
    }
    
    private static void Insert(DreamBlock dreamBlock, Player player)
    {
        if (dreamBlock is CrossRoomDreamBlock crossRoomDreamBlock)
        {
            lastCrossRoomDreamBlock = crossRoomDreamBlock;
        }

        if (lastCrossRoomDreamBlock != null && lastCrossRoomDreamBlock == dreamBlock)
        {
            player.StateMachine.State = 9;
        }
    }
    
    
    
    public override void Added(Scene scene)
    {
        base.Added(scene);
        if (Scene.Tracker.GetEntity<Player>() != null && Scene.Tracker.GetEntity<Player>().CollideCheck(this))
        {
            Scene.Tracker.GetEntity<Player>().StateMachine.State = 9;
        }
    }
    
    

    
    
}