using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.World
{
    public enum Layer
    {
        FLOOR,
        WALL,
        WIRE,
        LENGTH,
        NULL
    }

    class TileManager
    {
        public const int TileSize = 32;
    }
}
