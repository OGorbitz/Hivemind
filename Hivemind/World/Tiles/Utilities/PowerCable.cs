using Hivemind.Utility;
using Hivemind.World.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.World.Tiles.Utilities
{
    public class PowerCable : BaseTile
    {
        public static Texture2D[] UIcon;
        private static int[,] Tex;

        public int Tier = 0;

        public readonly int[,] neighbors =
        {
            { -1, 0 },
            { 0, -1 },
            { 1, 0 },
            { 0, 1 }
        };

        public override void Destroy()
        {
            throw new NotImplementedException();
        }

        public override void Draw(SpriteBatch spriteBatch, Color color)
        {
            for (int i = 0; i < 4; i++)
            {
                Point p = new Point(Pos.X + neighbors[i, 0], Pos.Y + neighbors[i, 1]);
                Tile n = Parent.GetTile(p);
                if (n.PowerCable != null)
                {
                    if (n.PowerCable.Tier < Tier)
                    {
                        
                    }
                }
            }
        }

        public static void LoadAssets(ContentManager content, GraphicsDevice graphicsDevice)
        {
            var buffer = new RenderTarget2D(graphicsDevice, 64, 64);

            Tex = new int[2,16];
            UIcon = new Texture2D[2];

            var texture = content.Load<Texture2D>("Tiles/Utilities/PowerT1");

            SpriteBatch spriteBatch = new SpriteBatch(graphicsDevice);

            for (int i = 0; i < 16; i++)
            {
                graphicsDevice.SetRenderTarget(buffer);
                graphicsDevice.Clear(Color.Transparent);
                spriteBatch.Begin();
                for (int j = 1; j < 5; j++)
                {
                    if ((i & 1 << j) != 0)
                    {
                        spriteBatch.Draw(texture, new Rectangle(0, 0, 64, 64), sourceRectangle: new Rectangle(i * 64, 0, 64, 64), Color.White);
                    }
                }
                if (i == 10 || i == 5)
                {
                    spriteBatch.Draw(texture, new Rectangle(0, 0, 64, 64), sourceRectangle: new Rectangle(0, 0, 64, 64), Color.White);
                }
                spriteBatch.End();

                if (i == 15)
                {
                    UIcon[0] = new RenderTarget2D(graphicsDevice, 64, 64);
                    graphicsDevice.SetRenderTarget((RenderTarget2D)UIcon[0]);
                    spriteBatch.Begin();
                    spriteBatch.Draw(buffer, new Rectangle(0, 0, 64, 64), Color.White);
                    spriteBatch.End();
                }

                Tex[1,i] = TextureAtlas.AddTexture((Texture2D)buffer, graphicsDevice);
            }

            for (int i = 0; i < 16; i++)
            {
                graphicsDevice.SetRenderTarget(buffer);
                graphicsDevice.Clear(Color.Transparent);
                spriteBatch.Begin();
                for (int j = 1; j < 5; j++)
                {
                    if ((i & 1 << j) != 0)
                    {
                        spriteBatch.Draw(texture, new Rectangle(0, 0, 64, 64), sourceRectangle: new Rectangle(i * 64, 0, 64, 64), Color.Red);
                    }
                }
                if (i == 10 || i == 5)
                {
                    spriteBatch.Draw(texture, new Rectangle(0, 0, 64, 64), sourceRectangle: new Rectangle(0, 0, 64, 64), Color.Red);
                }
                spriteBatch.End();

                if (i == 15)
                {
                    UIcon[1] = new RenderTarget2D(graphicsDevice, 64, 64);
                    graphicsDevice.SetRenderTarget((RenderTarget2D)UIcon[1]);
                    spriteBatch.Begin();
                    spriteBatch.Draw(buffer, new Rectangle(0, 0, 64, 64), Color.White);
                    spriteBatch.End();
                }

                Tex[2, i] = TextureAtlas.AddTexture((Texture2D)buffer, graphicsDevice);
            }

            TileConstructor.RegisterConstructor("PowerCableT1", T1Constructor);
            TileConstructor.RegisterConstructor("PowerCableT2", T2Constructor);
        }

        public static PowerCable T1Constructor()
        {
            PowerCable cable = new PowerCable();
            cable.Tier = 0;
            return cable;
        }

        public static PowerCable T2Constructor()
        {
            PowerCable cable = new PowerCable();
            cable.Tier = 1;
            return cable;
        }
    }
}
