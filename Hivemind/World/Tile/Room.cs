using Hivemind.World.Entity;
using Hivemind.World.Entity.Moving;
using Hivemind.World.Tiles;
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
            OpenTiles.Add(position);

            CheckRoom();
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

                if (tile.Usable)
                {
                    Tiles.TryAdd(add, tile);
                    tile.Tile.Room = this;

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

        public void Destroy()
        {
            foreach(KeyValuePair<Point, RoomTile> k in Tiles)
            {
                k.Value.Tile.Room = null;
            }
            TileMap.RemoveRoom(this);
        }
    }

    public class RoomTile
    {
        public bool Usable;

        public Point Pos;
        public Tile Tile;
        public BaseFloor Floor => Tile.Floor;
        public BaseWall Wall => Tile.Wall;

        public List<BaseEntity> Entities;
        public TileEntity TileEntity;

        public RoomTile(Point position, TileMap tileMap)
        {
            Pos = position;

            Tile = tileMap.GetTile(position);

            if (!Tile.Real)
                Usable = false;
            else if (Wall != null)
                Usable = false;
            else
                Usable = true;
        }
    }
}
