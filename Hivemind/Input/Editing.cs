using Hivemind.World;
using Hivemind.World.Tile;
using Hivemind.World.Tile.Wall;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.Input
{
    public enum EditShape { SINGLE, LINE, RECTANGLE }
    public enum PlacingType { TILE, ENTITY, TILE_ENTITY }
    public enum Action { BUILD, DESTROY, SELECT, DEBUG_BUILD }

    public class Editing
    {
        public static EditShape Shape = EditShape.LINE;
        public static PlacingType PlacingType { get; private set; } = PlacingType.TILE;
        public static Type SelectedType { get; private set; } = typeof(Wall_Cinderblock);

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
            WorldManager.GetEditorTileMap().SetTile((BaseTile)Activator.CreateInstance(SelectedType, location));
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
                HoloTile t = new HoloTile((BaseTile)Activator.CreateInstance(SelectedType, start));
                WorldManager.GetActiveTileMap().SetTile(t);
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
                        HoloTile t = new HoloTile((BaseTile)Activator.CreateInstance(SelectedType, current));
                        WorldManager.GetActiveTileMap().SetTile(t);
                        Start = current;
                    }
                    else if (action == Action.DESTROY)
                    {
                        BaseTile t = WorldManager.GetActiveTileMap().GetTile(current, Layer.WALL);
                        if (t != null)
                            t.Destroy();
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
                        BaseTile t = (BaseTile)Activator.CreateInstance(SelectedType, p);
                        WorldManager.GetEditorTileMap().SetTile(t);
                    }

                }
                else if (action == Action.DESTROY)
                {
                    foreach (Point p in GetAffectedTiles(current))
                    {

                        BaseTile t = WorldManager.GetActiveTileMap().GetTile(p, Layer.WALL);
                        if (t != null)
                            WorldManager.GetEditorTileMap().SetTile((BaseTile)Activator.CreateInstance(t.GetType(), p));
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
                        BaseTile t = WorldManager.GetActiveTileMap().GetTile(p, Layer.WALL);
                        if (t != null)
                            t.Destroy();
                    }
                    else if(action == Action.BUILD)
                    {
                        HoloTile t = new HoloTile((BaseTile)Activator.CreateInstance(SelectedType, p));
                        WorldManager.GetActiveTileMap().SetTile(t);
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
