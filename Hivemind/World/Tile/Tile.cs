using Hivemind.World.Entity;
using Hivemind.World.Tiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.World.Tiles
{
    public class Tile
    {
        static readonly int[,] Neighbors =
{
                {0, -1},
                {1, 0},
                {0, 1},
                {-1, 0},
                {0, 0},
                {-1, -1},
                {1, -1},
                {1, 1},
                {-1, 1},
            };

        private BaseFloor _floor;
        public BaseFloor Floor
        {
            get
            {
                return _floor;
            }
            set
            {
                _floor = value;
                if(_floor != null)
                    _floor.SetParent(this, Parent);

                for (var i = 0; i < Neighbors.GetLength(0); i++)
                {
                    Point v = new Point(Pos.X + Neighbors[i, 0], Pos.Y + Neighbors[i, 1]);
                    Tile t = Parent.GetTile(v);
                    if (t != null && t.Floor != null)
                        t.Floor.Dirty = true;
                }
            }
        }

        public HoloTile _holoFloor;
        public HoloTile HoloFloor
        {
            get
            {
                return _holoFloor;
            }
            set
            {
                _holoFloor = value;
                _holoFloor.SetParent(this, Parent);
            }
        }

        public BaseWall _wall;
        public BaseWall Wall
        {
            get
            {
                return _wall;
            }
            set
            {
                _wall = value;
                if(_wall != null)
                    _wall.SetParent(this, Parent);
                for (var i = 0; i < 4; i++)
                {
                    Point v = new Point(Pos.X + Neighbors[i, 0], Pos.Y + Neighbors[i, 1]);
                    Tile t = Parent.GetTile(v);
                    if (t != null && t.Wall != null)
                        t.Wall.Dirty = true;
                }

                if (Parent.GetType() == typeof(TileMap))
                {
                    if (value == null)
                    {
                        List<Room> rooms = new List<Room>();
                        Room biggest = null;

                        for (int i = 0; i < 4; i++)
                        {
                            Point p = Pos + new Point(Neighbors[i, 0], Neighbors[i, 1]);
                            Tile t = Parent.GetTile(p);
                            if (t != null && t.Room != null)
                            {
                                if (biggest == null)
                                    biggest = t.Room;
                                else if (biggest.Size < t.Room.Size)
                                {
                                    rooms.Add(biggest);
                                    biggest = t.Room;
                                }
                                else if (!rooms.Contains(t.Room))
                                    rooms.Add(t.Room);
                            }
                        }

                        if (biggest != null)
                        {
                            biggest.AddTile(Pos);
                            foreach (Room r in rooms)
                            {
                                biggest.MergeRoom(r);
                            }
                        }
                        else
                        {
                            ((TileMap)Parent).CreateRoom(Pos);
                        }
                    }
                    else
                    {
                        if(Room != null)
                            Room.SplitRoom(Pos);
                    }
                }
            }
        }
        public HoloTile _holoWall;
        public HoloTile HoloWall
        {
            get
            {
                return _holoWall;
            }
            set
            {
                _holoWall = value;
                if(value != null)
                    _holoWall.SetParent(this, Parent);
            }
        }

        public Visibility Visible;

        public TileEntity _tileEntity;
        public TileEntity TileEntity
        {
            get
            {
                return _tileEntity;
            }
            set
            {
                _tileEntity = value;
                if(value != null)
                    _tileEntity.TileMap = (TileMap)Parent;
            }
        }

        public ITileMap Parent;

        public readonly Point Pos;

        public readonly bool Real = true;

        public Room Room;
        public RoomTask RoomUpdate = RoomTask.NONE;

        public Rectangle WorldBounds
        {
            get
            {
                return new Rectangle(Pos.X * TileManager.TileSize, Pos.Y * TileManager.TileSize, TileManager.TileSize, TileManager.TileSize);
            }
        }

        public Tile(Point pos, ITileMap parent, bool real)
        {
            Real = real;
            Pos = pos;
            Visible = Visibility.HIDDEN;
            Parent = parent;
        }
        public Tile(Point pos, ITileMap parent)
        {
            Pos = pos;
            Visible = Visibility.HIDDEN;
            Parent = parent;
        }
    }
}
