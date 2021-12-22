using Hivemind.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.World.Particle
{
    public abstract class ParticleSource
    {
        public List<Particle> Particles = new List<Particle>();
        public Vector2 Position;

        public ParticleSource(Vector2 position)
        {
            Position = position;
        }

        public bool Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            for (var x = Particles.Count - 1; x >= 0; x--)
            {
                var p = (Particle)Particles[x];
                if (p.Draw(spriteBatch, gameTime))
                    Particles.RemoveAt(x);
            }

            if (Particles.Count <= 0)
                return true;
            return false;
        }
    }

    public class SparkSource : ParticleSource
    {
        public SparkSource(Vector2 position, int particles) : base(position)
        {
            for (var n = 0; n < particles; n++)
            {
                var d = (float)Helper.Random();
                var pp = new Vector2((float)Math.Cos(2f * Math.PI * d), (float)Math.Sin(2f * Math.PI * d));

                Particles.Add(new SparkParticle(new Vector3(Position, 20) + new Vector3(pp, 0) * (float)Helper.Random() * 5, new Vector3(pp * (10 + (float)Helper.Random() * 20), 50 + (float)50 * Helper.Random())));
            }
        }
    }
}
