using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.Input
{
    public enum EditShape { SINGLE, LINE, RECTANGLE }
    public enum PlacingType { TILE, ENTITY, TILE_ENTITY }
    public enum Action { BUILD, DESTROY, SELECT }

    public class Editing
    {
        public static EditShape Shape = EditShape.SINGLE;
        public static PlacingType Type = PlacingType.TILE;
        public static Vector2 Start;

        public static void StartEditing(Point start)
        {

        }



        public static List<Point> GetCurrentTiles(Point current)
        {
            

            switch (Shape)
            {
                case EditShape.SINGLE:
                    List<Point> tiles = new List<Point>();
                    tiles.Add(current);
                    return tiles;
                case EditShape.LINE:
                    return GetLine(current);
                case EditShape.RECTANGLE:
                    return GetRect(current);
                default: 
                    return null;
            }
        }

        public static List<Point> GetLine(Point current)
        {
            Point p1 = new Point((int)Math.Min(Start.X, current.X), (int)Math.Min(Start.Y, current.Y));
            Point p2 = new Point((int)Math.Max(Start.X, current.X), (int)Math.Max(Start.Y, current.Y));

            Point diff = p2 - p1;

            List<Point> list = new List<Point>();

            if(diff.Y > diff.X)
            {
                for(int i = p1.Y; i <= p2.Y; i++)
                {
                    list.Add(new Point(p1.X, i));
                }
            }
            else
            {
                for(int i = p1.X; i <= p2.X; i++)
                {
                    list.Add(new Point(i, p1.Y));
                }
            }

            return list;
        }

        public static List<Point> GetRect(Point current)
        {
            Point p1 = new Point((int)Math.Min(Start.X, current.X), (int)Math.Min(Start.Y, current.Y));
            Point p2 = new Point((int)Math.Max(Start.X, current.X), (int)Math.Max(Start.Y, current.Y));

            List<Point> list = new List<Point>();
    
            for(int x = p1.X; x <= p2.X; x++)
            {
                for(int y = p1.Y; y <= p2.Y; y++)
                {
                    list.Add(new Point(x, y));
                }
            }

            return list;
        }
    }
}
