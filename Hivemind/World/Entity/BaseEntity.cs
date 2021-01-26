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

    [Serializable]
    public class MovingEntity : BaseEntity
    {
        public const string UType = "MovingEntity";
        public override string Type => UType;

        public readonly Point USize = new Point(TileManager.TileSize, TileManager.TileSize);
        public virtual Point Size => USize;

        public HashCell<MovingEntity> Cell;
        public int ID;
        public Vector2 Pos;

        public MovingEntity(Vector2 pos)
        {
            Pos = pos;
            ID = EntityManager.GetID();
        }

        public MovingEntity(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            ID = info.GetInt32("ID");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ID", ID);
        }

        public override void Update(GameTime gameTime)
        {
            if (Cell != null)
                if (!(Pos.X >= Cell.Position.X && Pos.X <= Cell.Position.X + Cell.Size.X &&
                        Pos.Y >= Cell.Position.Y && Pos.Y <= Cell.Position.Y + Cell.Size.Y))
                {
                    Cell.RemoveMember(this);
                    Cell = null;
                }
            if (Cell == null)
                if (Pos.X >= 0 && Pos.X < Parent.Size * TileManager.TileSize && Pos.Y >= 0 && Pos.Y < Parent.Size * TileManager.TileSize)
                    Parent.AddEntity(this);

            if (Focused)
            {
                Vector2 FPos = Pos;
                FPos.Floor();
                Parent.Cam.MoveTo(FPos);
                Focused = false;
            }

            base.Update(gameTime);
        }

        public virtual void Destroy()
        {
            Parent.RemoveEntity(this);

            base.Destroy();
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            var frame = Controller.GetFrame(gameTime);
            var sprite = EntityManager.GetSprite(Type, frame);
            spriteBatch.Draw(sprite, new Rectangle((int)(Pos.X - sprite.Width / 2), (int)(Pos.Y - sprite.Height / 2), sprite.Width, sprite.Height),
                new Rectangle(0, 0, sprite.Width, sprite.Height),
                Color.White, 0f, Vector2.Zero, SpriteEffects.None,
                layerDepth: Parent.GetLayerDepth((int)Pos.Y / TileManager.TileSize) + 0.0005f);
        }

        public override void DrawSelected(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            spriteBatch.Draw(EntityManager.Selected, GetBounds(), Color.White);
        }

        public virtual Rectangle GetBounds()
        {
            return new Rectangle((int)(Pos.X - Size.X / 2), (int)(Pos.Y - Size.Y / 2), Size.X, Size.Y);
        }
    }

    [Serializable]
    public class TileEntity : BaseEntity
    {
        public readonly Point USize = new Point(1, 1);
        public virtual Point Size => USize;

        public bool Updated = false, Rendered = false;

        public Point Pos;

        public TileEntity(Point p)
        {
            Pos = p;
        }

        public TileEntity(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public new virtual void Update(GameTime gameTime)
        {
            if (Focused)
            {
                Vector2 FPos = (Pos.ToVector2() + new Vector2(0.5f)) * TileManager.TileSize;
                FPos.Floor();
                Parent.Cam.MoveTo(FPos);
                Focused = false;
            }

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            var frame = Controller.GetFrame(gameTime);
            var sprite = EntityManager.GetSprite(Type, frame);
            spriteBatch.Draw(sprite, new Rectangle((int)(Pos.X * TileManager.TileSize), (int)(Pos.Y * TileManager.TileSize), TileManager.TileSize, TileManager.TileSize),
                new Rectangle(0, 0, TileManager.TileSize, TileManager.TileSize),
                Color.White, 0f, Vector2.Zero, SpriteEffects.None,
                layerDepth: Parent.GetLayerDepth((int)Pos.Y) + 0.0005f);
        }

        public override void Destroy()
        {
            Rectangle r = GetBounds();
            for (int x = r.X; x < r.Right; x++)
                for (int y = r.Y; y < r.Bottom; y++)
                    Parent.RemoveTileEntity(new Point(x, y));

            base.Destroy();
        }

        public virtual Rectangle GetBounds()
        {
            return new Rectangle((int)Pos.X, (int)Pos.Y, Size.X, Size.Y);
        }

        public override void DrawSelected(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            Rectangle r = new Rectangle((int)(Pos.X * TileManager.TileSize), (int)(Pos.Y * TileManager.TileSize), Size.X * TileManager.TileSize, Size.Y * TileManager.TileSize);
            spriteBatch.Draw(EntityManager.Selected, r, Color.White);
        }
    }
}
