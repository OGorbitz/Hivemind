using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.Utility
{
    public class ShapeSelecter
    {
        public static List<Point> GetCircle(Point position, float size)
        {
            List<Point> selected = new List<Point>();

            for(int i = position.X - (int)Math.Ceiling(size); i < position.X + (int)Math.Ceiling(size); i++)
            {
                for (int j = position.Y - (int)Math.Ceiling(size); j < position.Y + (int)Math.Ceiling(size); j++)
                {
                    Vector2 dist = new Vector2(i, j) - position.ToVector2();
                    if (dist.Length() < size)
                        selected.Add(new Point(i, j));
                }
            }

            return selected;
        }
    }
}
