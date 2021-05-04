using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.World
{
    public enum FloorPriority
    {
        Floor_Dirt,
        Floor_Grass,
        Floor_Concrete,
        COUNT
    }

    class FloorMask
    {
        public static Texture2D DirtMask;
        public static Texture2D DirtOverlay;

        public static Texture2D[] Textures = new Texture2D[(int)FloorPriority.COUNT];

        public static readonly int[,] indices =
        {
            {1, 1},
            {0, 1},
            {-1, 1},
            {1, 0},
            {-1, 0},
            {1, -1},
            {0, -1},
            {-1, -1}
        };

        internal static void LoadContent(ContentManager content, GraphicsDevice gdevice)
        {
            DirtMask = content.Load<Texture2D>("Tiles/Masks/DirtMask");
            DirtOverlay = content.Load<Texture2D>("Tiles/Masks/DirtOverlay");
        }
    }
}
