using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Hivemind.World.Generator.Noise;
using Microsoft.Xna.Framework;

namespace Hivemind.World.Generator
{
    public class WorldGenerator
    {
        public const float TEMPERATURE_SCALAR = 0.05f;

        private readonly OpenSimplexNoise TempMap, Noise1, Noise2;

        public double BoulderChance = 0.01;
        public double BoulderOffset;

        public long Seed;

        public TileMap TileMap;

        /// <summary>
        /// Creates instance of a simplex noise world generator
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="tileMap"></param>
        public WorldGenerator(long seed, TileMap tileMap)
        {
            Seed = seed;
            TileMap = tileMap;

            TempMap = new OpenSimplexNoise(Seed);
            var SettingNoise = new OpenSimplexNoise(Seed - 1);
            Noise1 = new OpenSimplexNoise(Seed + 1);
            Noise2 = new OpenSimplexNoise(Seed + 2);

            BoulderOffset = (SettingNoise.Evaluate(1, 1) + 1) / 2;
            BoulderOffset *= 1.0 - BoulderChance;
        }

        public double GetTemperature(Vector2 p)
        {
            p *= TEMPERATURE_SCALAR;
            return TempMap.Evaluate(p.X, p.Y);
        }

        public double GetNoise1(Vector2 p)
        {
            return Noise1.Evaluate(p.X, p.Y);
        }
    }
}