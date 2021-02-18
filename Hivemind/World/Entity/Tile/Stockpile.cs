using Hivemind.World.Entity.Moving;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Hivemind.World.Entity.Tile
{
    public class Stockpile : TileEntity
    {
        public const string UType = "Entity_Stockpile";
        public readonly Point USize = new Point(3, 3);

        public override string Type => UType;
        public override Point Size => USize;

        public Dictionary<Material, float> Stored;

        public static Texture2D UIcon;

        public Stockpile(Point p) : base(p)
        {
        }

        public Stockpile(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public void AddMaterial(Material material, float Amount)
        {
            if (Amount <= 0)
                return;

            if (Stored.ContainsKey(material))
            {
                Stored[material] += Amount;
            }
            else
            {
                Stored.Add(material, Amount);
            }
        }

        public float TakeMaterial(Material material, float Amount)
        {
            if (Stored.ContainsKey(material))
            {
                if(Stored[material] > Amount)
                {
                    Stored[material] -= Amount;
                    return Amount;
                }
                else
                {
                    float rtn = Stored[material];
                    Stored.Remove(material);
                    return rtn;
                }
            }
            return 0;
        }
    }
}
