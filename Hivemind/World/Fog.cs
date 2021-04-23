using Hivemind.Utility;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.World
{
    public enum Visibility { HIDDEN, KNOWN, VISIBLE }

    public static class Sight
    {
        public static void RevealCircle(Point position, float size, TileMap tileMap)
        {
            List<Point> area = ShapeSelecter.GetCircle(position, (int)(size / TileManager.TileSize));
            foreach(Point p in area)
            {
                tileMap.GetTile(p).Visibility = Visibility.VISIBLE;
            }
        }
    }
}
