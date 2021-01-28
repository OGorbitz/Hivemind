﻿using Hivemind.World.Colony;
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
        public const int UBuildWork = 1000;

        public virtual string Name => UName;
        public virtual Layer Layer => ULayer;
        public virtual float Resistance => UResistance;
        public virtual int BuildWork => UBuildWork;

        //Instance variables
        public Point Pos { get; private set; }

        /// <summary>
        /// True if tile render index needs to be updated
        /// </summary>
        public bool Dirty = true;

        protected ITileMap _parent;
        public virtual ITileMap Parent { 
            get
            {
                return _parent;
            }
            set
            {
                _parent = value;
            }
        }

        //Constructors and serializers
        public BaseTile(Point p)
        {
            Pos = p;
        }

        public BaseTile(SerializationInfo info, StreamingContext context)
        {
            //Position = ((V2S)info.GetValue("Pos", typeof(V2S))).ToVector2();
        }
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //info.AddValue("Pos", new V2S(Position));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, Color.White);
        }
        public abstract void Draw(SpriteBatch spriteBatch, Color color);
        
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
        public virtual int FloorLayer => URenderPriority;
        /// <summary>
        /// A float resistance modifier for pathfinding. 1 is normal movement, lower is easier, higher is more difficult.
        /// </summary>
        public override float Resistance => UResistance;
        public override Layer Layer => ULayer;


        //Constructors and serializers
        public BaseFloor(Point p) : base(p)
        {
        }
        public BaseFloor(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public class HoloTile : BaseTile
    {
        public BaseTile Child;
        public BaseTask Task;


        public static RenderTarget2D RenderTarget;

        public const float UResistance = 0;
        public const string UName = "HOLO-";
        public override float Resistance => UResistance;
        public override string Name => UName + Child.Name;
        public override Layer Layer => Child.Layer;
        public override int BuildWork => Child.BuildWork;


        public override ITileMap Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                _parent = value;
                Child.Parent = value;

                if (_parent.GetType() == typeof(TileMap))
                    Task = new BuildTask(Child.BuildWork, this, (TileMap)Parent);
                else
                    throw new Exception("Trying to set holotile in an editor tilemap... THIS SHOULD NOT HAPPEN!");
            }
        }

        public HoloTile(BaseTile tile) : base(tile.Pos)
        {
            Child = tile;
        }

        public static void LoadAssets(GraphicsDevice graphicsDevice)
        {
            RenderTarget = new RenderTarget2D(
                graphicsDevice,
                TileManager.TileSize,
                TileManager.TileSize + TileManager.WallHeight,
                false,
                graphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
        }

        public override void Draw(SpriteBatch spriteBatch, Color color)
        {
            Child.Draw(spriteBatch, new Color(0.3f, 0.3f, 0.3f, 0.5f));
        }
    }
}
