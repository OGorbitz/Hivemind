using Hivemind.World;
using Hivemind.World.Entity;
using Hivemind.World.Entity.Tile;
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
                    if (t.Visibility == Visibility.HIDDEN)
                        data[x + y * RenderedMap.Width] = Color.Black;
                    else if (t.Wall != null)
                        data[x + y * RenderedMap.Width] = t.Wall.AverageColor;
                    else if (t.TileEntity != null && t.TileEntity.Type == SpaseShip.UType)
                        data[x + y * RenderedMap.Width] = Color.Green;
                    else if (t.Floor != null)
                        data[x + y * RenderedMap.Width] = t.Floor.AverageColor;
                    else
                        data[x + y * RenderedMap.Width] = Color.Black;
                }
            }

            foreach(KeyValuePair<int, MovingEntity> e in WorldManager.GetActiveTileMap().Entities)
            {
                Point wpos = (e.Value.Pos / new Vector2(TileManager.TileSize)).ToPoint();
                Point p = wpos - Position + Size / new Point(2);
                if(p.X > 0 && p.X < Size.X && p.Y > 0 && p.Y < Size.Y)
                    if(WorldManager.GetActiveTileMap().GetTile(wpos).Visibility == Visibility.KNOWN)
                        data[p.X + p.Y * RenderedMap.Width] = Color.Green;
            }

            RenderedMap.SetData<Color>(data);
        }
    }
}
