using Hivemind.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Hivemind.World.Tiles.Floor
{
    [Serializable]
    class Floor_Dirt : BaseFloor
    {
        //Static variables
        public new const string UName = "Floor_Dirt";
        public new const Layer ULayer = Layer.FLOOR;
        public new const int URenderPriority = (int)FloorPriority.Floor_Dirt;
        public new const float UResistance = 1.5f;
        private static Color UAverageColor = Color.Pink;

        public override string Name => UName;
        public override Layer Layer => ULayer;
        public override float Resistance => UResistance;
        public override int FloorLayer => URenderPriority;
        public override Color AverageColor => UAverageColor;

        //Assets
        public static Texture2D UIcon;
        private static Texture2D texture;

        //Instance variables


        //Constructors and serializers
        public Floor_Dirt()
        {
        }
        public Floor_Dirt(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public static void LoadAssets(ContentManager content)
        {
            texture = content.Load<Texture2D>("Tiles/Floor/Floor_Dirt");
            FloorMask.Textures[URenderPriority] = texture;
            UAverageColor = Helper.AverageColor(texture);

            UIcon = content.Load<Texture2D>("Tiles/Floor/Floor_Dirt_Icon");
        }

        public override void Draw(SpriteBatch spriteBatch, Color color)
        {
            var sourcepos = new Rectangle((int)Pos.X % 4 * TileManager.TileSize,
                (int)Pos.Y % 4 * TileManager.TileSize, TileManager.TileSize, TileManager.TileSize);
            spriteBatch.Draw(texture, new Vector2(Pos.X * TileManager.TileSize, Pos.Y * TileManager.TileSize),
                sourceRectangle: sourcepos, color: color);
        }
    }
}