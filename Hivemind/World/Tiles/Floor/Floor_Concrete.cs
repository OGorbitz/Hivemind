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
    class Floor_Concrete : BaseFloor
    {
        //Static variables
        public new const string UName = "Floor_Concrete";
        public new const Layer ULayer = Layer.FLOOR;
        public new const int URenderPriority = (int)FloorPriority.Floor_Concrete;
        public new const float UResistance = 1.5f;
        new private static Color UAverageColor = Color.Pink;

        public override string Name => UName;
        public override Layer Layer => ULayer;
        public override float Resistance => UResistance;
        public override int FloorLayer => URenderPriority;
        public override Color AverageColor => UAverageColor;

        //Assets
        public static Texture2D UIcon;

        //Instance variables


        //Constructors and serializers
        public Floor_Concrete()
        {
        }
        public Floor_Concrete(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public static void LoadAssets(ContentManager content)
        {
            texture = content.Load<Texture2D>("Tiles/Floor/Floor_Concrete");
            FloorMask.Textures[URenderPriority] = texture;
            UAverageColor = Helper.AverageColor(texture);

            UIcon = content.Load<Texture2D>("Tiles/Floor/Floor_Concrete_Icon");

            TileConstructor.RegisterConstructor(UName, Constructor);
        }

        public static BaseFloor Constructor()
        {
            return new Floor_Concrete();
        }
    }
}
