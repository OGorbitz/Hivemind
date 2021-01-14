using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Hivemind.World.Entity
{
    [Serializable]
    public class MovingEntity : BaseEntity
    {
        public const string UType = "MovingEntity";
        public override string Type => UType;

        public readonly Point USize = new Point(TileManager.TileSize, TileManager.TileSize);
        public virtual Point Size => USize;

        public HashCell<MovingEntity> Cell;
        public int ID;

        public MovingEntity(Vector2 pos) : base(pos)
        {
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

            base.Update(gameTime);
        }

        public virtual void Destroy()
        {
            Parent.RemoveEntity(this);

            base.Destroy();
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
}
