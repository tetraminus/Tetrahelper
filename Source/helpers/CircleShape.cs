using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.Tetrahelper.helpers
{
	/// <summary>
	/// Drawable circle shape. Can be drawn by using static methods or be instantiated.
	/// </summary>
	public static class CircleShape
	{
		static CircleShape()
		{
			CircleVerticesCount = 16;
		}

		/// <summary>
		/// Amount of vertices in one circle. 
		/// </summary>
		public static int CircleVerticesCount 
		{
			set
			{
				if (_circleVerticesCount == value)
				{
					return;
				}
				_circleVerticesCount = value;
				_circleVectors = new Vector2[value];
				_circleVertices = new VertexPositionColor[value];

				var angAdd = Math.PI * 2 / value;
				
				for(var i = 0; i < value; i += 1)
				{
					_circleVectors[i] = new Vector2((float)Math.Cos(angAdd * i), (float)Math.Sin(angAdd * i));
				}
				CreateIndexBuffers();
			}
			get => _circleVerticesCount;
		}
		private static int _circleVerticesCount = 0;


		private static Vector2[] _circleVectors = new Vector2[_circleVerticesCount];

		private static VertexPositionColor[] _circleVertices = new VertexPositionColor[_circleVerticesCount];

		private static int[] _filledCircleIndices = new int[_circleVerticesCount * 3];
		
		
		/// <summary>
		/// Draws a circle.
		/// </summary>
		public static void Draw(Level level, Vector2 p, float r, Color color, float zDepth = 0)
		{
			
			for(var i = 0; i < _circleVerticesCount; i += 1)
			{
				_circleVertices[i].Position = new Vector3(
					p.X + r * _circleVectors[i].X, 
					p.Y + r * _circleVectors[i].Y, 
					zDepth
				);
				_circleVertices[i].Color = color;
			}
			Logger.Log("CircleShape", "Drawing circle with " + _circleVerticesCount + " vertices");
			GameplayRenderer.End();
			GFX.DrawIndexedVertices<VertexPositionColor>(
				level.Camera.Matrix,
				_circleVertices, 
				_circleVerticesCount * 3,
				_filledCircleIndices,
				_circleVerticesCount - 1
				);
			GameplayRenderer.Begin();
		}
		public static void RawDraw(Vector2 p, float r, Color color, float zDepth = 0)
		{
			
			for(var i = 0; i < _circleVerticesCount; i += 1)
			{
				_circleVertices[i].Position = new Vector3(
					p.X + r * _circleVectors[i].X, 
					p.Y + r * _circleVectors[i].Y, 
					zDepth
				);
				_circleVertices[i].Color = color;
			}
			Logger.Log("CircleShape", "Drawing circle with " + _circleVerticesCount + " vertices");
			// GameplayRenderer.End();
			GFX.DrawIndexedVertices<VertexPositionColor>(
				Matrix.Identity,
				_circleVertices, 
				_circleVerticesCount * 3,
				_filledCircleIndices,
				_circleVerticesCount - 1
			);
			// GameplayRenderer.Begin();
		}
		

		private static void CreateIndexBuffers()
		{
			_filledCircleIndices = new int[_circleVerticesCount * 3];
			for (var i = 0; i < _circleVerticesCount - 1; i += 1)
			{
				_filledCircleIndices[i * 3] = 0;
				_filledCircleIndices[i * 3 + 1] = (int)i;
				_filledCircleIndices[i * 3 + 2] = (int)(i + 1);
			}
		}
	}
}