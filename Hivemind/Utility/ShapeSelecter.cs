using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.Utility
{
    public class ShapeSelecter
    {
        private static int _rotation = 0;
        
        public static int Rotation
        {
            get 
            { 
                return _rotation; 
            }
            set 
            { 
                int i = value % 4;
                _rotation = i < 0 ? i + 4 : i;
            }
        }

        public static List<Point> GetCircle(Vector2 position, float size)
        {
            List<Point> selected = new List<Point>();

            for(int i = (int)position.X - (int)Math.Ceiling(size); i < position.X + (int)Math.Ceiling(size); i++)
            {
                for (int j = (int)position.Y - (int)Math.Ceiling(size); j < position.Y + (int)Math.Ceiling(size); j++)
                {
                    Vector2 dist = new Vector2(i, j) - position;
                    if (dist.Length() < size)
                        selected.Add(new Point(i, j));
                }
            }

            return selected;
        }
    }
}
