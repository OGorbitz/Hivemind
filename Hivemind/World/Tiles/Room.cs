using Hivemind.World.Entity;
using Hivemind.World.Entity.Moving;
using Hivemind.World.Tile;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.World.Tiles
{
    public class Room
    {
        int[,] Neighbors = new int[,]
        {
            { 0, 1 },
            { 1, 0 },
            { 0, -1 },
            { -1, 0 }
        };

        public int Size
        {
            get { return Tiles.Count; }
        }
        
        TileMap TileMap;
        List<Point> OpenTiles = new List<Point>();
        Dictionary<Point, RoomTile> Tiles = new Dictionary<Point, RoomTile>();
        Dictionary<Material, float> Materials = new Dictionary<Material, float>();

        public Room(Point position, TileMap tileMap)
        {
            TileMap = tileMap;
            Tiles.TryAdd(position, new RoomTile(position, tileMap));
            
            for(int i = 0; i < Neighbors.GetLength(0); i++)
            {
                OpenTiles.Add(position + new Point(Neighbors[i, 0], Neighbors[i, 1]));
            }
        }

        public void CheckRoom()
        {
            while (true)
            {
                if (OpenTiles.Count <= 0)
                    break;

                Point add = OpenTiles[0];
                OpenTiles.RemoveAt(0);

                RoomTile tile = new RoomTile(add, TileMap);
                Tiles.TryAdd(add, tile);

                if (tile.Usable)
                {
                    tile.Floor.Room = this;

                    for (int i = 0; i < Neighbors.GetLength(0); i++)
                    {
                        Point p = add + new Point(Neighbors[i, 0], Neighbors[i, 1]);
                        if (!Tiles.ContainsKey(p) && !OpenTiles.Contains(p))
                        {
                            OpenTiles.Add(p);
                        }
                    }
                }
            }
        }
    }

    public class RoomTile
    {
        public bool Usable;

        public Point Pos;
        public BaseFloor Floor;
        public BaseWall Wall;

        public List<BaseEntity> Entities;
        public TileEntity TileEntity;

        public RoomTile(Point position, TileMap tileMap)
        {
            Pos = position;
            Floor = (BaseFloor)tileMap.GetTile(Pos, Layer.FLOOR);
            Wall = (BaseWall)tileMap.GetTile(Pos, Layer.WALL);

            if (Wall != null)
                Usable = false;
            else
                Usable = true;
        }
    }
}
