using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.World
{
    class ScrollingBuffer
    {
        public Point BufferPosition { get; private set; }
        public Point BufferSize { get; private set; }

        public ScrollingBuffer(Point bufferSize)
        {
            BufferSize = bufferSize;
        }

        public List<Point> GetDrawnPoints(Point newPosition)
        {
            List<Point> pointsToDraw = new List<Point>();

            bool run = true;
            while (run)
            {
                run = false;

                Point bdiff = newPosition - BufferPosition;

                if (Math.Abs(bdiff.X) > BufferSize.X || Math.Abs(bdiff.Y) > BufferSize.Y)
                {
                    BufferPosition = newPosition;

                    for (int x = BufferPosition.X; x <= BufferPosition.X + BufferSize.X; x++)
                    {
                        for (int y = BufferPosition.Y; y <= BufferPosition.Y + BufferSize.Y; y++)
                        {
                            pointsToDraw.Add(new Point(x, y));
                        }
                    }
                }
                else
                {
                    if (bdiff.X > 0)
                    {
                        run = true;
                        BufferPosition += new Point(1, 0);
                        int x = BufferPosition.X + BufferSize.X - 1;
                        for (int y = BufferPosition.Y; y <= BufferPosition.Y + BufferSize.Y; y++)
                        {
                            pointsToDraw.Add(new Point(x, y));
                        }
                    }
                    if (bdiff.X < 0)
                    {
                        run = true;
                        BufferPosition += new Point(-1, 0);
                        int x = BufferPosition.X;
                        for (int y = BufferPosition.Y; y <= BufferPosition.Y + BufferSize.Y; y++)
                        {
                            pointsToDraw.Add(new Point(x, y));
                        }
                    }
                    if (bdiff.Y > 0)
                    {
                        run = true;
                        BufferPosition += new Point(0, 1);

                        int y = BufferPosition.Y + BufferSize.Y - 1;
                        for (int x = BufferPosition.X; x <= BufferPosition.X + BufferSize.X; x++)
                        {
                            pointsToDraw.Add(new Point(x, y));
                        }
                    }
                    if (bdiff.Y < 0)
                    {
                        run = true;
                        BufferPosition += new Point(0, -1);
                        int y = BufferPosition.Y;
                        for (int x = BufferPosition.X; x <= BufferPosition.X + BufferSize.X; x++)
                        {
                            pointsToDraw.Add(new Point(x, y));
                        }
                    }
                }

            }

            return pointsToDraw;
        }

        //To be called to "refresh" the whole buffer, will return all BufferPoints contained
        //As opposed to returning only the new ones as it moves.
        public List<Point> GetAllPoints()
        {
            List<Point> pointsToDraw = new List<Point>();
            for(int x = BufferPosition.X; x < BufferPosition.X + BufferSize.X; x++)
            {
                for(int y = BufferPosition.Y; y < BufferPosition.Y + BufferSize.Y; y++)
                {
                    pointsToDraw.Add(new Point(x, y));
                }
            }
            return pointsToDraw;
        }

        public Point GetBufferPosition(Point p)
        {
            return new Point(p.X % BufferSize.X, p.Y % BufferSize.Y);
        }
    }
}
