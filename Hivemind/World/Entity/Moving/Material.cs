using Hivemind.Utility;
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
        public Material Type;

        public DroppedMaterial(Point pos, Material material) : base((pos.ToVector2() + new Vector2(0.25f + 0.5f * Helper.Random(), 0.25f + 0.5f * Helper.Random())) * TileManager.TileSize)
        {
            Type = material;
        }

        public DroppedMaterial(Vector2 pos, Material material) : base(pos)
        {
            Type = material;
        }

        public DroppedMaterial(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Draw(Type.Texture, new Rectangle((int)(Pos.X - Type.Texture.Width / 2), (int)(Pos.Y - Type.Texture.Height / 2), Type.Texture.Width, Type.Texture.Height),
                new Rectangle(0, 0, Type.Texture.Width, Type.Texture.Height),
                Color.White, 0f, Vector2.Zero, SpriteEffects.None,
                layerDepth: Parent.GetLayerDepth((int)Pos.Y / TileManager.TileSize) + 0.0005f);
        }
    }
}
