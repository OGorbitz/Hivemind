using Hivemind.World.Entity;
using Hivemind.World.Tiles.Utilities;
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
                if (value != null)
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

        public PowerCable _powerCable;
        public PowerCable PowerCable
        {
            get
            {
                return _powerCable;
            }
            set
            {
                _powerCable = value;
                DirtyCable = true;

                if (value != null)
                {
                    _powerCable.SetParent(this, Parent);
                    for (var i = 0; i < 4; i++)
                    {
                        Point v = new Point(Pos.X + Neighbors[i, 0], Pos.Y + Neighbors[i, 1]);
                        Tile t = Parent.GetTile(v);
                        if (t != null)
                            t.DirtyCable = true;
                    }
                }

            }
        }

        private HoloTile _holoPowerCable;
        public HoloTile HoloPowerCable
        {
            get
            {
                return _holoPowerCable;
            }
            set
            {
                _holoPowerCable = value;
                if (value != null)
                    _holoPowerCable.SetParent(this, Parent);
            }
        }

        public Visibility _visibility;
        public Visibility _unpushedVisibility;
        public Visibility Visibility
        {
            get
            {
                return _visibility;
            }
            set
            {
                _unpushedVisibility = value;
            }
        }

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
                if(value != null && Parent.GetType() == typeof(TileMap))
                    _tileEntity.TileMap = (TileMap)Parent;
            }
        }

        public ITileMap Parent;

        public readonly Point Pos;

        public readonly bool Real = true;

        public bool DirtyFog = true;
        public bool DirtyCable = true;

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
            Visibility = Visibility.HIDDEN;
            Parent = parent;
        }
        public Tile(Point pos, ITileMap parent)
        {
            Pos = pos;
            Visibility = Visibility.HIDDEN;
            Parent = parent;
        }

        public void PushVisibility()
        {
            if (_unpushedVisibility != _visibility)
            {
                for (var i = 0; i < 9; i++)
                {
                    Point v = new Point(Pos.X + Neighbors[i, 0], Pos.Y + Neighbors[i, 1]);
                    Tile t = Parent.GetTile(v);
                    if (t != null)
                        t.DirtyFog = true;
                }
                _visibility = _unpushedVisibility;
            }
        }
    }
}
