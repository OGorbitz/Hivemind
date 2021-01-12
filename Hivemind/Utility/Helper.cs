using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.Utility
{
    public class Helper
    {
        public static Random Rand;

        public static float Random()
        {
            if (Rand == null)
                Rand = new Random((int)Hivemind.CurrentGameTime.TotalGameTime.TotalMilliseconds);
            return (float)Rand.NextDouble();
        }
    }
}
