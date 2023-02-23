
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.Tetrahelper

{
    public class StencilMaker
    {
        private static readonly AlphaTestEffect alphaTestEffect = new AlphaTestEffect(Engine.Graphics.GraphicsDevice)
        {
            VertexColorEnabled = true,
            DiffuseColor = Color.White.ToVector3(),
            AlphaFunction = CompareFunction.GreaterEqual,
            ReferenceAlpha = 1,
            World = Matrix.Identity,
            View = Matrix.Identity,
            Projection = Matrix.CreateOrthographicOffCenter(0, 320, 180, 0, 0, 1)
        };

        private static readonly DepthStencilState stencilMask = new DepthStencilState
        {
            StencilEnable = true,
            StencilFunction = CompareFunction.Always,
            StencilPass = StencilOperation.Replace,
            ReferenceStencil = 1,
            DepthBufferEnable = false,
        };

        public static readonly DepthStencilState stencilContent = new DepthStencilState
        {
            StencilEnable = true,
            StencilFunction = CompareFunction.Equal,
            StencilPass = StencilOperation.Keep,
            ReferenceStencil = 1,
            DepthBufferEnable = false,
        };

        private static VirtualRenderTarget stencilTarget;

        public static void SwapToStencilTarget()
        {
            if (stencilTarget == null)
            {
                stencilTarget = VirtualContent.CreateRenderTarget("stencil-maker", 320, 180, depth: true);
            }

            Engine.Graphics.GraphicsDevice.SetRenderTarget(stencilTarget);
            Engine.Graphics.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.Stencil, Color.Transparent, 0, 0);
        }

        public static void SwapFromStencilTarget()
        {
            Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Gameplay);
        }

        public static void StartMask(BlendState blendState = null)
        {
            Draw.SpriteBatch.Begin(SpriteSortMode.Immediate, blendState, null, stencilMask, null, alphaTestEffect);
        }

        public static void EndMask()
        {
            Draw.SpriteBatch.End();
        }

        public static void StartContent(BlendState blendState = null)
        {
            Draw.SpriteBatch.Begin(SpriteSortMode.Immediate, blendState, null, stencilContent, null, null);
        }

        public static void EndContent()
        {
            Draw.SpriteBatch.End();
        }

        public static Texture2D GetStencilTexture()
        {
            if (stencilTarget != null) {
                return stencilTarget;
            }
            else
            {
                return null;
            }
        }
    }
}
