using Hivemind.World.Generator;
using Hivemind.World.Tile;
using Hivemind.World.Tile.Floor;
using Hivemind.World.Tile.Wall;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Hivemind.World
{
    [Serializable]
    public class TileMap
    {
        //Owned objects
        public Camera Cam;
        private RenderTarget2D RenderTarget;
        public WorldGenerator WorldGenerator;

        //Holds pre rendered floor
        public RenderTarget2D FloorBuffer;
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



        //Specific data
        public int Size;
        private BaseTile[,,] Tiles;

        //Creation, Destruction
        public TileMap(int s)
        {
            Size = s;
            Tiles = new BaseTile[s, s, (int) Layer.LENGTH];
            Cam = new Camera();
            WorldGenerator = new WorldGenerator(69l, this);

            FloorBuffer = null;

            Generate();
        }

        public void Generate()
        {
            for (var x = 0; x < Size; x++)
            {
                for (var y = 0; y < Size; y++)
                {
                    var pos = new Vector2(x, y);
                    SetTile(new Floor_Concrete(pos));
                    var r = new Rectangle(4, 4, 7, 5);
                    var rr = new Rectangle(5, 5, 5, 3);

                    var t = WorldGenerator.GetTemperature(pos);

                    if (t > 0.35)
                        SetTile(new Wall_Cinderblock(pos));

                    if (t > 0.25)
                        SetTile(new Floor_Concrete(pos));
                    else if (t > -0.25)
                        SetTile(new Floor_Dirt(pos));
                    else
                        SetTile(new Floor_Grass(pos));
                }
            }
        }

        public TileMap(SerializationInfo info, StreamingContext context)
        {
            Tiles = (BaseTile[,,])info.GetValue("Tiles", typeof(BaseTile[,,]));
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

        internal float GetLayerDepth(int y)
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
        /// <param name="coords">World position <see cref="Vector2"/></param>
        /// <returns>A <see cref="Vector2"/> containing tile coordinates</returns>
        public static Vector2 GetTileCoords(Vector2 coords)
        {
            var wx = coords.X / TileManager.TileSize;
            var wy = coords.Y / TileManager.TileSize;

            if (wx < 0)
                wx = (float)Math.Floor(wx);
            if (wy < 0)
                wy = (float)Math.Floor(wy);
            return new Vector2((int)wx, (int)wy);
        }

        /// <summary>
        /// Checks if a vector is within bounds of the Tilemap
        /// </summary>
        /// <param name="position">A position vector</param>
        /// <returns>True if within bounds</returns>
        public bool InBounds(Vector2 position)
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
        public BaseTile GetTile(Vector2 position, Layer layer)
        {
            if (InBounds(position))
            {
                return Tiles[(int) position.X, (int) position.Y, (int) layer];
            }
            return null;
        }

        public BaseFloor GetFloor(Vector2 position)
        {
            return (BaseFloor) GetTile(position, Layer.FLOOR);
        }

        /// <summary>
        /// Sets array value to the tile, at it's given position. Does nothing if out of bounds.
        /// </summary>
        /// <param name="tile">Tile to be set</param>
        public void SetTile(BaseTile tile)
        {
            if (InBounds(tile.Pos))
            {
                Tiles[(int)tile.Pos.X, (int)tile.Pos.Y, (int)tile.Layer] = tile;
                tile.Parent = this;
                DirtySurroundingTiles(tile.Pos, tile.Layer);
            }
        }

        /// <summary>
        /// CALLED BY <see cref="BaseTile.Destroy()"/>! <br/>
        /// Removes the tile at a given position from the array.
        /// </summary>
        public void RemoveTile(Vector2 position, Layer layer)
        {
            if (InBounds(position))
            {
                Tiles[(int)position.X, (int)position.Y, (int) layer] = null;
            }
        }

        /// <summary>
        /// Sets surrounding tiles as dirty, to have their render indices updated
        /// </summary>
        /// <param name="position">The position to set adjacent tiles to dirty</param>
        public void DirtySurroundingTiles(Vector2 position, Layer layer)
        {
            int[,] surroundingtiles =
            {
                {0, 0},
                {-1, -1},
                {-0, -1},
                {1, -1},
                {1, 0},
                {1, 1},
                {0, 1},
                {-1, 1},
                {-1, 0}
            };
            for (var i = 0; i < surroundingtiles.GetLength(0); i++)
            {
                Vector2 v = new Vector2(position.X + surroundingtiles[i, 0], position.Y + surroundingtiles[i, 1]);
                BaseTile t = GetTile(v, layer);
                if (t != null)
                    t.Dirty = true;
            }
        }

        /// <summary>
        /// Updates the render index of the tile at the given position
        /// </summary>
        public void UpdateRenderIndex(Vector2 position, Layer layer)
        {
            BaseTile t = GetTile(position, layer);
            if(t != null)
            {
                // TODO: Update tile render index
            }
        }

        public void DrawFloor(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            for (int l = 0; l < (int)FloorPriority.COUNT; l++)
            {
                graphicsDevice.SetRenderTarget(RenderBuffer);
                graphicsDevice.Clear(Color.Transparent);

                spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
                for (int x = 0; x < Size; x++)
                {
                    for (int y = 0; y < Size; y++)
                    {
                        var tile = GetFloor(new Vector2(x, y));
                        if (tile == null)
                            continue;
                        if (!tile.Dirty)
                            continue;

                        if (tile.RenderPriority == l)
                        {
                            spriteBatch.Draw(FloorMask.Solid, new Vector2(x * TileManager.TileSize, y * TileManager.TileSize), Color.White);
                        }
                        else if (tile.RenderPriority < l)
                        {
                            int index = 0;
                            for (int n = 0; n < 8; n++)
                            {
                                var ctile = GetFloor(new Vector2(x + FloorMask.indices[n, 0], y + FloorMask.indices[n, 1]));
                                if (ctile == null)
                                    continue;
                                if (ctile.RenderPriority >= l) //Tile is "solid" for layer
                                {
                                    index += 1 << n;
                                }
                            }

                            spriteBatch.Draw(FloorMask.MaskAtlas, new Vector2(x * TileManager.TileSize, y * TileManager.TileSize),
                                sourceRectangle: new Rectangle(index * 64, 0, 64, 64), Color.White);
                        }
                    }
                }

                spriteBatch.End();

                spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: StencilBlendState);

                Texture2D t = FloorMask.Textures[l];
                for (int x = 0; x < Size * TileManager.TileSize; x += t.Width) //Replace with reference to current layer
                {
                    for (int y = 0; y < Size * TileManager.TileSize; y += t.Height)
                    {
                        spriteBatch.Draw(t, new Vector2(x, y), Color.White);
                    }
                }

                spriteBatch.End();

                graphicsDevice.SetRenderTarget(FloorBuffer);

                spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
                spriteBatch.Draw(RenderBuffer, Vector2.Zero, Color.White);
                spriteBatch.End();
            }

            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {
                    var tile = GetFloor(new Vector2(x, y));
                    if (tile == null)
                        continue;
                    tile.Dirty = false;
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

            //TODO: Render floors
            int width = 1 + (int) Math.Ceiling((float)RenderTarget.Width / (float)TileManager.TileSize);
            int height = 1 + (int)Math.Ceiling((float)RenderTarget.Height / (float)TileManager.TileSize);

            width *= TileManager.TileSize;
            height *= TileManager.TileSize;

            if (FloorBuffer == null)
                FloorBuffer = new RenderTarget2D(graphicsDevice, width, height, false, SurfaceFormat.Vector4, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents); ;
            if (RenderBuffer == null)
                RenderBuffer = new RenderTarget2D(graphicsDevice, width, height);

            DrawFloor(spriteBatch, graphicsDevice, gameTime);

            graphicsDevice.SetRenderTarget(RenderTarget);

            spriteBatch.Begin(transformMatrix: Cam.Translate, samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            spriteBatch.Draw(FloorBuffer, Vector2.Zero, Color.White);
            spriteBatch.End();

            //TODO: Render entities

            //TODO: Render walls

            spriteBatch.Begin(transformMatrix: Cam.Translate, samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            for(int x = 0; x < Size; x++)
            {
                for(int y = 0; y < Size; y++)
                {
                    BaseTile t = GetTile(new Vector2(x, y), Layer.WALL);
                    if(t != null)
                    {
                        t.Draw(spriteBatch);
                    }
                }
            }
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
