using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.World.Entity.Projectile
{
    public class BaseProjectile
    {
        public Vector2 Position;
        public Vector2 Source;
        public Vector2 Velocity;
        public BaseEntity Sender;

        public float Damage = 0;

        public BaseProjectile(Vector2 source, Vector2 velocity, BaseEntity sender)
        {
            Position = Source = source;
            Velocity = velocity;
            Sender = sender;
        }

        public virtual bool Update(GameTime gameTime)
        {
            Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            //Perform collision checks, damage target, return true if target hit

            //Projectile is outside of the map bounds
            if (Position.X < 0 || Position.X > Sender.TileMap.Size * TileManager.TileSize || Position.Y < 0 || Position.Y > Sender.TileMap.Size * TileManager.TileSize)
            {
                Destroy();
            }

            return false;
        }

        public virtual void OnHit(BaseEntity target)
        {
            target.Damage(Damage, Sender);
        }

        public virtual void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {

        }

        public virtual void Destroy()
        {
            Sender.TileMap.RemoveProjectile(this);
        }
    }
}
