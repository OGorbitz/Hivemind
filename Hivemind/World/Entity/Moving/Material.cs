using Hivemind.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Hivemind.World.Entity.Moving
{
    public class Material
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

    public class DroppedMaterial : MovingEntity
    {
        public readonly Point USize = new Point(32, 32);
        public override Point Size => USize;
        public override string Type => MaterialType.Name;


        public Material MaterialType;
        public float Amount;
        
        public override Rectangle Bounds
        {
            get
            {
                return new Rectangle((int)(Pos.X - Size.X / 2), (int)(Pos.Y - Size.Y / 2), Size.X, Size.Y);
            }
        }

        public DroppedMaterial(Point pos, Material material, float amount) : base((pos.ToVector2() + new Vector2(0.25f + 0.5f * Helper.Random(), 0.25f + 0.5f * Helper.Random())) * TileManager.TileSize)
        {
            MaterialType = material;
            Amount = amount;
        }

        public DroppedMaterial(Vector2 pos, Material material) : base(pos)
        {
            MaterialType = material;
        }

        public DroppedMaterial(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Draw(MaterialType.Texture, new Rectangle((int)(Pos.X - MaterialType.Texture.Width / 2), (int)(Pos.Y - MaterialType.Texture.Height / 2), MaterialType.Texture.Width, MaterialType.Texture.Height),
                new Rectangle(0, 0, MaterialType.Texture.Width, MaterialType.Texture.Height),
                Color.White, 0f, Vector2.Zero, SpriteEffects.None,
                layerDepth: Parent.GetLayerDepth((int)Pos.Y / TileManager.TileSize) + 0.0005f);
        }
    }
}
