using Hivemind.Input;
using Hivemind.Utility;
using Hivemind.World.Colony;
using Hivemind.World.Entity;
using Hivemind.World.Entity.Moving;
using Hivemind.World.Entity.Projectile;
using Hivemind.World.Entity.Tile;
using Hivemind.World.Generator;
using Hivemind.World.Particle;
using Hivemind.World.Tiles;
using Hivemind.World.Tiles.Floor;
using Hivemind.World.Tiles.Utilities;
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
        private ScrollingBuffer _buffer;
        private List<Point> DirtyPoints;
        private RenderTarget2D RenderTarget;
        public WorldGenerator Generator;

        public Rectangle RenderBounds;

        public static List<double> TimeFloor = new List<double>(), TimeFog = new List<double>(), TimeWalls = new List<double>(), TimeEntities = new List<double>();
        public static double AvgTimeFloor, AvgTimeFog, AvgTimeWalls, AvgTimeEntities;

        //Holds pre rendered floor
        public RenderTarget2D FloorBuffer, FogBuffer, FogDrawn, StaticBuffer, CableBuffer;
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

        public static BlendState OverWriteState = new BlendState
        {
            ColorSourceBlend = Blend.One,
            ColorBlendFunction = BlendFunction.Add,
            ColorDestinationBlend = Blend.Zero,
            AlphaSourceBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.Add,
            AlphaDestinationBlend = Blend.Zero
        };

        static readonly Color FogColor = new Color(0f, 0f, 0f, 1f);
        static readonly Color FogKnownColor = new Color(FogColor.R, FogColor.G, FogColor.B, 0.65f);

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
        public List<BaseProjectile> Projectiles = new List<BaseProjectile>();
        public List<ParticleSource> ParticleSources = new List<ParticleSource>();
        public Dictionary<int, MovingEntity> Entities = new Dictionary<int, MovingEntity>();
        public Dictionary<Point, TileEntity> TileEntities = new Dictionary<Point, TileEntity>();
        private SpacialHash<MovingEntity> EntityHash;
        public List<Room> Rooms = new List<Room>();

        public TaskManager TaskManager;

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
            EntityHash = new SpacialHash<MovingEntity>(new Vector2(Size * TileManager.TileSize), new Vector2(TileManager.TileSize * 4));

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
                    if (x >= 20 && x < 40 && y == 23)
                        GetTile(new Point(x, y)).PowerCable = (PowerCable) TileConstructor.ConstructTile("PowerCableT1");
                }
            }
            AddEntity(new Worker(new Vector2(8 * TileManager.TileSize, 8 * TileManager.TileSize)));
            AddEntity(new Worker(new Vector2(20 * TileManager.TileSize, 8 * TileManager.TileSize)));
            AddEntity(new Worker(new Vector2(15 * TileManager.TileSize, 15 * TileManager.TileSize)));
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

        public void AddProjectile(BaseProjectile projectile)
        {
            Projectiles.Add(projectile);
        }

        public void RemoveProjectile(BaseProjectile projectile)
        {
            Projectiles.Remove(projectile);
        }

        public void UpdateProjectiles(GameTime gameTime)
        {
            foreach(BaseProjectile p in Projectiles)
            {
                p.Update(gameTime);
            }
        }

        public void AddParticleSource(ParticleSource particleSource)
        {
            ParticleSources.Add(particleSource);
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
            if (position.X < 0 || position.X >= Size || position.Y < 0 || position.Y >= Size)
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
                    case Layer.POWER:
                        t.PowerCable = (PowerCable)tile;
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
                    case Layer.POWER:
                        GetTile(pos).HoloPowerCable = tile;
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
                entity.TileMap = this;
                entity.OnPlace();
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
        public void BufferMoved(Point position)
        {
            Tile t = GetTile(position);
            t.DirtyFog = true;
            if (t.Floor == null)
                return;
            t.Floor.Dirty = true;
        }

        public void Update(GameTime gameTime)
        {
            Updated = !Updated;

            foreach (KeyValuePair<int, MovingEntity> e in Entities) { 
                e.Value.Update(gameTime);
            }

            if (false)
            {
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
            }

            foreach(KeyValuePair<Point, TileEntity> e in TileEntities)
            {
                if (e.Value != null)
                    if (e.Value.Updated = Updated)
                    {
                        e.Value.Updated = !e.Value.Updated;
                        e.Value.Update(gameTime);
                    }
            }

            for(int i = Projectiles.Count - 1; i >= 0; i--)
            {
                Projectiles[i].Update(gameTime);
            }

            UpdateFog();
        }

        public void UpdateFog()
        {
            for (int x = RenderBounds.Left; x < RenderBounds.Right; x++)
            {
                for (int y = RenderBounds.Top; y < RenderBounds.Bottom; y++)
                {
                    Tile t = GetTile(new Point(x, y));
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
            for (int x = RenderBounds.Left; x < RenderBounds.Right; x++)
            {
                for (int y = RenderBounds.Top; y < RenderBounds.Bottom; y++)
                {
                    Tile t = GetTile(new Point(x, y));
                    t.PushVisibility();
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
            graphicsDevice.SetRenderTarget(FogBuffer);

            for (int x = _buffer.BufferPosition.X; x < _buffer.BufferPosition.X + _buffer.BufferSize.X; x++)
            {
                for (int y = _buffer.BufferPosition.Y; y < _buffer.BufferPosition.Y + _buffer.BufferSize.Y; y++)
                {
                    var ti = GetTile(new Point(x, y));
                    //if tile is null, ignore
                    if (ti == null || !ti.Real)
                        continue;

                    //if point is in dirtypoints, needs to be updated, so ignore other continues
                    if (!DirtyPoints.Contains(new Point(x, y)))
                    {
                        if (!ti.DirtyFog == null)
                            continue;
                    }

                    //convert coords to buffer-specific coordinates
                    Point buffer_coords = _buffer.GetBufferPosition(new Point(x, y));


                    spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: FogBlendState);
                    spriteBatch.Draw(Fog.MaskAtlas, buffer_coords.ToVector2() * TileManager.TileSize,
                        sourceRectangle: new Rectangle(256, 64, 64, 64), Color.White);
                    spriteBatch.End();


                    spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
                    var visibility = GetTile(new Point(x, y)).Visibility;
                    if (visibility == Visibility.HIDDEN)
                        spriteBatch.Draw(Fog.MaskAtlas, buffer_coords.ToVector2() * TileManager.TileSize,
                            sourceRectangle: new Rectangle(256, 0, 64, 64), FogColor);
                    else if (visibility == Visibility.KNOWN)
                    {
                        spriteBatch.Draw(Fog.MaskAtlas, buffer_coords.ToVector2() * TileManager.TileSize,
                            sourceRectangle: new Rectangle(256, 0, 64, 64), FogKnownColor);
                        for (int i = 0; i < 4; i++)
                        {
                            int index = 0;

                            //For each indexed corner tile
                            for (int n = 0; n < 3; n++)
                            {
                                if (GetTile(new Point(CornerCheck[i, n, 0] + x, CornerCheck[i, n, 1] + y)).Visibility == Visibility.HIDDEN)
                                    index += 1 << n;
                            }

                            spriteBatch.Draw(Fog.MaskAtlas, buffer_coords.ToVector2() * TileManager.TileSize + CornerRenderOffset[i],
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

                            spriteBatch.Draw(Fog.MaskAtlas, buffer_coords.ToVector2() * TileManager.TileSize + CornerRenderOffset[i],
                            sourceRectangle: new Rectangle(indexhidden * 32, i * 32, 32, 32), FogColor);

                            spriteBatch.Draw(Fog.MaskAtlas, buffer_coords.ToVector2() * TileManager.TileSize + CornerRenderOffset[i],
                            sourceRectangle: new Rectangle(indexknown * 32, i * 32, 32, 32), FogKnownColor);
                        }
                    }
                    spriteBatch.End();
                }
            }

            for (int x = _buffer.BufferPosition.X; x < _buffer.BufferPosition.X + _buffer.BufferSize.X; x++)
            {
                for (int y = _buffer.BufferPosition.Y; y < _buffer.BufferPosition.Y + _buffer.BufferSize.Y; y++)
                {
                    var tile = GetTile(new Point(x, y));
                    if (tile == null || tile.Floor == null)
                        continue;
                    tile.DirtyFog = false;
                }
            }
            foreach(Point p in DirtyPoints)
            {
                var tile = GetTile(p);
                if (tile == null || tile.Floor == null)
                    continue;
                tile.DirtyFog = false;
            }

            graphicsDevice.SetRenderTarget(FogDrawn);
            graphicsDevice.Clear(Color.Transparent);

            Vector2 bufferpos = _buffer.BufferPosition.ToVector2();
            bufferpos -= new Vector2(bufferpos.X % _buffer.BufferSize.X, bufferpos.Y % _buffer.BufferSize.Y);

            spriteBatch.Begin(transformMatrix: Cam.TranslateScaleOffset, samplerState: SamplerState.PointClamp);
            spriteBatch.Draw(FogBuffer, bufferpos * TileManager.TileSize, Color.White);
            spriteBatch.Draw(FogBuffer, bufferpos * TileManager.TileSize + new Vector2(FloorBuffer.Width, 0), Color.White);
            spriteBatch.Draw(FogBuffer, bufferpos * TileManager.TileSize + new Vector2(0, FloorBuffer.Height), Color.White);
            spriteBatch.Draw(FogBuffer, bufferpos * TileManager.TileSize + new Vector2(FloorBuffer.Width, FloorBuffer.Height), Color.White);
            spriteBatch.End();

            graphicsDevice.SetRenderTarget(StaticBuffer);

            graphicsDevice.Clear(new Color(0f, 0.06f, 0f));
            var ms = Hivemind.CurrentGameTime.TotalGameTime.Seconds * 1000 + Hivemind.CurrentGameTime.TotalGameTime.Milliseconds;
            var t = (int)(ms % 3000 / 3000f * 96f);
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            for (var x = -t; x <= graphicsDevice.Viewport.Height; x += Hivemind.ComputerLines.Height * 5)
                spriteBatch.Draw(Hivemind.ComputerLines,
                    new Rectangle(new Point(0, x),
                        new Point(graphicsDevice.Viewport.Width, Hivemind.ComputerLines.Height * 5)),
                    new Color(1f, 1f, 1f, 0.25f));
            t = (int)(ms % 2000 / 2000f * 64f);
            for (var x = t - 64; x <= graphicsDevice.Viewport.Height; x += Hivemind.ComputerLines.Height * 2)
                spriteBatch.Draw(Hivemind.ComputerLines,
                    new Rectangle(new Point(0, x),
                        new Point(graphicsDevice.Viewport.Width, Hivemind.ComputerLines.Height * 2)),
                    new Color(1f, 1f, 1f, 0.3f));
            spriteBatch.End();

            graphicsDevice.SetRenderTarget(FogDrawn);
            spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: StencilBlendState);
            spriteBatch.Draw(StaticBuffer, Vector2.Zero, Color.White);
            spriteBatch.End();
        }

        public void DrawFloor(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            for(int l = 0; l < (int)FloorPriority.COUNT; l++)
            {
                graphicsDevice.SetRenderTarget(RenderBuffer);
                graphicsDevice.Clear(Color.Transparent);

                //Draw alpha masks for the texture
                spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
                for (int x = _buffer.BufferPosition.X; x < _buffer.BufferPosition.X + _buffer.BufferSize.X; x++)
                {
                    for (int y = _buffer.BufferPosition.Y; y < _buffer.BufferPosition.Y + _buffer.BufferSize.Y; y++)
                    {
                        var ti = GetTile(new Point(x, y));
                        //if tile is null, ignore
                        if (ti == null)
                            continue;
                        var floor = ti.Floor;
                        if (floor == null)
                            continue;

                        //if point is in dirtypoints, needs to be updated, so ignore other continues
                        if (!DirtyPoints.Contains(new Point(x, y)))
                        {
                            if (!floor.Dirty)
                                continue;
                        }

                        //convert coords to buffer-specific coordinates
                        Point buffer_coords = _buffer.GetBufferPosition(new Point(x, y));

                        if (floor.FloorLayer == l)
                        {
                            //if this tile is on the currently drawn layer, draw a full square for mask
                            spriteBatch.Draw(FloorMask.DirtMask, buffer_coords.ToVector2() * TileManager.TileSize,
                                sourceRectangle: new Rectangle(256, 0, 64, 64), Color.White);
                        }
                        else if (floor.FloorLayer < l)
                        {
                            //if this tile is below the current layer, calculate which mask to use
                            for (int i = 0; i < 4; i++)
                            {
                                int index = 0;
                                bool draw = false;

                                //For each indexed corner tile
                                for (int n = 0; n < 3; n++)
                                {
                                    var ctile = GetTile(new Point(CornerCheck[i, n, 0] + x, CornerCheck[i, n, 1] + y));
                                    if (ctile == null || ctile.Floor == null)
                                        continue;
                                    if (ctile.Floor.FloorLayer >= l)
                                        index += 1 << n;
                                    if (ctile.Floor.FloorLayer == l)
                                        draw = true;
                                }
                                if (index == 0 || draw == false)
                                    continue;

                                spriteBatch.Draw(FloorMask.DirtMask, buffer_coords.ToVector2() * TileManager.TileSize + CornerRenderOffset[i],
                                sourceRectangle: new Rectangle(index * 32, i * 32, 32, 32), Color.White);
                            }
                        }
                    }
                }
                spriteBatch.End();

                spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: StencilBlendState);

                Texture2D t = FloorMask.Textures[l];

                //Draw texture over mask
                for (int x = _buffer.BufferPosition.X; x < _buffer.BufferPosition.X + _buffer.BufferSize.X; x++)
                {
                    for (int y = _buffer.BufferPosition.Y; y < _buffer.BufferPosition.Y + _buffer.BufferSize.Y; y++)
                    {
                        Point buffer_coords = _buffer.GetBufferPosition(new Point(x, y));

                        Rectangle sourceRectangle = new Rectangle((int)(x * TileManager.TileSize % t.Width), (int)(y * TileManager.TileSize % t.Height), TileManager.TileSize, TileManager.TileSize);
                        spriteBatch.Draw(t, position: buffer_coords.ToVector2() * TileManager.TileSize, sourceRectangle: sourceRectangle, Color.White);
                    }
                }

                spriteBatch.End();

                graphicsDevice.SetRenderTarget(FloorBuffer);

                //Draw final product onto the buffer
                spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
                spriteBatch.Draw(RenderBuffer, Vector2.Zero, Color.White);
                spriteBatch.End();

                //Draw border overlays
                spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
                for (int x = _buffer.BufferPosition.X; x < _buffer.BufferPosition.X + _buffer.BufferSize.X; x++)
                {
                    for (int y = _buffer.BufferPosition.Y; y < _buffer.BufferPosition.Y + _buffer.BufferSize.Y; y++)
                    {
                        var ti = GetTile(new Point(x, y));
                        //if tile is null, ignore
                        if (ti == null)
                            continue;
                        var floor = ti.Floor;
                        if (floor == null)
                            continue;

                        //if point is in dirtypoints, needs to be updated, so ignore other continues
                        if (!DirtyPoints.Contains(new Point(x, y)))
                        {
                            if (!floor.Dirty)
                                continue;
                        }

                        Point buffer_coords = _buffer.GetBufferPosition(new Point(x, y));

                        if (floor.FloorLayer < l)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                int index = 0;

                                //For each indexed corner tile
                                for (int n = 0; n < 3; n++)
                                {
                                    var ctile = GetTile(new Point(CornerCheck[i, n, 0] + x, CornerCheck[i, n, 1] + y));
                                    if (ctile == null || ctile.Floor == null)
                                        continue;
                                    if (ctile.Floor.FloorLayer >= l)
                                        index += 1 << n;
                                }
                                if (index == 0)
                                    continue;

                                spriteBatch.Draw(FloorMask.DirtOverlay, buffer_coords.ToVector2() * TileManager.TileSize + CornerRenderOffset[i],
                                sourceRectangle: new Rectangle(index * 32, i * 32, 32, 32), Color.White);
                            }
                        }
                    }
                }

                spriteBatch.End();
            }

            for (int x = _buffer.BufferPosition.X; x < _buffer.BufferPosition.X + _buffer.BufferSize.X; x++)
            {
                for (int y = _buffer.BufferPosition.Y; y < _buffer.BufferPosition.Y + _buffer.BufferSize.Y; y++)
                {
                    var tile = GetTile(new Point(x, y));
                    if (tile == null || tile.Floor == null)
                        continue;
                    tile.Floor.Dirty = false;
                }
            }
            foreach(Point p in DirtyPoints)
            {
                var tile = GetTile(p);
                if (tile == null || tile.Floor == null)
                    continue;
                tile.Floor.Dirty = false;
            }
        }

        public readonly int[,] neighbors =
{
            { -1, 0 },
            { 0, -1 },
            { 1, 0 },
            { 0, 1 }
        };

        public void DrawCables(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            graphicsDevice.SetRenderTarget(CableBuffer);


            //Draw alpha masks for the texture
            spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: OverWriteState);
            for (int x = _buffer.BufferPosition.X; x < _buffer.BufferPosition.X + _buffer.BufferSize.X; x++)
            {
                for (int y = _buffer.BufferPosition.Y; y < _buffer.BufferPosition.Y + _buffer.BufferSize.Y; y++)
                {
                    var ti = GetTile(new Point(x, y));
                    //if tile is null, ignore

                    //convert coords to buffer-specific coordinates
                    Point buffer_coords = _buffer.GetBufferPosition(new Point(x, y));

                    if (DirtyPoints.Contains(new Point(x, y)))
                        spriteBatch.Draw(Helper.pixel, new Rectangle(new Point(TileManager.TileSize) * buffer_coords, new Point(TileManager.TileSize)), Color.Transparent);

                }
            }
            spriteBatch.End();
            spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            for (int x = _buffer.BufferPosition.X; x < _buffer.BufferPosition.X + _buffer.BufferSize.X; x++)
            {
                for (int y = _buffer.BufferPosition.Y; y < _buffer.BufferPosition.Y + _buffer.BufferSize.Y; y++)
                {
                    var ti = GetTile(new Point(x, y));
                    //if tile is null, ignore

                    //convert coords to buffer-specific coordinates
                    Point buffer_coords = _buffer.GetBufferPosition(new Point(x, y));

                    if (ti == null)
                        continue;
                    if (ti.PowerCable == null)
                        continue;
                    if (!ti.DirtyCable && !DirtyPoints.Contains(new Point(x, y)))
                        continue;

                    var powercable = ti.PowerCable;

                    //if point is in dirtypoints, needs to be updated, so ignore other continues

                    powercable.Draw(spriteBatch, Color.White, buffer_coords * new Point(TileManager.TileSize));

                }
            }
            spriteBatch.End();

            for (int x = _buffer.BufferPosition.X; x < _buffer.BufferPosition.X + _buffer.BufferSize.X; x++)
            {
                for (int y = _buffer.BufferPosition.Y; y < _buffer.BufferPosition.Y + _buffer.BufferSize.Y; y++)
                {
                    var tile = GetTile(new Point(x, y));
                    if (tile == null || tile.Floor == null)
                        continue;
                    tile.DirtyCable = false;
                }
            }
            foreach (Point p in DirtyPoints)
            {
                var tile = GetTile(p);
                if (tile == null || tile.Floor == null)
                    continue;
                tile.DirtyCable = false;
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

            if(_buffer == null)
            {
                _buffer = new ScrollingBuffer(new Point(width, height));
                DirtyPoints = _buffer.GetAllPoints();
            }
            else
            {
                Rectangle cam = Cam.GetBounds();
                DirtyPoints = _buffer.GetDrawnPoints(GetTileCoords(new Vector2(cam.Left, cam.Top)));
            }

            width *= TileManager.TileSize;
            height *= TileManager.TileSize;

            //Initialize all buffers
            if (FloorBuffer == null)
                FloorBuffer = new RenderTarget2D(graphicsDevice, width, height, false, SurfaceFormat.Vector4, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);
            if (RenderBuffer == null)
                RenderBuffer = new RenderTarget2D(graphicsDevice, width, height);
            if (FogBuffer == null)
                FogBuffer = new RenderTarget2D(graphicsDevice, width, height, false, SurfaceFormat.Vector4, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);
            if (FogDrawn == null)
                FogDrawn = new RenderTarget2D(graphicsDevice, graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight, false, SurfaceFormat.Vector4, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);
            if(StaticBuffer == null)
                StaticBuffer = new RenderTarget2D(graphicsDevice, graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight, false, SurfaceFormat.Vector4, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);
            if (CableBuffer == null)
                CableBuffer = new RenderTarget2D(graphicsDevice, width, height, false, SurfaceFormat.Vector4, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);


            //Draw floor to buffer
            DateTime StartTime = DateTime.Now;
            DrawFloor(spriteBatch, graphicsDevice, gameTime);
            TimeFloor.Add((DateTime.Now - StartTime).TotalMilliseconds);

            //Draw fog to buffer
            StartTime = DateTime.Now;
            DrawFog(spriteBatch, graphicsDevice, gameTime);
            TimeFog.Add((DateTime.Now - StartTime).TotalMilliseconds);

            //Draw cables to buffer
            DrawCables(spriteBatch, graphicsDevice, gameTime);

            
            //Switch to main buffer
            graphicsDevice.SetRenderTarget(RenderTarget);

            //Calculate required offset for buffer rendering
            Vector2 bufferpos = _buffer.BufferPosition.ToVector2();
            bufferpos -= new Vector2(bufferpos.X % _buffer.BufferSize.X, bufferpos.Y % _buffer.BufferSize.Y);

            //Draw floor buffer
            spriteBatch.Begin(transformMatrix: Cam.Translate, samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            spriteBatch.Draw(FloorBuffer, bufferpos * TileManager.TileSize, Color.White);
            spriteBatch.Draw(FloorBuffer, bufferpos * TileManager.TileSize + new Vector2(FloorBuffer.Width, 0), Color.White);
            spriteBatch.Draw(FloorBuffer, bufferpos * TileManager.TileSize + new Vector2(0, FloorBuffer.Height), Color.White);
            spriteBatch.Draw(FloorBuffer, bufferpos * TileManager.TileSize + new Vector2(FloorBuffer.Width, FloorBuffer.Height), Color.White);
            spriteBatch.End();

            //Required for proper rendering of tile entities TODO: change this to a better method
            Rendered = !Rendered;

            //Calculate camera bounds for render culling
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

            RenderBounds = new Rectangle(p1, p2);

            //Draw floor holo tiles
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


            //Draw tile entities
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

            //Draw entities
            foreach (KeyValuePair<int, MovingEntity> e in Entities)
            {
                e.Value.Draw(spriteBatch, gameTime);
            }

            //Draw walls, holo walls, and holo wires
            StartTime = DateTime.Now;
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

                    ht = GetTile(new Point(x, y)).HoloPowerCable;
                    if(ht != null)
                    {
                        ht.Draw(spriteBatch);
                    }
                }
            }
            spriteBatch.End();
            TimeWalls.Add((DateTime.Now - StartTime).TotalMilliseconds);

            //Draw cables from buffer
            spriteBatch.Begin(transformMatrix: Cam.Translate, samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            spriteBatch.Draw(CableBuffer, bufferpos * TileManager.TileSize, Color.White);
            spriteBatch.Draw(CableBuffer, bufferpos * TileManager.TileSize + new Vector2(CableBuffer.Width, 0), Color.White);
            spriteBatch.Draw(CableBuffer, bufferpos * TileManager.TileSize + new Vector2(0, CableBuffer.Height), Color.White);
            spriteBatch.Draw(CableBuffer, bufferpos * TileManager.TileSize + new Vector2(CableBuffer.Width, CableBuffer.Height), Color.White);
            spriteBatch.End();


            //Draw selection rectangles
            spriteBatch.Begin(transformMatrix: Cam.Translate, samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            Selection.DrawSelectionRectangles(spriteBatch, graphicsDevice, gameTime);
            spriteBatch.End();


            //Draw particles
            spriteBatch.Begin(transformMatrix: Cam.Translate, samplerState: SamplerState.PointClamp, blendState: BlendState.NonPremultiplied);
            for (int i = ParticleSources.Count - 1; i >= 0; i--)
            {
                if (ParticleSources[i].Draw(spriteBatch, gameTime))
                    ParticleSources.RemoveAt(i);
            }
            
            //Draw projectiles
            foreach(BaseProjectile p in Projectiles)
            {
                p.Draw(spriteBatch, gameTime);
            }
            
            spriteBatch.End();


            //Set target to backbuffer, clear
            graphicsDevice.SetRenderTarget(null);
            graphicsDevice.Clear(Color.Black);

            //Draw main buffer to backbuffer with scale and offset
            spriteBatch.Begin(transformMatrix: Cam.ScaleOffset, samplerState: SamplerState.PointClamp);
            spriteBatch.Draw(RenderTarget, Vector2.Zero, Color.White);
            spriteBatch.End();

            //Draw fog to buffer
            spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            spriteBatch.Draw(FogDrawn, Vector2.Zero, Color.White);
            spriteBatch.End();

            while (TimeWalls.Count > 100)
                TimeWalls.RemoveAt(0);
            while (TimeFloor.Count > 100)
                TimeFloor.RemoveAt(0);
            while (TimeFog.Count > 100)
                TimeFog.RemoveAt(0);

            AvgTimeWalls = 0;
            for(int i = 0; i < TimeWalls.Count; i++)
            {
                AvgTimeWalls += TimeWalls[i];
            }
            AvgTimeWalls /= TimeWalls.Count;

            AvgTimeFloor = 0;
            for (int i = 0; i < TimeFloor.Count; i++)
            {
                AvgTimeFloor += TimeFloor[i];
            }
            AvgTimeFloor /= TimeFloor.Count;

            AvgTimeFog = 0;
            for (int i = 0; i < TimeFog.Count; i++)
            {
                AvgTimeFog += TimeFog[i];
            }
            AvgTimeFog /= TimeFog.Count;

        }
    }
}
