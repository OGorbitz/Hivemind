using Hivemind.World.Entity;
using Hivemind.World.Tile;
using Hivemind.World.Tile.Floor;
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
        public Dictionary<Vector2, BaseTile>[] Tiles;
        public Dictionary<Vector2, BaseTile> Floors => Tiles[(int)Layer.FLOOR];
        public Dictionary<Vector2, bool> DirtyFloor;

        public bool Rendered = false;

        public RenderTarget2D FloorBuffer, RenderBuffer, RenderTarget;
        public Vector2 BufferPosition = Vector2.Zero, BufferOffset = Vector2.Zero, BufferSize = Vector2.Zero;

        public static readonly Color RColorRed = new Color(1f, 0f, 0f, 0.75f);
        public static readonly Color RColorGreen = new Color(0.25f, 0.5f, 0.125f, 0.75f);
        public Color RenderColor = RColorGreen;

        public Vector2 GetTileCoords(Vector2 input) => TileMap.GetTileCoords(input);

        public EditorTileMap()
        {
            Tiles = new Dictionary<Vector2, BaseTile>[(int)Layer.LENGTH];
            for (int i = 0; i < (int)Layer.LENGTH; i++)
            {
                Tiles[i] = new Dictionary<Vector2, BaseTile>();
            }
            DirtyFloor = new Dictionary<Vector2, bool>();
        }

        public float GetLayerDepth(int y) => Owner.GetLayerDepth(y);

        public bool InBounds(Vector2 position) => Owner.InBounds(position);
        public BaseTile GetTile(Vector2 position, Layer layer)
        {
            if (InBounds(position))
            {
                if (Tiles[(int)layer].ContainsKey(position))
                {
                    return Tiles[(int)layer][position];
                }
            }
            return null;
        }

        public BaseFloor GetFloor(Vector2 position)
        {
            if (InBounds(position))
            {
                if (Floors.ContainsKey(position))
                {
                    return (BaseFloor)Floors[position];
                }
            }
            return null;
        }

        public void RenderFloor(Vector2 position)
        {
            if (!DirtyFloor.ContainsKey(position))
            {
                DirtyFloor.Add(position, true);
            }
        }

        public void SetTile(BaseTile tile)
        {
            if (InBounds(tile.Pos))
            {
                if (Tiles[(int)tile.Layer].TryAdd(tile.Pos, tile))
                    tile.Parent = this;
            }
        }

        public void ClearTiles()
        {
            for (int i = 0; i < Tiles.Length; i++)
            {
                Tiles[i].Clear();
            }
        }

        public void RemoveTile(Vector2 position, Layer layer)
        {
            if (Tiles[(int)layer].ContainsKey(position))
                Tiles[(int)layer].Remove(position);
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

            BufferSize = new Vector2(width, height);

            width *= TileManager.TileSize;
            height *= TileManager.TileSize;

            if (FloorBuffer == null)
                FloorBuffer = new RenderTarget2D(graphicsDevice, width, height, false, SurfaceFormat.Vector4, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents); ;
            if (RenderBuffer == null)
                RenderBuffer = new RenderTarget2D(graphicsDevice, width, height);
            
            graphicsDevice.SetRenderTarget(RenderTarget);
            graphicsDevice.Clear(Color.TransparentBlack);

            spriteBatch.Begin(transformMatrix: Cam.Translate, samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            foreach (KeyValuePair<Vector2, BaseTile> pair in Tiles[(int)Layer.FLOOR])
            {
                var f = (BaseFloor) pair.Value;
                Texture2D t = FloorMask.Textures[f.FloorLayer];

                Rectangle sourceRectangle = new Rectangle((int)(f.Pos.X * TileManager.TileSize % t.Width), (int)(f.Pos.Y * TileManager.TileSize % t.Height), TileManager.TileSize, TileManager.TileSize);
                spriteBatch.Draw(t, position: f.Pos * TileManager.TileSize, sourceRectangle: sourceRectangle, Color.White);
            }
            spriteBatch.End();

            spriteBatch.Begin(transformMatrix: Cam.Translate, samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            foreach (KeyValuePair<Vector2, BaseTile> t in Tiles[(int)Layer.WALL])
            {
                t.Value.Draw(spriteBatch);
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
