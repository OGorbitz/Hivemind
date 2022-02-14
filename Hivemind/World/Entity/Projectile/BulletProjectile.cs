using Hivemind.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hivemind.World.Entity.Projectile
{
    public class BulletProjectile : BaseProjectile
    {
        public BulletProjectile(Vector2 source, Vector2 velocity, BaseEntity sender) : base(source, velocity, sender)
        {

        }

        public override bool Update(GameTime gameTime)
        {
            return base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            //A vector to be used for the end of the projectile's trail
            Vector2 p = Position - Velocity * 0.2f;


            //Check if tail is beyond the source. If so, use the source as the beginning of the tail
            if (Velocity.X > 0 && p.X < Source.X)
                p = Source;

            if(Velocity.X < 0 && p.X > Source.X)
                p = Source;

            if (Velocity.Y > 0 && p.Y < Source.Y)
                p = Source;

            if (Velocity.Y < 0 && p.Y < Source.Y)
                p = Source;

            Helper.DrawLine(spriteBatch, p, Position, Color.Yellow, 2);


            spriteBatch.Draw(Helper.pixel, new Rectangle(Position.ToPoint() - new Point(2), new Point(4)), Color.Red);
        }
    }
}
