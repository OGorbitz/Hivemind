using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Hivemind.World.Entity
{
    public class SpacialHash<T>
    {
        public Vector2 Size, CellSize;
        public HashCell<T>[,] Cells;

        public SpacialHash(Vector2 size, Vector2 cellsize)
        {
            Size = size;
            CellSize = cellsize;
            Vector2 hashSize = new Vector2((int)Math.Ceiling(size.X / cellsize.X), (int)Math.Ceiling(size.Y / cellsize.Y));

            Cells = new HashCell<T>[(int)hashSize.X, (int)hashSize.Y];
            for (int x = 0; x < hashSize.X; x++)
            {
                for (int y = 0; y < hashSize.Y; y++)
                {
                    var cell = new HashCell<T>(this, new Vector2(x, y));
                    Cells[x, y] = cell;
                }
            }
        }

        public HashCell<T> AddMember(Vector2 position, T member)
        {
            Point cpos = new Point((int)Math.Floor(position.X / CellSize.X), (int)Math.Floor(position.Y / CellSize.Y));

            if (cpos.X < Cells.GetLength(0) && cpos.Y < Cells.GetLength(1) && cpos.X >= 0 && cpos.Y >= 0)
            {
                HashCell<T> cell = Cells[cpos.X, cpos.Y];
                cell.AddMember(member);
                return cell;
            }
            return null;
        }

        public List<T> GetMembers(Rectangle region)
        {
            Vector2 start = new Vector2((int)Math.Floor(region.Left / CellSize.X), (int)Math.Floor(region.Top / CellSize.Y));
            Vector2 end = new Vector2((int)Math.Floor(region.Right / CellSize.X), (int)Math.Floor(region.Bottom / CellSize.Y));

            if (start.X < 0)
                start.X = 0;
            if (end.X < 0)
                end.X = 0;
            if (start.X > Cells.GetLength(0))
                start.X = Cells.GetLength(0);
            if (end.X > Cells.GetLength(0))
                end.X = Cells.GetLength(0);
            if (start.Y < 0)
                start.Y = 0;
            if (end.Y < 0)
                end.Y = 0;
            if (end.Y > Cells.GetLength(1))
                end.Y = Cells.GetLength(1);
            if (start.Y > Cells.GetLength(1))
                start.Y = Cells.GetLength(1);

            List<T> Fetched = new List<T>();

            for (int x = (int)start.X; x <= end.X; x++)
            {
                for (int y = (int)start.Y; y <= end.Y; y++)
                {
                    Fetched.AddRange(Cells[x, y].Members);
                }
            }

            return Fetched;
        }
    }

    public class HashCell<T>
    {
        public SpacialHash<T> Parent;
        public List<T> Members;

        public Vector2 CPos;
        public Vector2 Position;
        public Vector2 Size => Parent.CellSize;

        public HashCell(SpacialHash<T> parent, Vector2 cpos)
        {
            Members = new List<T>();
            Parent = parent;
            CPos = cpos;
            Position = CPos * Parent.CellSize;
        }

        public void AddMember(T member)
        {
            if (!Members.Contains(member))
            {
                Members.Add(member);
            }
        }

        public void RemoveMember(T member)
        {
            if (Members.Contains(member))
            {
                Members.Remove(member);
            }
        }

        public List<T> GetMembers()
        {
            return Members;
        }
    }
}
