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
        public static RenderTarget2D MaskAtlas;
        public static Texture2D Solid;

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
            Texture2D[] Masks = new Texture2D[9];
            for (int n = 0; n < 9; n++)
            {
                Masks[n] = content.Load<Texture2D>("Tiles/Masks/FloorMask1_" + (n + 1));
            }

            Solid = Masks[8];

            MaskAtlas = new RenderTarget2D(gdevice, 256 * 64, 64);
            gdevice.SetRenderTarget(MaskAtlas);
            gdevice.Clear(Color.Transparent);

            SpriteBatch spriteBatch = new SpriteBatch(gdevice);

            spriteBatch.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp, blendState: BlendState.NonPremultiplied);

            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if ((i & (1 << j)) != 0)
                    {
                        spriteBatch.Draw(Masks[j], new Vector2(64 * i, 0), Color.White);
                    }
                }
            }

            spriteBatch.End();

            gdevice.SetRenderTarget(null);
        }
    }
}
