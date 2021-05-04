using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.Utility
{
    public class Helper
    {
        public static Random Rand;
        public static Texture2D pixel;


        public static void Init(GraphicsDevice graphicsDevice)
        {
            pixel = new Texture2D(graphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
        }

        public static void DrawLine(SpriteBatch spriteBatch, Texture2D t, Vector2 start, Vector2 end, Color color)
        {
            var edge = end - start;
            // calculate angle to rotate line
            var angle = (float)Math.Atan2(edge.Y, edge.X);


            spriteBatch.Draw(t,
                new Rectangle( // rectangle defines shape of line and position of start of line
                    (int)start.X,
                    (int)start.Y,
                    (int)edge.Length(), //sb will strech the texture to fill this rectangle
                    1), //width of line, change this to make thicker line
                null,
                color, //colour of line
                angle, //angle of line (calulated above)
                new Vector2(0, 0), // point in line about which to rotate
                SpriteEffects.None,
                0);
        }

        public static Color AverageColor(Texture2D texture)
        {
            float r = 0, g = 0, b = 0;
            Color[] textureData = new Color[texture.Width * texture.Height];
            texture.GetData<Color>(textureData);

            for(int x = 0; x < textureData.Length; x++)
            {
                Vector4 c = textureData[x].ToVector4();
                if (c.W < 1.0f)
                    continue;

                r += c.X;
                g += c.Y;
                b += c.Z;
            }
            return new Color(r / textureData.Length, g / textureData.Length, b / textureData.Length);
        }

        public static float Random()
        {
            if (Rand == null)
                Rand = new Random();
            return (float)Rand.NextDouble();
        }
    }
}
