using Hivemind.World;
using Hivemind.World.Tiles;
using Hivemind.World.Tiles.Wall;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.Input
{
    public enum EditShape { SINGLE, LINE, RECTANGLE }
    public enum PlacingType { TILE, ENTITY, TILE_ENTITY,
        NONE
    }
    public enum Action { BUILD, DESTROY, SELECT, DEBUG_BUILD }

    public class Editing
    {
        public static EditShape Shape = EditShape.LINE;
        public static PlacingType PlacingType { get; private set; } = PlacingType.TILE;
        public static Type SelectedType { get; private set; } = typeof(Wall_Cinderblock);
        public static bool PlaceHolo = true;

        public static Point Start;

        public static void SetType(Type selectedType, PlacingType placetype)
        {
            SelectedType = selectedType; 
            PlacingType = placetype;
        }

        /// <summary>
        /// Shows holographic placement on editor tilemap
        /// </summary>
        /// <param name="location"></param>
        public static void Hover(Point location)
        {
            WorldManager.GetEditorTileMap().SetTile(location, (BaseTile)Activator.CreateInstance(SelectedType));
        }


        public static void SetTile(Point p)
        {
            if (PlaceHolo)
            {
                HoloTile t = new HoloTile((BaseTile)Activator.CreateInstance(SelectedType));
                WorldManager.GetActiveTileMap().SetTile(p, t);
            }
            else
            {
                BaseTile t = (BaseTile)Activator.CreateInstance(SelectedType);
                WorldManager.GetActiveTileMap().SetTile(p, t);
            }
        }

        public static void SetEditorTile(Point p, bool d)
        {
            if (d)
            {
                Tile t = WorldManager.GetActiveTileMap().GetTile(p);
                if (t.Wall != null)
                {
                    BaseTile tile = (BaseTile)Activator.CreateInstance(t.Wall.GetType());
                    WorldManager.GetEditorTileMap().SetTile(p, tile);
                }
            }
            else
            {
                BaseTile tile = (BaseTile)Activator.CreateInstance(SelectedType);
                WorldManager.GetEditorTileMap().SetTile(p, tile);
            }

        }

        public static void RemoveTile(Point p)
        {
            BaseWall t = WorldManager.GetActiveTileMap().GetTile(p).Wall;
            if (t != null)
                t.Destroy();
        }

        /// <summary>
        /// Set start point
        /// </summary>
        /// <param name="start"></param>
        public static void StartEditing(Point start)
        {
            Start = start;

            if(Shape == EditShape.SINGLE)
            {
                SetTile(start);
            }
        }

        /// <summary>
        /// Update editor tilemap
        /// </summary>
        /// <param name="current"></param>
        public static void UpdateEditing(Point current, Action action)
        {
            if(Shape == EditShape.SINGLE)
            {
                if(Start != current)
                {
                    if(action == Action.BUILD)
                    {
                        SetTile(current);
                        Start = current;
                    }
                    else if (action == Action.DESTROY)
                    {
                        RemoveTile(current);
                        Start = current;
                    }
                    
                }
            }
            else
            {
                if (action == Action.BUILD)
                {
                    foreach (Point p in GetAffectedTiles(current))
                    {
                        SetEditorTile(p, false);
                    }

                }
                else if (action == Action.DESTROY)
                {
                    foreach (Point p in GetAffectedTiles(current))
                    {
                        SetEditorTile(p, true);
                    }
                }

            }
        }

        /// <summary>
        /// Apply all changes
        /// </summary>
        /// <param name="end"></param>
        public static void EndEditing(Point end, Action action)
        {
            if (Shape != EditShape.SINGLE)
            {
                foreach (Point p in GetAffectedTiles(end))
                {
                    if (action == Action.DESTROY)
                    {
                        RemoveTile(p);
                    }
                    else if(action == Action.BUILD)
                    {
                        SetTile(p);
                    }
                }
            }
        }

        public static List<Point> GetAffectedTiles(Point end)
        {
            

            switch (Shape)
            {
                case EditShape.SINGLE:
                    List<Point> tiles = new List<Point>();
                    tiles.Add(end);
                    return tiles;
                case EditShape.LINE:
                    return GetLine(end);
                case EditShape.RECTANGLE:
                    return GetRect(end);
                default: 
                    return null;
            }
        }

        public static List<Point> GetLine(Point end)
        {
            Point diff = end - Start;

            List<Point> list = new List<Point>();

            if(Math.Abs(diff.Y) > Math.Abs(diff.X))
            {
                int s = Math.Min(Start.Y, end.Y);
                int e = Math.Max(Start.Y, end.Y);
                for(int i = s; i <= e; i++)
                {
                    list.Add(new Point(Start.X, i));
                }
            }
            else
            {
                int s = Math.Min(Start.X, end.X);
                int e = Math.Max(Start.X, end.X);
                for (int i = s; i <= e; i++)
                {
                    list.Add(new Point(i, Start.Y));
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
