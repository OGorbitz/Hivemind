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
        public const bool UIsTileEntity = false;
        
        public virtual string Type => UType;
        public virtual bool IsTileEntity => UIsTileEntity;

        public int ID;

        public TileMap Parent;
        public SpriteController Controller;

        public Vector2 Pos;

        public BaseEntity(Vector2 pos)
        {
            ID = EntityManager.GetID();
            Pos = pos;
            Controller = new SpriteController();
        }

        public BaseEntity(SerializationInfo info, StreamingContext context)
        {
            Pos = ((V2S)info.GetValue("Pos", typeof(V2S))).ToVector2();
            ID = info.GetInt32("ID");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Pos", new V2S(Pos));
            info.AddValue("ID", ID);
        }

        public virtual void Update(GameTime gameTime)
        {

        }

        public virtual void Destroy()
        {
            Parent.RemoveEntity(this);

            GC.SuppressFinalize(this);
        }

        public virtual void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            var frame = Controller.GetFrame(gameTime);
            var sprite = EntityManager.GetSprite(Type, frame);
        }
    }
}
