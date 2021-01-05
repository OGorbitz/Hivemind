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

        public double BushChance = 0.5;
        public double RockChance = 0.3;
        public double BushOffset;
        public double RockOffset;

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

            BushOffset = (SettingNoise.Evaluate(1, 1) + 1) / 2;
            BushOffset *= 1.0 - BushChance;
            RockOffset = (SettingNoise.Evaluate(1, 2) + 1) / 2;
            RockOffset *= 1.0 - RockChance;
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

        public double GetNoise2(Vector2 p)
        {
            return Noise2.Evaluate(p.X, p.Y);
        }
    }
}