using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Hivemind.World.Entity.Moving
{
    class Material
    {
        public Texture2D Texture;
        public string Name { get; private set; }

        public Material(string name, string texture)
        {
            Name = name;
            Texture = Hivemind.CManager.Load<Texture2D>("Items/" + texture);
        }

        public static readonly Material CrushedRock = new Material("Crushed Rock", "CrushedRock");
        public static readonly Material IronOre = new Material("Iron Ore", "IronOre");
    }

    class DroppedMaterial : MovingEntity
    {

        public DroppedMaterial(Vector2 pos, Material material) : base(pos)
        {
        }

        public DroppedMaterial(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
