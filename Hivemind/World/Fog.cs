using Hivemind.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.World
{
    public enum Visibility { HIDDEN, KNOWN, VISIBLE }


    public static class Fog
    {
        public static Texture2D MaskAtlas;

        public static void Init(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            MaskAtlas = contentManager.Load<Texture2D>("Tiles/Masks/FogMask");
        }

        public static void RevealCircle(Vector2 position, float size, TileMap tileMap)
        {
            List<Point> area = ShapeSelecter.GetCircle(position, size);
            foreach(Point p in area)
            {
                tileMap.GetTile(p).Visibility = Visibility.VISIBLE;
            }
        }
    }
}
