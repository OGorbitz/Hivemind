using Hivemind.Sprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Hivemind.World.Entity
{
    [Serializable]
    public abstract class BaseEntity : ISerializable
    {
        //Entity type specific variables
        public const string UType = "BaseEntity";
        
        public virtual string Type => UType;

        public TileMap Parent;
        public SpriteController Controller;

        public Vector2 Pos;

        public BaseEntity(Vector2 pos)
        {
            Pos = pos;
            Controller = new SpriteController();
        }

        public BaseEntity(SerializationInfo info, StreamingContext context)
        {
            Pos = ((V2S)info.GetValue("Pos", typeof(V2S))).ToVector2();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Pos", new V2S(Pos));
        }

        public virtual void Update(GameTime gameTime)
        {
        }

        public virtual void Destroy()
        {

            GC.SuppressFinalize(this);
        }

        public virtual void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            var frame = Controller.GetFrame(gameTime);
            var sprite = EntityManager.GetSprite(Type, frame);
            spriteBatch.Draw(sprite, new Rectangle((int)(Pos.X - sprite.Width / 2), (int)(Pos.Y - sprite.Height / 2), sprite.Width, sprite.Height),
                new Rectangle(0, 0, sprite.Width, sprite.Height),
                Color.White, 0f, Vector2.Zero, SpriteEffects.None,
                layerDepth: Parent.GetLayerDepth((int) Pos.Y / TileManager.TileSize) + 0.0005f);
        }
    }
}
