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
        public static Layer ULayer = Layer.POWER;
        public override Layer Layer => ULayer;
        public static string[] UName = { "PowerCableT1", "PowerCableT2" };
        public override string Name => UName[Tier];

        public static Texture2D[] UIcon;
        public static int[,] Tex;

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

        public override void Draw(SpriteBatch spriteBatch, Color color, Point dest)
        {
            bool needsjunction = false;

            for (int t = 0; t < Tier; t++)
            {
                int ind = 0;

                for (int i = 0; i < 4; i++)
                {
                    Point p = Pos + new Point(neighbors[i, 0], neighbors[i, 1]);
                    Tile n = Parent.GetTile(p);
                    if (n != null && n.PowerCable != null)
                    {
                        if (n.PowerCable.Tier == t)
                        {
                            ind += 1 << i;
                            needsjunction = true;
                        }
                    }
                }

                spriteBatch.Draw(TextureAtlas.Atlas, dest.ToVector2(), sourceRectangle: TextureAtlas.GetSourceRect(PowerCable.Tex[t, ind]), color);
            }

            int index = 0;

            for (int i = 0; i < 4; i++)
            {
                Point p = Pos + new Point(neighbors[i, 0], neighbors[i, 1]);
                Tile n = Parent.GetTile(p);
                if (n != null){
                    if (n.PowerCable != null)
                    {
                        if (n.PowerCable.Tier >= Tier)
                        {
                            index += 1 << i;
                        }
                    }
                    else if (n.HoloPowerCable != null)
                    {
                        if (((PowerCable)n.HoloPowerCable.Child).Tier >= Tier)
                        {
                            index += 1 << i;
                        }
                    }
                }
            }
            if (index == 15 || index == 14 || index == 13 || index == 11 || index == 7 || index == 0)
                needsjunction = true;

            spriteBatch.Draw(TextureAtlas.Atlas, dest.ToVector2(), sourceRectangle: TextureAtlas.GetSourceRect(PowerCable.Tex[Tier, index]), color);
            if (needsjunction)
                spriteBatch.Draw(TextureAtlas.Atlas, dest.ToVector2(), sourceRectangle: TextureAtlas.GetSourceRect(PowerCable.Tex[Tier, 16]), color);

        }

        public static void LoadAssets(ContentManager content, GraphicsDevice graphicsDevice)
        {
            var buffer = new RenderTarget2D(graphicsDevice, 64, 64);

            Tex = new int[2,17];
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
                    if ((i & 1 << (j - 1)) != 0)
                    {
                        spriteBatch.Draw(texture, new Rectangle(0, 0, 64, 64), sourceRectangle: new Rectangle(j * 64, 0, 64, 64), Color.White);
                    }
                }
                
                spriteBatch.End();

                if (i == 15)
                {
                    UIcon[0] = new RenderTarget2D(graphicsDevice, 64, 64);
                    graphicsDevice.SetRenderTarget((RenderTarget2D)UIcon[0]);
                    graphicsDevice.Clear(Color.Transparent);
                    spriteBatch.Begin();
                    spriteBatch.Draw(buffer, new Rectangle(0, 0, 64, 64), Color.White);
                    spriteBatch.Draw(texture, new Rectangle(0, 0, 64, 64), sourceRectangle: new Rectangle(0, 0, 64, 64), Color.White);
                    spriteBatch.End();
                }

                Tex[0,i] = TextureAtlas.AddTexture((Texture2D)buffer, graphicsDevice);
            }

            graphicsDevice.SetRenderTarget(buffer);
            graphicsDevice.Clear(Color.Transparent);

            spriteBatch.Begin();
            spriteBatch.Draw(texture, new Rectangle(0, 0, 64, 64), sourceRectangle: new Rectangle(0, 0, 64, 64), Color.White);
            spriteBatch.End();

            Tex[0, 16] = TextureAtlas.AddTexture((Texture2D)buffer, graphicsDevice);

            texture = content.Load<Texture2D>("Tiles/Utilities/PowerT2");

            for (int i = 0; i < 16; i++)
            {
                graphicsDevice.SetRenderTarget(buffer);
                graphicsDevice.Clear(Color.Transparent);
                spriteBatch.Begin();
                for (int j = 1; j < 5; j++)
                {
                    if ((i & 1 << (j - 1)) != 0)
                    {
                        spriteBatch.Draw(texture, new Rectangle(0, 0, 64, 64), sourceRectangle: new Rectangle(j * 64, 0, 64, 64), Color.White);
                    }
                }
                spriteBatch.End();

                if (i == 15)
                {
                    UIcon[1] = new RenderTarget2D(graphicsDevice, 64, 64);
                    graphicsDevice.SetRenderTarget((RenderTarget2D)UIcon[1]);
                    graphicsDevice.Clear(Color.Transparent);
                    spriteBatch.Begin();
                    spriteBatch.Draw(buffer, new Rectangle(0, 0, 64, 64), Color.White);
                    spriteBatch.Draw(texture, new Rectangle(0, 0, 64, 64), sourceRectangle: new Rectangle(0, 0, 64, 64), Color.White);
                    spriteBatch.End();
                }

                Tex[1, i] = TextureAtlas.AddTexture((Texture2D)buffer, graphicsDevice);
            }

            graphicsDevice.SetRenderTarget(buffer);
            graphicsDevice.Clear(Color.Transparent);

            spriteBatch.Begin();
            spriteBatch.Draw(texture, new Rectangle(0, 0, 64, 64), sourceRectangle: new Rectangle(0, 0, 64, 64), Color.White);
            spriteBatch.End();

            Tex[1, 16] = TextureAtlas.AddTexture((Texture2D)buffer, graphicsDevice);

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