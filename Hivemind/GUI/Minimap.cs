using Hivemind.World;
using Hivemind.World.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.GUI
{
    public class Minimap
    {
        public Texture2D RenderedMap;
        public Point Size;
        public Point Position;

        public Minimap(Point size, GraphicsDevice graphicsDevice)
        {
            Size = size;
            RenderedMap = new Texture2D(graphicsDevice, Size.X, Size.Y);
        }

        public void SetPosition(Point pos)
        {
            Position = pos;
        }

        public void Redraw()
        {
            Position = TileMap.GetTileCoords(WorldManager.GetActiveTileMap().Cam.Pos);

            Color[] data = new Color[RenderedMap.Width * RenderedMap.Height];

            for(int x = 0; x < RenderedMap.Width; x++)
            {
                for(int y = 0; y < RenderedMap.Height; y++)
                {
                    Tile t = WorldManager.GetActiveTileMap().GetTile(new Point(Position.X - Size.X / 2 + x, Position.Y - Size.Y / 2 + y));
                    if (t.Wall != null)
                        data[x + y * RenderedMap.Width] = t.Wall.AverageColor;
                    else if(t.Floor != null)
                        data[x + y * RenderedMap.Width] = t.Floor.AverageColor;
                    else
                        data[x + y * RenderedMap.Width] = Color.Black;
                }
            }

            RenderedMap.SetData<Color>(data);
        }
    }
}
