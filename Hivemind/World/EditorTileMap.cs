using Hivemind.World.Entity;
using Hivemind.World.Tiles;
using Hivemind.World.Tiles.Floor;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.World
{
    class EditorTileMap : ITileMap
    {
        public Camera Cam => Owner.Cam;
        public TileMap Owner => WorldManager.GetActiveTileMap();

        public TileEntity TEntity;
        public MovingEntity MEntity;
        public Dictionary<Point, Tile> Tiles;
        public Dictionary<Point, bool> DirtyFloor;

        public bool Rendered = false;

        public RenderTarget2D FloorBuffer, RenderBuffer, RenderTarget;
        public Point BufferPosition = Point.Zero, BufferOffset = Point.Zero, BufferSize = Point.Zero;

        public static readonly Color RColorRed = new Color(1f, 0f, 0f, 0.75f);
        public static readonly Color RColorGreen = new Color(0.25f, 0.5f, 0.125f, 0.75f);
        public Color RenderColor = RColorGreen;

        public Point GetTileCoords(Point input) => TileMap.GetTileCoords(input);
        public Point GetTileCoords(Vector2 input) => TileMap.GetTileCoords(input);

        public EditorTileMap()
        {
            Tiles = new Dictionary<Point, Tile>();
            DirtyFloor = new Dictionary<Point, bool>();
        }

        public float GetLayerDepth(int y) => Owner.GetLayerDepth(y);

        public bool InBounds(Point position) => Owner.InBounds(position);
        public Tile GetTile(Point position)
        {
            if (InBounds(position))
            {
                if (Tiles.ContainsKey(position))
                {
                    return Tiles[position];
                }
            }
            return null;
        }

        public void RenderFloor(Point position)
        {
            if (!DirtyFloor.ContainsKey(position))
            {
                DirtyFloor.Add(position, true);
            }
        }

        public void SetTile(Point pos, BaseTile tile)
        {
            if (InBounds(pos))
            {

                if (Tiles.ContainsKey(pos))
                {
                    switch (tile.Layer)
                    {
                        case Layer.WALL:
                            Tiles[pos].Wall = (BaseWall)tile;
                            break;
                        case Layer.FLOOR:
                            Tiles[pos].Floor = (BaseFloor)tile;
                            break;
                    }

                }
                else
                {
                    Tile t = new Tile(pos, this);
                    switch (tile.Layer)
                    {
                        case Layer.WALL:
                            t.Wall = (BaseWall)tile;
                            break;
                        case Layer.FLOOR:
                            t.Floor = (BaseFloor)tile;
                            break;
                    }
                    Tiles.Add(pos, t);
                }
            }
        }

        public void ClearTiles()
        {
            Tiles.Clear();
        }

        public void RemoveTile(Point position, Layer layer)
        {
            if (Tiles.ContainsKey(position))
                Tiles.Remove(position);
        }

        public void Render(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, GameTime gameTime)
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

            int width = 1 + (int)Math.Ceiling((float)RenderTarget.Width / (float)TileManager.TileSize);
            int height = 1 + (int)Math.Ceiling((float)RenderTarget.Height / (float)TileManager.TileSize);

            BufferSize = new Point(width, height);

            width *= TileManager.TileSize;
            height *= TileManager.TileSize;

            if (FloorBuffer == null)
                FloorBuffer = new RenderTarget2D(graphicsDevice, width, height, false, SurfaceFormat.Vector4, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents); ;
            if (RenderBuffer == null)
                RenderBuffer = new RenderTarget2D(graphicsDevice, width, height);
            
            graphicsDevice.SetRenderTarget(RenderTarget);
            graphicsDevice.Clear(Color.TransparentBlack);

            spriteBatch.Begin(transformMatrix: Cam.Translate, samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            foreach (KeyValuePair<Point, Tile> pair in Tiles)
            {
                var f = pair.Value.Floor;
                if (f == null)
                    continue;
                Texture2D t = FloorMask.Textures[f.FloorLayer];

                Rectangle sourceRectangle = new Rectangle((int)(f.Pos.X * TileManager.TileSize % t.Width), (int)(f.Pos.Y * TileManager.TileSize % t.Height), TileManager.TileSize, TileManager.TileSize);
                spriteBatch.Draw(t, position: f.Pos.ToVector2() * TileManager.TileSize, sourceRectangle: sourceRectangle, Color.White);
            }
            spriteBatch.End();

            spriteBatch.Begin(transformMatrix: Cam.Translate, samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            foreach (KeyValuePair<Point, Tile> t in Tiles)
            {
                if (t.Value.Wall == null)
                    continue;
                t.Value.Wall.Draw(spriteBatch);
            }
            spriteBatch.End();

            var bs = new BlendState
            {
                ColorSourceBlend = Blend.Zero,
                ColorBlendFunction = BlendFunction.Add,
                ColorDestinationBlend = Blend.One,
                AlphaSourceBlend = Blend.One,
                AlphaBlendFunction = BlendFunction.ReverseSubtract,
                AlphaDestinationBlend = Blend.One
            };

            spriteBatch.Begin(blendState: bs, samplerState: SamplerState.PointClamp);

            var ms = gameTime.TotalGameTime.Seconds * 1000 + gameTime.TotalGameTime.Milliseconds;
            var n = (int)(ms % 2000 / 2000f * 32f);
            for (var x = -n; x <= RenderTarget.Height; x += Hivemind.ComputerLines.Height)
                spriteBatch.Draw(Hivemind.ComputerLines,
                    new Rectangle(new Point(0, x), new Point(RenderTarget.Width, Hivemind.ComputerLines.Height)),
                    new Color(1f, 1f, 1f, 0.25f));
            n = (int)(ms % 3000 / 3000f * 32f);
            for (var x = n - 32; x <= RenderTarget.Height; x += Hivemind.ComputerLines.Height)
                spriteBatch.Draw(Hivemind.ComputerLines,
                    new Rectangle(new Point(0, x), new Point(RenderTarget.Width, Hivemind.ComputerLines.Height * 4)),
                    new Color(1f, 1f, 1f, 0.125f));
            spriteBatch.End();
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            spriteBatch.Begin(transformMatrix: Cam.ScaleOffset, samplerState: SamplerState.PointClamp);
            spriteBatch.Draw(RenderTarget, Vector2.Zero, RenderColor);
            spriteBatch.End();
        }
    }
}
