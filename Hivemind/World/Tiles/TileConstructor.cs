using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.World.Tiles
{
    public class TileConstructor
    {
        public delegate BaseTile TileConstructorMethod();
        public static Dictionary<string, TileConstructorMethod> TileConstructors = new Dictionary<string, TileConstructorMethod>();

        public static void RegisterConstructor(string tileIdentifier, TileConstructorMethod tileConstructorMethod)
        {
            TileConstructors[tileIdentifier] = tileConstructorMethod;
        }

        public static BaseTile ConstructTile(string tileIdentifier)
        {
            if (TileConstructors.ContainsKey(tileIdentifier))
            {
                TileConstructorMethod tileConstructorMethod = TileConstructors[tileIdentifier];
                return tileConstructorMethod();
            }
            else
            {
                return null;
            }
        }
    }
}
