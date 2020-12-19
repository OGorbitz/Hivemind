﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Hivemind.World.Tile.Floor
{
    [Serializable]
    class Floor_Concrete : BaseFloor
    {
        //Static variables
        public new const string UName = "Floor_Concrete";
        public new const Layer ULayer = Layer.FLOOR;
        public new const int URenderPriority = (int)FloorPriority.Floor_Concrete;
        public new const float UResistance = 1.5f;

        public override string Name => UName;
        public override Layer Layer => ULayer;
        public override float Resistance => UResistance;
        public override int RenderPriority => URenderPriority;

        //Assets
        public static Texture2D UIcon;
        private static Texture2D texture;

        //Instance variables


        //Constructors and serializers
        public Floor_Concrete(Vector2 p) : base(p)
        {
        }
        public Floor_Concrete(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public static void LoadAssets(ContentManager content)
        {
            texture = content.Load<Texture2D>("Floor_Concrete");
            UIcon = content.Load<Texture2D>("Floor_Concrete_Icon");
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var sourcepos = new Rectangle((int)Pos.X % 8 * TileManager.TileSize,
                (int)Pos.Y % 8 * TileManager.TileSize, TileManager.TileSize, TileManager.TileSize);
            spriteBatch.Draw(texture, new Vector2(Pos.X * TileManager.TileSize, Pos.Y * TileManager.TileSize),
                sourceRectangle: sourcepos, color: Color.White);
        }
    }
}