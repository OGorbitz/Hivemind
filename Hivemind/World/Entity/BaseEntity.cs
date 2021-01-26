using Hivemind.Input;
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
    public abstract class BaseEntity : ISerializable, ISelectable
    {
        //Entity type specific variables
        public const string UType = "BaseEntity";
        
        public virtual string Type => UType;

        public TileMap Parent;
        public SpriteController Controller;

        public bool Focused = false;


        public BaseEntity()
        {
            Controller = new SpriteController();
        }

        public BaseEntity(SerializationInfo info, StreamingContext context)
        {
            //Pos = ((V2S)info.GetValue("Pos", typeof(V2S))).ToVector2();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //info.AddValue("Pos", new V2S(Pos));
        }

        public virtual void Update(GameTime gameTime)
        {
        }

        public virtual void Destroy()
        {
            if (Selection.Selected.Contains(this))
                Selection.Selected.Remove(this);
            GC.SuppressFinalize(this);
        }

        public abstract void Draw(SpriteBatch spriteBatch, GameTime gameTime);

        /// <summary>
        /// Draws the rectangle around this object if it is selected
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="graphicsDevice"></param>
        /// <param name="gameTime"></param>
        public abstract void DrawSelected(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, GameTime gameTime);

        public void Command(Vector2 position)
        {
            throw new NotImplementedException();
        }

        public virtual bool SetFocused(bool focused)
        {
            Focused = focused;
            return true;
        }
    }
}
