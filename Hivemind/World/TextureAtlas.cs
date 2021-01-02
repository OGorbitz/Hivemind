using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.World
{
    class TextureAtlas
    {
        internal static RenderTarget2D Atlas;

        private const int AtlasWidth = 32;

        public static int CurrentTexture = 0, CurrentHeight = 0;


        public static void Init(GraphicsDevice graphicsDevice)
        {
            Atlas = new RenderTarget2D(graphicsDevice, AtlasWidth * TileManager.TileSize, TileManager.TileSize + TileManager.WallHeight, false, SurfaceFormat.Vector4, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);
        }

        public static int AddTexture(Texture2D t, GraphicsDevice graphicsDevice)
        {
            int x = CurrentTexture % AtlasWidth;
            int y = CurrentTexture / AtlasWidth;

            if (y > CurrentHeight)
            {
                CurrentHeight++;
                RenderTarget2D rt = new RenderTarget2D(graphicsDevice, AtlasWidth * TileManager.TileSize, (CurrentHeight + 1) * (TileManager.TileSize + TileManager.WallHeight), false, SurfaceFormat.Vector4, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);
                graphicsDevice.SetRenderTarget(rt);
                SpriteBatch sb = new SpriteBatch(graphicsDevice);
                sb.Begin();
                sb.Draw(Atlas, Vector2.Zero, Color.White);
                sb.End();
                Atlas.Dispose();
                Atlas = rt;
            }
            graphicsDevice.SetRenderTarget(Atlas);

            SpriteBatch spriteBatch = new SpriteBatch(graphicsDevice);
            spriteBatch.Begin();
            spriteBatch.Draw(t, new Vector2(x * TileManager.TileSize, y * (TileManager.TileSize + TileManager.WallHeight)), Color.White);
            spriteBatch.End();
            graphicsDevice.SetRenderTarget(null);
            CurrentTexture++;
            return CurrentTexture - 1;
        }

        public static Rectangle GetSourceRect(int ID)
        {
            int x = ID % AtlasWidth;
            int y = ID / AtlasWidth;
            return new Rectangle(x * TileManager.TileSize, y * (TileManager.TileSize + TileManager.WallHeight), TileManager.TileSize, TileManager.TileSize + TileManager.WallHeight);
        }
    }
}
