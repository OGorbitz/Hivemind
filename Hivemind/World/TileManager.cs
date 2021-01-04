﻿using Hivemind.World.Tile.Floor;
using Hivemind.World.Tile.Wall;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.World
{
    public enum Layer
    {
        FLOOR,
        WALL,
        WIRE,
        LENGTH,
        NULL
    }

    class TileManager
    {
        public const int TileSize = 64;
        public const int WallHeight = 20;

        public static void LoadTiles(ContentManager content, GraphicsDevice graphicsDevice)
        {
            //Initialize tile objects here
            Floor_Concrete.LoadAssets(content);
            Floor_Dirt.LoadAssets(content);
            Floor_Grass.LoadAssets(content);

            Wall_Cinderblock.LoadAssets(content, graphicsDevice);
            Wall_Dirt.LoadAssets(content, graphicsDevice);
            //Wall_Door.LoadAssets();

            //Wire.LoadAssets();
        }

    }
}
