using System;
using System.Runtime.Serialization;
using Hivemind.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Hivemind.World.Tiles.Wall
{
    [Serializable]
    internal class Wall_Dirt : BaseWall
    {
        //Static variables
        new public const string UName = "Wall_Dirt";
        new public const Layer ULayer = Layer.WALL;
        new public const float UResistance = -1;
        new private static Color UAverageColor = Color.Magenta;

        public override string Name => UName;
        public override float Resistance => UResistance;
        public override Layer Layer => ULayer;
        public virtual Texture2D Icon => UIcon;
        public override Color AverageColor => UAverageColor;


        //Assets
        public static Texture2D UIcon;
        private static int[] Tex;

        //Custom variables
        private static int[,] tilecheck = new int[4, 3]
            {
                {1, 0, -1},
                {2, 1, 0},
                {4, 0, 1},
                {8, -1, 0}
            };

        private int renderindex;

        public Wall_Dirt()
        {
        }

        public Wall_Dirt(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public static void LoadAssets(ContentManager content, GraphicsDevice graphicsDevice)
        {
            var textures = new Texture2D[16];
            Tex = new int[textures.Length];
            for (var i = 0; i < 16; i++) textures[i] = content.Load<Texture2D>("Tiles/Wall/Dirt/" + (i + 1));

            UIcon = textures[0];

            Vector3 color = Vector3.Zero;
            for (int i = 0; i < textures.Length; i++)
            {
                Tex[i] = TextureAtlas.AddTexture(textures[i], graphicsDevice);
                color += Helper.AverageColor(textures[i]).ToVector3();
            }
            color /= textures.Length;
            UAverageColor = new Color(color);

            TileConstructor.RegisterConstructor(UName, Constructor);
        }

        public static BaseWall Constructor()
        {
            return new Wall_Dirt();
        }

        public static void Unlock()
        {
        }

        public void UpdateRenderIndex()
        {
            var ri = 0;

            for (var i = 0; i < 4; i++)
            {
                var t = Parent.GetTile(new Point((int)Pos.X + tilecheck[i, 1], (int)Pos.Y + tilecheck[i, 2]));
                if (t == null || t.Wall == null)
                    continue;
                if (t.Wall.Name == Name) ri += tilecheck[i, 0];
            }

            if (ri >= 16) ri = 0;
            renderindex = ri;
        }

        public override void Draw(SpriteBatch spriteBatch, Color color, Point dest)
        {
            if (Dirty)
                UpdateRenderIndex();

            base.Draw(spriteBatch, color, dest, Tex[renderindex]);
        }
    }
}