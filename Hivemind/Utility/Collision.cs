using Hivemind.World;
using Hivemind.World.Tile;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.Utility
{
    public class Collision
    {

        public static Vector2 CheckWorld(Vector2 move, Rectangle moving)
        {
            Vector2 WorldPos = TileMap.GetTileCoords(new Vector2(moving.X, moving.Y));
            Vector2 TileSize = TileMap.GetTileCoords(new Vector2(moving.Width, moving.Height));

            Point Start = (WorldPos - TileSize - new Vector2(1)).ToPoint();
            Point End = (WorldPos + TileSize + new Vector2(1)).ToPoint();

            Vector2 calculated_move = move;

            for (int x = (int)Start.X; x <= End.X; x++)
            {
                for (int y = (int)Start.Y; y <= End.Y; y++)
                {
                    BaseTile tile = WorldManager.GetActiveTileMap().GetTile(new Vector2(x, y), Layer.WALL);
                    if(tile != null)
                        calculated_move = Check(calculated_move, moving, tile.Collider());
                }
            }

            return calculated_move;
        }

        /// <summary>
        /// Checks the given move vector and returns a resolved movement vector based on the bounds rectangle given
        /// </summary>
        /// <param name="move">The movement vector checked and resolved</param>
        /// <param name="moving">The object moving</param>
        /// <param name="bounds">The object checked against</param>
        /// <returns></returns>
        public static Vector2 Check(Vector2 move, Rectangle moving, Rectangle bounds)
        {
            Vector2 DCache = new Vector2(1) / move;

            float TNearX = (bounds.Left - moving.Width - moving.Left) * DCache.X;
            float TNearY = (bounds.Top - moving.Height - moving.Top) * DCache.Y;
            float TFarX = (bounds.Right - moving.X) * DCache.X;
            float TFarY = (bounds.Bottom - moving.Y) * DCache.Y;

            if (Single.IsNaN(TNearX) || Single.IsNaN(TNearY))
                return move;
            if (Single.IsNaN(TFarX) || Single.IsNaN(TFarY))
                return move;

            Vector2 TNear = Vector2.Zero;
            Vector2 TFar = Vector2.Zero;

            if(TFarX > TNearX)
            {
                TNear.X = TNearX;
                TFar.X = TFarX;
            }
            else
            {
                TNear.X = TFarX;
                TFar.X = TNearX;
            }

            if (TFarY > TNearY)
            {
                TNear.Y = TNearY;
                TFar.Y = TFarY;
            }
            else
            {
                TNear.Y = TFarY;
                TFar.Y = TNearY;
            }


            if (TFar.X < TNear.Y || TFar.Y < TNear.X)
                return move;

            float THitNear = Math.Max(TNear.X, TNear.Y);
            float THitFar = Math.Min(TFar.X, TFar.Y);

            if (THitFar <= 0)
                return move;

            if (THitNear >= 1.0f)
                return move;


            Vector2 normal = Vector2.Zero;

            if(TNear.X > TNear.Y)
                normal = new Vector2(1, 0);
            else if(TNear.X < TNear.Y)
                normal = new Vector2(0, 1);

            Vector2 diff = -move * normal * (1 - THitNear);

            return move + diff;
        }

    }
}
