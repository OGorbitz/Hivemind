using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Hivemind.World.Entity
{
    [Serializable]
    public class TileEntity : BaseEntity
    {
        public readonly Point USize = new Point(1, 1);
        public virtual Point Size => USize;

        public bool Updated = false, Rendered = false;

        public TileEntity(Vector2 p) : base(p)
        {
        }

        public TileEntity(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public new virtual void Update(GameTime gameTime)
        {
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
            for (var x = r.X; x < r.Right; x++)
                for (var y = r.Y; y < r.Bottom; y++)
                    Parent.RemoveTileEntity(new Vector2(x, y));

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
