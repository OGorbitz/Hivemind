using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hivemind.World
{
    class WorldManager
    {
        private static TileMap ActiveTileMap;
        private static EditorTileMap EditorTileMap = new EditorTileMap();

        public static void SetActiveTileMap(TileMap tileMap)
        {
            ActiveTileMap = tileMap;
        }

        public static TileMap GetActiveTileMap()
        {
            return ActiveTileMap;
        }

        public static EditorTileMap GetEditorTileMap()
        {
            return EditorTileMap;
        }

        internal static void RenderEditor(object spriteBatch, GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        internal static void DrawEditor(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        internal static void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            EditorTileMap.Render(spriteBatch, graphicsDevice, gameTime);
            EditorTileMap.ClearTiles();

            if (ActiveTileMap != null)
                ActiveTileMap.Draw(spriteBatch, graphicsDevice, gameTime);
            EditorTileMap.Draw(spriteBatch, graphicsDevice, gameTime);
        }

        internal static void Update(GameTime gameTime)
        {
            if (ActiveTileMap != null)
                ActiveTileMap.Update(gameTime);
        }
    }
}
