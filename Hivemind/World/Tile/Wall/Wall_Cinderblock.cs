using System;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Hivemind.World.Tile.Wall
{
    [Serializable]
    internal class Wall_Cinderblock : BaseTile
    {
        //Static variables
        public const string UName = "WALL_CINDERBLOCK";
        public const Layer ULayer = Layer.WALL;
        public const float UResistance = -1;

        public override string Name => UName;
        public override float Resistance => UResistance;
        public override Layer Layer => ULayer;
        public virtual Texture2D Icon => UIcon;

        //Assets
        public static Texture2D UIcon;
        private static int[] Tex;

        //Custom variables
        private static int[,] tilecheck;
        private int renderindex;

        public Wall_Cinderblock(Vector2 p) : base(p)
        {
        }

        public Wall_Cinderblock(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public static void LoadAssets(ContentManager content, GraphicsDevice graphicsDevice)
        {
            var textures = new Texture2D[16];
            Tex = new int[textures.Length];
            for (var i = 0; i < 16; i++) textures[i] = content.Load<Texture2D>("Tiles/Wall/Cinderblock/" + (i + 1));

            tilecheck = new int[4, 3]
            {
                {1, 0, -1},
                {2, 1, 0},
                {4, 0, 1},
                {8, -1, 0}
            };

            UIcon = textures[0];

            for (int i = 0; i < textures.Length; i++)
            {
                Tex[i] = TextureAtlas.AddTexture(textures[i], graphicsDevice);
            }
        }

        public static void Unlock()
        {
        }

        public void UpdateRenderIndex()
        {
            var ri = 0;

            for (var i = 0; i < 4; i++)
                try
                {
                    var t = Parent.GetTile(new Vector2((int) Pos.X + tilecheck[i, 1], (int) Pos.Y + tilecheck[i, 2]), Layer.WALL);
                    if (t.Name == Name) ri += tilecheck[i, 0];
                }
                catch
                {
                    ri += tilecheck[i, 0];
                }

            if (ri >= 16) ri = 0;
            renderindex = ri;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Dirty)
                UpdateRenderIndex();

            var dest = new Rectangle(
                new Point((int) Pos.X * TileManager.TileSize,
                    (int) Pos.Y * TileManager.TileSize - TileManager.WallHeight),
                new Point(TileManager.TileSize, TileManager.TileSize + TileManager.WallHeight));

            spriteBatch.Draw(
                TextureAtlas.Atlas,
                destinationRectangle: dest,
                sourceRectangle: TextureAtlas.GetSourceRect(Tex[renderindex]),
                rotation: 0f,
                origin: Vector2.Zero,
                effects: SpriteEffects.None,
                color: Color.White,
                layerDepth: Parent.GetLayerDepth((int)Pos.Y));
        }
    }
}