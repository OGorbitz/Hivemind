using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Hivemind.World.Tile.Floor
{
    [Serializable]
    public abstract class BaseFloor : BaseTile
    {
        //Static variables
        public const int URenderPriority = -1;
        public const float UResistance = 1;
        public new const Layer ULayer = Layer.FLOOR;

        /// <summary>
        /// Integer for what order this floor layer should be rendered. Increasing values are rendered later
        /// </summary>
        public virtual int RenderPriority => URenderPriority;
        /// <summary>
        /// A float resistance modifier for pathfinding. 1 is normal movement, lower is easier, higher is more difficult.
        /// </summary>
        public override float Resistance => UResistance;
        public override Layer Layer => ULayer;

        //Instance variables
        public bool NeedsRender = true;


        //Constructors and serializers
        public BaseFloor(Vector2 p) : base(p)
        {
        }
        public BaseFloor(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
