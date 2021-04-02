using Hivemind.World.Entity;
using Hivemind.World.Entity.Moving;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Hivemind.World.Tiles
{
    public enum RoomTask { NONE, MERGE, SPLIT }
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
        public Dictionary<Point, RoomTile> Tiles;
        public List<DroppedMaterial> Materials = new List<DroppedMaterial>();

        public float GetMaterialAmount(Material m)
        {
            float amt = 0f;
            foreach(DroppedMaterial mat in Materials)
            {
                if (mat.MaterialType == m)
                    amt += mat.Amount;
            }
            return amt;
        }

        public List<DroppedMaterial> GetMaterials(Material m)
        {
            List<DroppedMaterial> rtn = new List<DroppedMaterial>();
            foreach(DroppedMaterial mat in Materials)
            {
                if (mat.MaterialType == m)
                    rtn.Add(mat);
            }
            return rtn;
        }

        public Room(Point position, TileMap tileMap)
        {
            List<Point> OpenTiles = new List<Point>();

            TileMap = tileMap;
            Tiles = new Dictionary<Point, RoomTile>();
            OpenTiles.Add(position);

            while (true)
            {
                if (OpenTiles.Count <= 0)
                    break;

                Point p = OpenTiles[0];
                OpenTiles.RemoveAt(0);

                RoomTile tile = new RoomTile(p, TileMap);

                if (tile.Usable)
                {
                    Tiles.TryAdd(p, tile);
                    tile.Tile.Room = this;

                    for (int i = 0; i < Neighbors.GetLength(0); i++)
                    {
                        Point pt = p + new Point(Neighbors[i, 0], Neighbors[i, 1]);
                        if (!Tiles.ContainsKey(pt) && !OpenTiles.Contains(pt))
                        {
                            OpenTiles.Add(pt);
                        }
                    }
                }
            }
        }

        public Room(Dictionary<Point, RoomTile> tiles, TileMap tileMap)
        {
            TileMap = tileMap;
            Tiles = tiles;

            foreach(KeyValuePair<Point, RoomTile> t in Tiles)
            {
                t.Value.Tile.Room = this;
            }
        }

        public void SplitRoom(Point pos)
        {
            if (Tiles.ContainsKey(pos))
            {
                Tiles[pos].Tile.Room = null;
                Tiles.Remove(pos);
            }
            else
                return;

            List<ScanRoom> rooms = new List<ScanRoom>();
            List<ScanRoom> finishedRooms = new List<ScanRoom>();

            for (int i = 0; i < Neighbors.GetLength(0); i++)
            {
                Point p = pos + new Point(Neighbors[i, 0], Neighbors[i, 1]);
                ScanRoom r = new ScanRoom();
                r.OpenTiles.Add(p);
                rooms.Add(r);
            }

            //For each unfinished splitroom
            while (rooms.Count > 1)
            {
                for(int i = rooms.Count - 1; i >= 0; i--)
                {
                    ScanRoom r = rooms[i];
                    if (r.OpenTiles.Count <= 0)
                    {
                        rooms.Remove(r);
                        finishedRooms.Add(r);
                        continue;
                    }

                    Point p = r.OpenTiles[0];
                    r.OpenTiles.RemoveAt(0);

                    RoomTile t = new RoomTile(p, TileMap);
                    if (t.Usable)
                    {
                        r.Tiles.TryAdd(p, t);

                        for (int j = 0; j < Neighbors.GetLength(0); j++)
                        {
                            Point pt = p + new Point(Neighbors[j, 0], Neighbors[j, 1]);
                            if (!r.Tiles.ContainsKey(pt) && !r.ClosedTiles.Contains(pt) && !r.OpenTiles.Contains(pt))
                            {
                                r.OpenTiles.Add(pt);
                            }
                        }

                        foreach (ScanRoom sr in rooms)
                        {
                            if (sr == r)
                                continue;
                            if (sr.Tiles.ContainsKey(t.Pos))
                            {
                                sr.Merge(r);
                                rooms.Remove(r);
                                break;
                            }
                        }
                    }
                    else
                    {
                        r.ClosedTiles.Add(p);
                        if(r.Tiles.Count == 0)
                        {
                            rooms.Remove(r);
                        }
                    }
                }
            }

            foreach(ScanRoom r in finishedRooms)
            {
                RemoveTiles(r.Tiles);

                TileMap.Rooms.Add(new Room(r.Tiles, TileMap));
            }

            for (int i = Materials.Count - 1; i >= 0; i--)
            {
                DroppedMaterial m = Materials[i];
                Tile t = TileMap.GetTile(TileMap.GetTileCoords(m.Pos));
                if (t.Room != this)
                    m.Room = t.Room;
            }

        }

        public void MergeRoom(Room r)
        {
            for(int n = r.Materials.Count - 1; n >= 0; n--)
            {
                r.Materials[n].Room = this;
            }
            foreach (KeyValuePair<Point, RoomTile> t in r.Tiles){
                if (!Tiles.ContainsKey(t.Key))
                {
                    AddTile(t.Key);
                }
            }
        }

        public void AddTile(Point p)
        {
            if (Tiles.ContainsKey(p))
                return;

            RoomTile tile = new RoomTile(p, TileMap);

            if (tile.Usable)
            {
                Tiles.TryAdd(p, tile);
                tile.Tile.Room = this;
                Rectangle bounds = tile.Tile.WorldBounds;
                foreach (MovingEntity m in TileMap.GetEntities(bounds))
                {
                    if (m.GetType() == typeof(DroppedMaterial) && bounds.Contains(m.Pos))
                        ((DroppedMaterial)m).Room = this;
                }
            }
        }

        public void AddTiles(Dictionary<Point, RoomTile> tiles)
        {
            foreach(KeyValuePair<Point, RoomTile> t in tiles)
            {
                if (!Tiles.ContainsKey(t.Key))
                    Tiles.Add(t.Key, t.Value);
            }
        }

        public void RemoveTiles(Dictionary<Point, RoomTile> tiles)
        {
            foreach (KeyValuePair<Point, RoomTile> t in tiles)
            {
                if (Tiles.ContainsKey(t.Key))
                    Tiles.Remove(t.Key);
            }
        }

        public void Destroy()
        {
            foreach(DroppedMaterial m in Materials)
            {
                m.Room = null;
            }
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

    public class ScanRoom
    {
        public Dictionary<Point, RoomTile> Tiles = new Dictionary<Point, RoomTile>();
        public List<Point> OpenTiles = new List<Point>();
        public List<Point> ClosedTiles = new List<Point>();

        public void Merge(ScanRoom r)
        {
            foreach(KeyValuePair<Point, RoomTile> t in r.Tiles)
            {
                if (!Tiles.ContainsKey(t.Key))
                    Tiles.Add(t.Key, t.Value);
            }
            foreach(Point p in r.OpenTiles)
            {
                if (!OpenTiles.Contains(p))
                    OpenTiles.Add(p);
            }
        }
    }
}
