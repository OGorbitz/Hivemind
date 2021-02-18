using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Hivemind.World
{
    [Serializable]
    class V2S
    {
        public V2S()
        {
        }

        private readonly float X;
        private readonly float Y;

        public V2S(SerializationInfo info, StreamingContext context)
        {
            X = (float)info.GetValue("X", typeof(float));
            Y = (float)info.GetValue("Y", typeof(float));
        }

        public V2S(Vector2 v)
        {
            X = v.X;
            Y = v.Y;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("X", X);
            info.AddValue("Y", Y);
        }

        public Vector2 ToVector2()
        {
            return new Vector2(X, Y);
        }

    }
}
