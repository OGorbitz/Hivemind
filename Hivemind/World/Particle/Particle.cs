using Hivemind.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.World.Particle
{
    public abstract class Particle
    {
        public const float ULifeSpan = 500;
        public const float UDrag = 0.1f;
        public const float UBounce = 0.8f;
        public const float UGravity = 200f;
        public virtual float LifeSpan => ULifeSpan;
        public virtual float Drag => UDrag;
        public virtual float Bounce => UBounce;
        public virtual float Gravity => UGravity;

        public Vector3 Position;
        public Vector3 Velocity;

        public double TimeAlive;
        public virtual Texture2D Texture => Helper.pixel;

        //Allows a Vector2 to be passed
        public Particle(Vector2 position) : this(new Vector3(position, 0), new Vector3(position, 0)) {}
        public Particle(Vector3 position, Vector3 velocity)
        {
            Position = position;
            Velocity = velocity;
        }



        /// <summary>
        /// Draws the particle
        /// <para>Note: Does not call Begin() or End() on <paramref name="spriteBatch"/></para>
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to be used</param>
        /// <param name="gameTime">GameTime object passed</param>
        /// <returns>A <see cref="bool"/>, true if particle is "dead"</returns>
        public virtual bool Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            TimeAlive += gameTime.ElapsedGameTime.TotalMilliseconds;

            float alpha = 1;

            if (TimeAlive > LifeSpan)
                alpha = (1000 - (int)(TimeAlive - LifeSpan)) / 1000f;
            if (TimeAlive > LifeSpan + 1000)
                return true;
            var effects = SpriteEffects.None;

            Position += Velocity * new Vector3((float)gameTime.ElapsedGameTime.TotalSeconds);

            Rectangle sourceRectangle = new Rectangle(0, 0, Texture.Width, Texture.Height);
            Rectangle destinationRectangle = new Rectangle((int)Position.X, (int)Position.Y + (int)Position.Z, Texture.Width, Texture.Height);

            spriteBatch.Draw(
                Texture,
                destinationRectangle: destinationRectangle,
                sourceRectangle: sourceRectangle,
                rotation: 0f,
                origin: Vector2.Zero,
                color: new Color(1f, 1f, 1f, alpha),
                effects: effects,
                layerDepth: 1);

            return false;


        }

        public static void LoadAssets(ContentManager contentManager)
        {
            SparkParticle.UTexture = contentManager.Load<Texture2D>("Particles/Spark");
        }
    }

    public class SparkParticle : Particle
    {
        public const float UDrag = 0.6f;
        public const float UBounce = 0.7f;
        public const float ULifeSpan = 1f;
        public override float LifeSpan => ULifeSpan;
        public override float Drag => UDrag;
        public override float Bounce => UBounce;

        public static Texture2D UTexture;
        public override Texture2D Texture => UTexture;

        static float FlipTime = 0.2f;
        int TimeOffset = (int)(Helper.Random() * FlipTime);

        private bool Fading = false;

        public SparkParticle(Vector3 position, Vector3 velocity) : base(position, velocity)
        {
        }

        public override bool Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            Velocity.Z -= (float)gameTime.ElapsedGameTime.TotalSeconds * Gravity;
            Velocity -= Velocity * new Vector3((float)gameTime.ElapsedGameTime.TotalSeconds * Drag);
            Position += Velocity * new Vector3((float)gameTime.ElapsedGameTime.TotalSeconds);

            if (Position.Z <= 0)
            {
                Velocity.Z = -Velocity.Z * Bounce;
                if (!Fading)
                {
                    Fading = true;
                }
            }

            float alpha = 1;

            if (Fading)
            {
                TimeAlive += gameTime.ElapsedGameTime.TotalSeconds;

                alpha = (float)(LifeSpan - TimeAlive) / LifeSpan;

                if (TimeAlive > LifeSpan)
                    return true;
            }

            var effects = SpriteEffects.None;
            if ((gameTime.TotalGameTime.TotalSeconds + TimeOffset) % FlipTime > FlipTime / 2)
                effects = SpriteEffects.FlipHorizontally;


            Rectangle sourceRectangle = new Rectangle(0, 0, Texture.Width, Texture.Height);
            Rectangle destinationRectangle = new Rectangle((int)Position.X, (int)Position.Y - (int)Position.Z, Texture.Width, Texture.Height);

            spriteBatch.Draw(
                Texture,
                destinationRectangle: destinationRectangle,
                sourceRectangle: sourceRectangle,
                rotation: 0f,
                origin: Vector2.Zero,
                color: new Color(1f, 1f, 1f, alpha),
                effects: effects,
                layerDepth: 1);

            return false;
        }
    }
}
