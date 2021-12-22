using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.Utility
{
    public class Helper
    {
        public static Random Rand = new System.Random();
        public static Texture2D pixel;


        public static void Init(GraphicsDevice graphicsDevice)
        {
            pixel = new Texture2D(graphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
        }

        /// <summary>
        /// Draws a line
        /// <para>Note: Does not call spriteBatch.Begin() or spriteBatch.End()</para>
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to be used on draw call</param>
        /// <param name="t"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="color"></param>
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

        /// <summary>
        /// Averages pixels of a given Texture2D
        /// </summary>
        /// <param name="texture">Texture to be averaged</param>
        /// <returns>Average color of given texture</returns>
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


        /// <summary>
        /// Random double generator
        /// </summary>
        /// <returns>Double from 0.0 to 1.0</returns>
        public static float Random()
        {
            return (float)Rand.NextDouble();
        }
    }
}
