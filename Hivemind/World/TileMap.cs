using Hivemind.Input;
using Hivemind.World.Colony;
using Hivemind.World.Entity;
using Hivemind.World.Entity.Moving;
using Hivemind.World.Entity.Tile;
using Hivemind.World.Generator;
using Hivemind.World.Tiles;
using Hivemind.World.Tiles.Floor;
using Hivemind.World.Tiles.Wall;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Hivemind.World
{
    public interface ITileMap
    {
        public Tile GetTile(Point position);
        public float GetLayerDepth(int y);
    }

    [Serializable]
    public class TileMap : ITileMap
    {
        //Owned objects
        public Camera Cam;
        private RenderTarget2D RenderTarget;
        public WorldGenerator Generator;

        //Holds pre rendered floor
        public RenderTarget2D FloorBuffer, FogBuffer;
        //Used to render floor
        public static RenderTarget2D RenderBuffer;
        //Blend state for rendering stencil effect
        public static BlendState StencilBlendState = new BlendState
        {
            ColorSourceBlend = Blend.DestinationAlpha,
            ColorBlendFunction = BlendFunction.Add,
            ColorDestinationBlend = Blend.Zero,
            AlphaSourceBlend = Blend.Zero,
            AlphaBlendFunction = BlendFunction.ReverseSubtract,
            AlphaDestinationBlend = Blend.One
        };

        public static BlendState FogBlendState = new BlendState
        {
            ColorSourceBlend = Blend.SourceAlpha,
            ColorBlendFunction = BlendFunction.Add,
            ColorDestinationBlend = Blend.Zero,
            AlphaSourceBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.Add,
            AlphaDestinationBlend = Blend.Zero
        };

        //Specific data
        public int Size;
        public Rectangle Bounds
        {
            get
            {
                return new Rectangle(0, 0, Size, Size);
            }
        }

        private Tile[,] Tiles;
        private Dictionary<int, MovingEntity> Entities;
        private SpacialHash<MovingEntity> EntityHash;
        public Dictionary<Point, TileEntity> TileEntities;
        public List<Room> Rooms = new List<Room>();

        public TaskManager TaskManager;

        public Point BufferPosition = Point.Zero, BufferOffset = Point.Zero, BufferSize = Point.Zero;
        public bool Updated, Rendered;


        //Creation, Destruction
        public TileMap(int s)
        {
            Size = s;
            
            Tiles = new Tile[s, s];
            for(int i = 0; i < Tiles.GetLength(0); i++)
            {
                for(int j = 0; j < Tiles.GetLength(1); j++)
                {
                    Tiles[i, j] = new Tile(new Point(i, j), this);
                }
            }


            Cam = new Camera(this);
            Generator = new WorldGenerator(69l, this);
            Entities = new Dictionary<int, MovingEntity>();
            EntityHash = new SpacialHash<MovingEntity>(new Vector2(Size * TileManager.TileSize), new Vector2(TileManager.TileSize * 4));
            TileEntities = new Dictionary<Point, TileEntity>();

            TaskManager = new TaskManager(this);

            FloorBuffer = null;

            Generate();

            RecalcRooms();
        }

        public void Generate()
        {
            for (var x = 0; x < Size; x++)
            {
                for (var y = 0; y < Size; y++)
                {
                    Tile tile = Tiles[x, y];

                    var pos = new Point(x, y);
                    var rr = new Rectangle(5, 5, 5, 3);

                    var temp = Generator.GetTemperature(pos.ToVector2());

                    if (temp > 0.35)
                        SetTile(pos, new Wall_Cinderblock());

                    if (temp > 0.25)
                        SetTile(pos, new Floor_Concrete());
                    else if (temp > -0.25)
                    {
                        SetTile(pos, new Floor_Dirt());
                    }
                    else
                    {
                        SetTile(pos, new Floor_Grass());
                        var d = Generator.GetNoise1(pos.ToVector2());
                        if (d > Generator.BushOffset && d < Generator.BushOffset + Generator.BushChance)
                        {
                            SetTileEntity(new Bush1(pos));
                        }
                    }
                    if(temp < 0.25)
                    {
                        var d = Generator.GetNoise2(pos.ToVector2());
                        if (d > Generator.RockOffset && d < Generator.RockOffset + Generator.RockChance)
                        {
                            if (GetTileEntity(pos) == null)
                                SetTileEntity(new Rock1(pos));
                        }
                    }

                }
            }
            AddEntity(new Worker(new Vector2(8 * TileManager.TileSize, 8 * TileManager.TileSize)));
            SetTile(new Point(10, 10), new HoloTile(new Wall_Cinderblock()));
            
            
            AddEntity(new Nommer(new Vector2(30 * TileManager.TileSize, 25 * TileManager.TileSize)));

            Rectangle r = new Rectangle(30, 21, 7, 7);
            Vector2 v = new Vector2(33, 24);
            for(int x = r.Left; x < r.Right; x++)
            {
                for(int y = r.Top; y < r.Bottom; y++)
                {
                    Vector2 t = new Vector2(x, y);
                    if((v - t).Length() < 3.5)
                    {
                        SetTile(t.ToPoint(), new Floor_Dirt());
                        BaseWall w = GetTile(t.ToPoint()).Wall;
                        if (w != null)
                            w.Destroy();
                    }
                }
            }
            SetTileEntity(new SpaseShip(new Point(32, 23)));
        }

        public TileMap(SerializationInfo info, StreamingContext context)
        {
            Size = Tiles.GetLength(0);
            
            FloorBuffer = null;
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            /*var al = new ArrayList();
            for (var x = 0; x < Size; x++)
                for (var y = 0; y < Size; y++)
                    if (TileEntities[x, y] != null && TileEntities[x, y].cpos == new Vector2(x, y))
                        al.Add(TileEntities[x, y]);
            info.AddValue("TileEntities", al);*/
            info.AddValue("Tiles", Tiles);
        }

        public float GetLayerDepth(int y)
        {
            return (float) y / (float) Size;
        }

        public void Dispose()
        {
            if (FloorBuffer != null)
                FloorBuffer.Dispose();

            GC.SuppressFinalize(this);
        }

        //Utility functions
        /// <summary>
        /// Converts world position to tile coordinates
        /// </summary>
        /// <param name="coords">World position <see cref="Point"/></param>
        /// <returns>A <see cref="Point"/> containing tile coordinates</returns>
        public static Point GetTileCoords(Vector2 coords)
        {
            var wx = coords.X / TileManager.TileSize;
            var wy = coords.Y / TileManager.TileSize;

            if (wx < 0)
                wx = (float)Math.Floor(wx);
            if (wy < 0)
                wy = (float)Math.Floor(wy);
            return new Point((int)wx, (int)wy);
        }

        public static Point GetTileCoords(Point coords)
        {
            return new Point(coords.X / TileManager.TileSize, coords.Y / TileManager.TileSize);
        }

        /// <summary>
        /// Checks if a vector is within bounds of the Tilemap
        /// </summary>
        /// <param name="position">A position vector</param>
        /// <returns>True if within bounds</returns>
        public bool InBounds(Point position)
        {
            if (position.X < 0 || position.X >= Size)
                return false;
            if (position.Y < 0 || position.Y >= Size)
                return false;
            return true;
        }

        /// <summary>
        /// Returns the tile at the given position and layer
        /// </summary>
        /// <returns><see cref="BaseTile"/> if position is within bounds and tile is not null <br/>
        /// Returns null otherwise</returns>
        public Tile GetTile(Point position)
        {
            if (InBounds(position))
            {
                return Tiles[position.X, position.Y];
            }
            return new Tile(position, this, false);
        }

        /// <summary>
        /// Sets array value to the tile, at it's given position. Does nothing if out of bounds.
        /// </summary>
        /// <param name="tile">Tile to be set</param>
        public void SetTile(Point pos, BaseTile tile)
        {
            if (InBounds(pos))
            {
                Tile t = GetTile(pos);
                switch (tile.Layer)
                {
                    case Layer.WALL:
                        t.Wall = (BaseWall)tile;
                        break;
                    case Layer.FLOOR:
                        t.Floor = (BaseFloor)tile;
                        break;
                }
            }
        }

        public void SetTile(Point pos, HoloTile tile)
        {
            if (InBounds(pos))
            {
                switch (tile.Layer)
                {
                    case Layer.WALL:
                        GetTile(pos).HoloWall = tile;
                        break;
                    case Layer.FLOOR:
                        GetTile(pos).HoloFloor = tile;
                        break;
                }
                Dictionary<Material, float> required = new Dictionary<Material, float>();
                for(int i = 0; i < tile.CostMaterials.Length; i++)
                {
                    required.Add(tile.CostMaterials[i], tile.CostAmounts[i]);
                }
                TaskManager.AddTask(new HaulTask(TaskManager, tile, required));
            }
        }

        public MovingEntity GetEntity(int id)
        {
            if (Entities.ContainsKey(id))
                return Entities[id];
            return null;
        }

        public List<MovingEntity> GetEntities(Rectangle region)
        {
            List<MovingEntity> entities = EntityHash.GetMembers(region);

            for(int i = entities.Count - 1; i > 0; i--)
            {
                if (!region.Intersects(entities[i].Bounds))
                    entities.RemoveAt(i);
            }

            return entities;
        }

        public void AddEntity(MovingEntity entity)
        {
            if (!Entities.ContainsKey(entity.ID))
            {
                Entities.Add(entity.ID, entity);
            }
            entity.TileMap = this;
            entity.Cell = EntityHash.AddMember(entity.Pos, entity);
            entity.Init();
        }

        public void RemoveEntity(MovingEntity entity)
        {
            if (Entities.ContainsKey(entity.ID))
                Entities.Remove(entity.ID);
        }

        public TileEntity GetTileEntity(Point pos)
        {
            if (InBounds(pos))
                return Tiles[(int)pos.X, (int)pos.Y].TileEntity;
            return null;
        }

        public List<TileEntity> GetTileEntities(Rectangle region)
        {
            Point start = new Point(region.Left, region.Top);
            Point end = new Point(region.Right, region.Bottom);

            if (start.X < 0)
                start.X = 0;
            if (end.X < 0)
                end.X = 0;
            if (start.X > Size)
                start.X = Size;
            if (end.X > Size)
                end.X = Size;
            if (start.Y < 0)
                start.Y = 0;
            if (end.Y < 0)
                end.Y = 0;
            if (end.Y > Size)
                end.Y = Size;
            if (start.Y > Size)
                start.Y = Size;

            List<TileEntity> Fetched = new List<TileEntity>();

            for (int x = (int)start.X; x < end.X; x++)
            {
                for (int y = (int)start.Y; y < end.Y; y++)
                {
                    TileEntity te = GetTileEntity(new Point(x, y));
                    if (te != null && !Fetched.Contains(te))
                        Fetched.Add(te);
                }
            }

            return Fetched;
        }

        public void SetTileEntity(TileEntity entity)
        {
            if (Bounds.Contains(entity.Bounds))
            {
                for(int x = entity.Bounds.Left; x < entity.Bounds.Right; x++)
                {
                    for(int y = entity.Bounds.Top; y < entity.Bounds.Bottom; y++)
                    {
                        var t = GetTile(new Point(x, y));
                        if(t.TileEntity != null)
                            t.TileEntity.Destroy();
                        t.TileEntity = entity;
                        TileEntities.Add(t.Pos, entity);
                    }
                }
            }
        }

        public void RemoveTileEntity(Point pos)
        {
            if (InBounds(pos))
                GetTile(pos).TileEntity = null;
        }

        public void RemoveRoom(Room r)
        {
            if (Rooms.Contains(r))
                Rooms.Remove(r);
        }

        public void CreateRoom(Point p)
        {
            Tile t = GetTile(p);
            if (t != null && t.Room == null)
            {
                Room r = new Room(p, this);
                Rooms.Add(r);
            }
        }

        public void RecalcRooms()
        {
            Rooms.Clear();
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    GetTile(new Point(i, j)).Room = null;
                }
            }
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    CreateRoom(new Point(i, j));
                }
            }
        }

        /// <summary>
        /// Requests the floor at the given position to be rerendered
        /// </summary>
        /// <param name="position"></param>
        public void RenderFloor(Point position)
        {
            Tile t = GetTile(position);
            if (t == null || t.Floor == null)
                return;
            t.Floor.Dirty = true;
        }

        public void Update(GameTime gameTime)
        {
            Updated = !Updated;

            foreach (KeyValuePair<int, MovingEntity> e in Entities) { 
                e.Value.Update(gameTime);
            }

            foreach (var e in Tiles)
            {
                if (e.TileEntity != null)
                    if (e.TileEntity.Updated = Updated)
                    {
                        e.TileEntity.Updated = !e.TileEntity.Updated;
                        e.TileEntity.Update(gameTime);
                    }
                if (e.Floor != null)
                    e.Floor.Update(gameTime);
                if (e.Wall != null)
                    e.Wall.Update(gameTime);
            }

            UpdateFog();
        }

        public void UpdateFog()
        {
            for(int i = 0; i < Size; i++)
            {
                for(int j = 0; j < Size; j++)
                {
                    Tile t = GetTile(new Point(i, j));
                    if (t.Visibility == Visibility.VISIBLE)
                        t.Visibility = Visibility.KNOWN;
                }
            }
            foreach(KeyValuePair<int, MovingEntity> e in Entities)
            {
                e.Value.UpdateVision();
            }
            foreach(KeyValuePair<Point, TileEntity> e in TileEntities)
            {
                e.Value.UpdateVision();
            }
        }

        public void UpdateBufferPosition()
        {
            Rectangle cam = Cam.GetBounds();

            bool run = true;

            while (run)
            {
                run = false;

                Point bdiff = GetTileCoords(new Vector2(cam.Left, cam.Top) - (BufferPosition.ToVector2() * TileManager.TileSize));

                if (Math.Abs(bdiff.X - BufferOffset.X) > BufferSize.X || Math.Abs(bdiff.Y - BufferOffset.Y) > BufferSize.Y)
                {
                    BufferPosition.X += (bdiff.X - (bdiff.X % BufferSize.X));
                    BufferOffset.X = bdiff.X % BufferSize.X;
                    BufferPosition.Y += bdiff.Y - (bdiff.Y % BufferSize.Y);
                    BufferOffset.Y = bdiff.Y % BufferSize.Y;

                    for (int x = BufferPosition.X + BufferOffset.X; x <= BufferPosition.X + BufferOffset.X + BufferSize.X; x++)
                    {
                        for (int y = BufferPosition.Y + BufferOffset.Y; y <= BufferPosition.Y + BufferOffset.Y + BufferSize.Y; y++)
                        {
                            RenderFloor(new Point(x, y));
                        }
                    }
                }
                else
                {
                    if (bdiff.X > BufferOffset.X)
                    {
                        run = true;
                        BufferOffset.X += 1;
                        int x = BufferPosition.X + BufferOffset.X + BufferSize.X - 1;
                        for (int y = BufferPosition.Y + BufferOffset.Y; y <= BufferPosition.Y + BufferOffset.Y + BufferSize.Y; y++)
                        {
                            RenderFloor(new Point(x, y));
                        }
                    }
                    if (bdiff.X < BufferOffset.X)
                    {
                        run = true;
                        BufferOffset.X -= 1;
                        int x = BufferPosition.X + BufferOffset.X;
                        for (int y = BufferPosition.Y + BufferOffset.Y; y <= BufferPosition.Y + BufferOffset.Y + BufferSize.Y; y++)
                        {
                            RenderFloor(new Point(x, y));
                        }
                    }
                    if (bdiff.Y > BufferOffset.Y)
                    {
                        run = true;
                        BufferOffset.Y += 1;
                        int y = BufferPosition.Y + BufferOffset.Y + BufferSize.Y - 1;
                        for (int x = BufferPosition.X + BufferOffset.X; x <= BufferPosition.X + BufferOffset.X + BufferSize.X; x++)
                        {
                            RenderFloor(new Point(x, y));
                        }
                    }
                    if (bdiff.Y < BufferOffset.Y)
                    {
                        run = true;
                        BufferOffset.Y -= 1;
                        int y = BufferPosition.Y + BufferOffset.Y;
                        for (int x = BufferPosition.X + BufferOffset.X; x <= BufferPosition.X + BufferOffset.X + BufferSize.X; x++)
                        {
                            RenderFloor(new Point(x, y));
                        }
                    }
                }
                if (BufferOffset.X >= BufferSize.X)
                {
                    BufferOffset.X -= BufferSize.X;
                    BufferPosition.X += BufferSize.X;
                }
                if (BufferOffset.X < 0)
                {
                    BufferOffset.X += BufferSize.X;
                    BufferPosition.X -= BufferSize.X;
                }
                if (BufferOffset.Y >= BufferSize.Y)
                {
                    BufferOffset.Y -= BufferSize.Y;
                    BufferPosition.Y += BufferSize.Y;
                }
                if (BufferOffset.Y < 0)
                {
                    BufferOffset.Y += BufferSize.Y;
                    BufferPosition.Y -= BufferSize.Y;
                }
            }
        }

        public static readonly int[,,] CornerCheck =
        {
            { { 0, -1 }, { -1, -1 }, { -1, 0 } },
            { { 1, 0 }, { 1, -1 }, { 0, -1 } },
            { { 0, 1 }, { 1, 1 }, { 1, 0 } },
            { { -1, 0 }, { -1, 1 }, { 0, 1 } },

        };

        public static readonly Vector2[] CornerRenderOffset =
        {
            new Vector2(0, 0),
            new Vector2(32, 0),
            new Vector2(32, 32),
            new Vector2(0, 32)
        };

        public void DrawFog(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            for (int l = 0; l < (int)FloorPriority.COUNT; l++)
            {
                graphicsDevice.SetRenderTarget(FogBuffer);

                for (int x = BufferPosition.X + BufferOffset.X; x < BufferPosition.X + BufferSize.X + BufferOffset.X; x++)
                {
                    for (int y = BufferPosition.Y + BufferOffset.Y; y < BufferPosition.Y + BufferSize.Y + BufferOffset.Y; y++)
                    {
                        var ti = GetTile(new Point(x, y));
                        if (ti == null || !ti.Real)
                            continue;
                        if (!ti.DirtyFog)
                            continue;

                        Point converted_coords = new Point(x - BufferPosition.X, y - BufferPosition.Y);
                        if (converted_coords.X >= BufferSize.X)
                            converted_coords.X -= BufferSize.X;
                        if (converted_coords.Y >= BufferSize.Y)
                            converted_coords.Y -= BufferSize.Y;


                        spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: FogBlendState);
                        spriteBatch.Draw(Fog.MaskAtlas, converted_coords.ToVector2() * TileManager.TileSize,
                            sourceRectangle: new Rectangle(256, 64, 64, 64), Color.White);
                        spriteBatch.End();


                        spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
                        var visibility = GetTile(new Point(x, y)).Visibility;
                        if (visibility == Visibility.HIDDEN)
                            spriteBatch.Draw(Fog.MaskAtlas, converted_coords.ToVector2() * TileManager.TileSize,
                                sourceRectangle: new Rectangle(256, 0, 64, 64), Color.White);
                        else if(visibility == Visibility.KNOWN)
                        {
                            spriteBatch.Draw(Fog.MaskAtlas, converted_coords.ToVector2() * TileManager.TileSize,
                                sourceRectangle: new Rectangle(256, 0, 64, 64), new Color(0f, 0f, 0f, 0.5f));
                            for (int i = 0; i < 4; i++)
                            {
                                int index = 0;

                                //For each indexed corner tile
                                for (int n = 0; n < 3; n++)
                                {
                                    if (GetTile(new Point(CornerCheck[i, n, 0] + x, CornerCheck[i, n, 1] + y)).Visibility == Visibility.HIDDEN)
                                        index += 1 << n;
                                }

                                spriteBatch.Draw(Fog.MaskAtlas, converted_coords.ToVector2() * TileManager.TileSize + CornerRenderOffset[i],
                                sourceRectangle: new Rectangle(index * 32, i * 32, 32, 32), Color.White);
                            }
                        }
                        else
                        {
                            //For each corner
                            for (int i = 0; i < 4; i++)
                            {
                                int indexhidden = 0;
                                int indexknown = 0;


                                //For each indexed corner tile
                                for (int n = 0; n < 3; n++)
                                {
                                    var vis = GetTile(new Point(CornerCheck[i, n, 0] + x, CornerCheck[i, n, 1] + y)).Visibility;
                                    if (vis == Visibility.HIDDEN)
                                        indexhidden += 1 << n;
                                    if (vis == Visibility.KNOWN)
                                        indexknown += 1 << n;
                                }

                                spriteBatch.Draw(Fog.MaskAtlas, converted_coords.ToVector2() * TileManager.TileSize + CornerRenderOffset[i],
                                sourceRectangle: new Rectangle(indexhidden * 32, i * 32, 32, 32), Color.White);

                                spriteBatch.Draw(Fog.MaskAtlas, converted_coords.ToVector2() * TileManager.TileSize + CornerRenderOffset[i],
                                sourceRectangle: new Rectangle(indexknown * 32, i * 32, 32, 32), new Color(0, 0, 0, 0.5f));
                            }
                        }
                        spriteBatch.End();
                    }
                }

            }

            for (int x = BufferPosition.X + BufferOffset.X; x < BufferPosition.X + BufferSize.X + BufferOffset.X; x++)
            {
                for (int y = BufferPosition.Y + BufferOffset.Y; y < BufferPosition.Y + BufferSize.Y + BufferOffset.Y; y++)
                {
                    var tile = GetTile(new Point(x, y));
                    if (tile == null || tile.Floor == null)
                        continue;
                    tile.DirtyFog = false;
                }
            }
        }

        public void DrawFloor(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, GameTime gameTime)
        {


            for (int l = 0; l < (int)FloorPriority.COUNT; l++)
            {
                graphicsDevice.SetRenderTarget(RenderBuffer);
                graphicsDevice.Clear(Color.Transparent);

                spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
                for (int x = BufferPosition.X + BufferOffset.X; x < BufferPosition.X + BufferSize.X + BufferOffset.X; x++)
                {
                    for (int y = BufferPosition.Y + BufferOffset.Y; y < BufferPosition.Y + BufferSize.Y + BufferOffset.Y; y++)
                    {
                        var ti = GetTile(new Point(x, y));
                        if (ti == null)
                            continue;
                        var tile = ti.Floor;
                        if (tile == null)
                            continue;
                        if (!tile.Dirty)
                            continue;

                        Point converted_coords = new Point(x - BufferPosition.X, y - BufferPosition.Y);
                        if (converted_coords.X >= BufferSize.X)
                            converted_coords.X -= BufferSize.X;
                        if (converted_coords.Y >= BufferSize.Y)
                            converted_coords.Y -= BufferSize.Y;


                        if (tile.FloorLayer == l)
                        {
                            spriteBatch.Draw(FloorMask.Solid, new Vector2(converted_coords.X * TileManager.TileSize, converted_coords.Y * TileManager.TileSize), Color.White);
                        }
                        else if (tile.FloorLayer < l)
                        {
                            int index = 0;
                            for (int n = 0; n < 8; n++)
                            {
                                var ctile = GetTile(new Point(x + FloorMask.indices[n, 0], y + FloorMask.indices[n, 1]));
                                if (ctile == null || ctile.Floor == null)
                                    continue;
                                else if (ctile.Floor.FloorLayer >= l) //Tile is "solid" for layer
                                {
                                    index += 1 << n;
                                }
                            }
                            if (index == 0)
                                continue;

                            spriteBatch.Draw(FloorMask.MaskAtlas, converted_coords.ToVector2() * TileManager.TileSize,
                                sourceRectangle: new Rectangle(index * 64, 0, 64, 64), Color.White);
                        }
                    }
                }

                spriteBatch.End();

                spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: StencilBlendState);

                Texture2D t = FloorMask.Textures[l];

                for (int x = BufferPosition.X + BufferOffset.X; x < BufferPosition.X + BufferSize.X + BufferOffset.X; x++)
                {
                    for (int y = BufferPosition.Y + BufferOffset.Y; y < BufferPosition.Y + BufferSize.Y + BufferOffset.Y; y++)
                    {
                        Point converted_coords = new Point(x - BufferPosition.X, y - BufferPosition.Y);
                        if (converted_coords.X >= BufferSize.X)
                            converted_coords.X -= BufferSize.X;
                        if (converted_coords.Y >= BufferSize.Y)
                            converted_coords.Y -= BufferSize.Y;

                        Rectangle sourceRectangle = new Rectangle((int)(x * TileManager.TileSize % t.Width), (int)(y * TileManager.TileSize % t.Height), TileManager.TileSize, TileManager.TileSize);
                        spriteBatch.Draw(t, position: converted_coords.ToVector2() * TileManager.TileSize, sourceRectangle: sourceRectangle, Color.White);
                    }
                }

                spriteBatch.End();

                graphicsDevice.SetRenderTarget(FloorBuffer);

                spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
                spriteBatch.Draw(RenderBuffer, Vector2.Zero, Color.White);
                spriteBatch.End();
            }

            for (int x = BufferPosition.X + BufferOffset.X; x < BufferPosition.X + BufferSize.X + BufferOffset.X; x++)
            {
                for (int y = BufferPosition.Y + BufferOffset.Y; y < BufferPosition.Y + BufferSize.Y + BufferOffset.Y; y++)
                {
                    var tile = GetTile(new Point(x, y));
                    if (tile == null || tile.Floor == null)
                        continue;
                    tile.Floor.Dirty = false;
                }
            }
        }


        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            var campos = Cam.Pos;

            if (RenderTarget == null)
                RenderTarget = new RenderTarget2D(
                    graphicsDevice,
                    graphicsDevice.PresentationParameters.BackBufferWidth * 2,
                    graphicsDevice.PresentationParameters.BackBufferHeight * 2,
                    false,
                    graphicsDevice.PresentationParameters.BackBufferFormat,
                    DepthFormat.Depth24);

            int width = 1 + (int) Math.Ceiling((float)RenderTarget.Width / (float)TileManager.TileSize);
            int height = 1 + (int)Math.Ceiling((float)RenderTarget.Height / (float)TileManager.TileSize);

            BufferSize = new Point(width, height);

            width *= TileManager.TileSize;
            height *= TileManager.TileSize;

            
            if (FloorBuffer == null)
                FloorBuffer = new RenderTarget2D(graphicsDevice, width, height, false, SurfaceFormat.Vector4, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);
            if (RenderBuffer == null)
                RenderBuffer = new RenderTarget2D(graphicsDevice, width, height);
            if (FogBuffer == null)
                FogBuffer = new RenderTarget2D(graphicsDevice, width, height, false, SurfaceFormat.Vector4, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);

            UpdateBufferPosition();

            DrawFloor(spriteBatch, graphicsDevice, gameTime);
            DrawFog(spriteBatch, graphicsDevice, gameTime);

            graphicsDevice.SetRenderTarget(RenderTarget);

            spriteBatch.Begin(transformMatrix: Cam.Translate, samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            spriteBatch.Draw(FloorBuffer, BufferPosition.ToVector2() * TileManager.TileSize, Color.White);
            spriteBatch.Draw(FloorBuffer, (BufferPosition.ToVector2() + new Vector2(BufferSize.X, 0)) * TileManager.TileSize, Color.White);
            spriteBatch.Draw(FloorBuffer, (BufferPosition.ToVector2() + new Vector2(0, BufferSize.Y)) * TileManager.TileSize, Color.White);
            spriteBatch.Draw(FloorBuffer, (BufferPosition + BufferSize).ToVector2() * TileManager.TileSize, Color.White);
            spriteBatch.End();


            //TODO: Render entities
            Rendered = !Rendered;

            var b = Cam.GetScaledBounds();
            Point p1 = GetTileCoords(new Point(b.Left, b.Top));
            Point p2 = GetTileCoords(new Point(b.Right, b.Bottom)) + new Point(2);

            if (p1.X < 0)
                p1.X = 0;
            if (p1.X >= Size)
                p1.X = Size;
            if (p1.Y < 0)
                p1.Y = 0;
            if (p1.Y >= Size)
                p1.Y = Size;
            if (p2.X < 0)
                p2.X = 0;
            if (p2.X >= Size)
                p2.X = Size;
            if (p2.Y < 0)
                p2.Y = 0;
            if (p2.Y >= Size)
                p2.Y = Size;

            spriteBatch.Begin(transformMatrix: Cam.Translate, samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            for (int x = (int)p1.X; x < p2.X; x++)
            {
                for (int y = (int)p1.Y; y < p2.Y; y++)
                {
                    HoloTile t = GetTile(new Point(x, y)).HoloFloor;
                    if (t != null)
                    {
                        t.Draw(spriteBatch);
                    }
                }
            }
            spriteBatch.End();


            spriteBatch.Begin(transformMatrix: Cam.Translate, samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);

            for (int x = (int)p1.X; x < p2.X; x++)
            {
                for (int y = (int)p1.Y; y < p2.Y; y++)
                {


                    TileEntity e = GetTile(new Point(x, y)).TileEntity;
                    if (e != null)
                        if (e.Rendered == null || e.Rendered == Rendered)
                        {
                            e.Rendered = !Rendered;
                            e.Draw(spriteBatch, gameTime);
                        }
                }
            }


            foreach (KeyValuePair<int, MovingEntity> e in Entities)
            {
                e.Value.Draw(spriteBatch, gameTime);
            }

            for (int x = (int)p1.X; x < p2.X; x++)
            {
                for (int y = (int)p1.Y; y < p2.Y; y++)
                {
                    BaseTile t = GetTile(new Point(x, y)).Wall;
                    if(t != null)
                    {
                        t.Draw(spriteBatch);
                    }

                    HoloTile ht = GetTile(new Point(x, y)).HoloWall;
                    if (ht != null)
                    {
                        ht.Draw(spriteBatch);
                    }
                }
            }
            spriteBatch.End();

            spriteBatch.Begin(transformMatrix: Cam.Translate, samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            spriteBatch.Draw(FogBuffer, BufferPosition.ToVector2() * TileManager.TileSize, Color.White);
            spriteBatch.Draw(FogBuffer, (BufferPosition.ToVector2() + new Vector2(BufferSize.X, 0)) * TileManager.TileSize, Color.White);
            spriteBatch.Draw(FogBuffer, (BufferPosition.ToVector2() + new Vector2(0, BufferSize.Y)) * TileManager.TileSize, Color.White);
            spriteBatch.Draw(FogBuffer, (BufferPosition + BufferSize).ToVector2() * TileManager.TileSize, Color.White);
            spriteBatch.End();


            spriteBatch.Begin(transformMatrix: Cam.Translate, samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            Selection.DrawSelectionRectangles(spriteBatch, graphicsDevice, gameTime);
            spriteBatch.End();

            //TODO: Render particles


            graphicsDevice.SetRenderTarget(null);
            graphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(transformMatrix: Cam.ScaleOffset, samplerState: SamplerState.PointClamp);
            spriteBatch.Draw(RenderTarget, Vector2.Zero, Color.White);
            spriteBatch.End();

        }
    }
}
