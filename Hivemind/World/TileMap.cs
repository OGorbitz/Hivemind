using Hivemind.World.Tile;
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

        //Holds pre rendered floor
        public RenderTarget2D FloorBuffer;
        //Used to render floor
        public static RenderTarget2D RenderBuffer;


        //Specific data
        public int Size;
        private BaseTile[,,] Tiles;

        //Creation, Destruction
        public TileMap(int s)
        {
            Size = s;
            Tiles = new BaseTile[s, s, (int) Layer.LENGTH];

            FloorBuffer = null;
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

        /// <summary>
        /// Sets array value to the tile, at it's given position. Does nothing if out of bounds.
        /// </summary>
        /// <param name="tile">Tile to be set</param>
        public void SetTile(BaseTile tile)
        {
            if (InBounds(tile.Pos))
            {
                Tiles[(int)tile.Pos.X, (int)tile.Pos.Y, (int)tile.Layer] = tile;
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

            //TODO: Render entities

            //TODO: Render walls

            //TODO: Render particles

            graphicsDevice.SetRenderTarget(null);
            graphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(transformMatrix: Cam.ScaleOffset, samplerState: SamplerState.PointClamp);
            spriteBatch.Draw(RenderTarget, Vector2.Zero, Color.White);
            spriteBatch.End();

        }
    }
}
