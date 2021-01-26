using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Hivemind.World.Tile
{
    [Serializable]
    public abstract class BaseTile : ISerializable
    {
        //Static variables
        public const string UName = "BaseTile";
        public const Layer ULayer = Layer.NULL;
        public const float UResistance = 0;

        public virtual string Name => UName;
        public virtual Layer Layer => ULayer;
        public virtual float Resistance => UResistance;

        //Instance variables
        private Point Position;
        public Point Pos => Position;

        /// <summary>
        /// True if tile render index needs to be updated
        /// </summary>
        public bool Dirty = true;

        public ITileMap Parent;

        //Constructors and serializers
        public BaseTile(Point p)
        {
            Position = p;
        }

        public BaseTile(SerializationInfo info, StreamingContext context)
        {
            //Position = ((V2S)info.GetValue("Pos", typeof(V2S))).ToVector2();
        }
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //info.AddValue("Pos", new V2S(Position));
        }

        public abstract void Draw(SpriteBatch spriteBatch);
        
        /// <summary>
        /// Actions to be performed if tile is destroyed
        /// </summary>
        public virtual void Destroy()
        {
            Console.WriteLine(Name + " tile broken");
            Parent.RemoveTile(Pos, Layer);
        }

        /// <summary>
        /// Actions to be performed on tile update call
        /// </summary>
        public virtual void Update(GameTime gameTime)
        {
        }

        public virtual Rectangle Collider()
        {
            return new Rectangle((int)Pos.X * TileManager.TileSize, (int)Pos.Y * TileManager.TileSize, TileManager.TileSize, TileManager.TileSize);
        }
    }
}
