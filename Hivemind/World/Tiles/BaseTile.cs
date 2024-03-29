﻿using Hivemind.World.Colony;
using Hivemind.World.Entity.Moving;
using Hivemind.World.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Hivemind.World.Tiles
{
    [Serializable]
    public abstract class BaseTile : ISerializable
    {
        //Static variables
        public const string UName = "BaseTile";
        public const Layer ULayer = Layer.NULL;
        public const float UResistance = 0;
        public const int UBuildWork = 1000;
        public readonly Material[] UCostMaterials = { Material.CrushedRock };
        public readonly float[] UCostAmounts = { 69 };
        public readonly Color UAverageColor = Color.Magenta;

        public virtual string Name => UName;
        public virtual Layer Layer => ULayer;
        public virtual float Resistance => UResistance;
        public virtual int BuildWork => UBuildWork;
        public virtual Material[] CostMaterials => UCostMaterials;
        public virtual float[] CostAmounts => UCostAmounts;
        public virtual Color AverageColor => UAverageColor;


        //Instance variables
        public Tile Tile;
        public Point Pos => Tile.Pos;


        /// <summary>
        /// True if tile render index needs to be updated
        /// </summary>
        public bool Dirty = true;
        public bool IsHolo = false;

        public ITileMap Parent;

        //Constructors and serializers
        public BaseTile()
        {
        }

        public virtual void SetTileMap(Tile tile, ITileMap tileMap)
        {
            Tile = tile;
            Parent = tileMap;
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
        public virtual void Draw(SpriteBatch spriteBatch, Color color)
        {
            Draw(spriteBatch, color, Pos * new Point(TileManager.TileSize));
        }

        public abstract void Draw(SpriteBatch spriteBatch, Color color, Point dest);
        /// <summary>
        /// Actions to be performed if tile is destroyed
        /// </summary>
        public abstract void Destroy();

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
        new public const float UResistance = 1;
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

        protected static Texture2D texture;

        //Constructors and serializers
        public BaseFloor()
        {
        }
        public BaseFloor(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override void Destroy()
        {
            Tile.Floor = null;
        }

        public override void Draw(SpriteBatch spriteBatch, Color color)
        {
            Draw(spriteBatch, color, Pos * new Point(TileManager.TileSize));
        }
        public override void Draw(SpriteBatch spriteBatch, Color color, Point dest)
        {
            var sourcepos = new Rectangle((int)Pos.X * TileManager.TileSize % texture.Width,
                (int)Pos.Y * TileManager.TileSize % texture.Height, TileManager.TileSize, TileManager.TileSize);
            spriteBatch.Draw(texture, dest.ToVector2(),
                sourceRectangle: sourcepos, color: color);
        }
    }

    public abstract class BaseWall : BaseTile
    {
        //Static variables
        new public const float UResistance = 0;
        public new const Layer ULayer = Layer.WALL;

        /// <summary>
        /// A float resistance modifier for pathfinding. 1 is normal movement, lower is easier, higher is more difficult.
        /// </summary>
        public override float Resistance => UResistance;
        public override Layer Layer => ULayer;


        public BaseWall()
        {
        }
        public BaseWall(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override void Destroy()
        {
            Tile.Wall = null;
        }

        public override void Draw(SpriteBatch spriteBatch, Color color)
        {
            Draw(spriteBatch, color, Pos * new Point(TileManager.TileSize) - new Point(0, TileManager.WallHeight));
        }

        public virtual void Draw(SpriteBatch spriteBatch, Color color, Point dest, int atlasindex)
        {
            Rectangle dest_rect;
            Rectangle source = TextureAtlas.GetSourceRect(atlasindex);

            if (!IsHolo || (Parent.GetType() == typeof(TileMap) && ((TileMap)Parent).GetTile(Pos + new Point(0, 1)).Wall == null))
            {
                dest_rect = new Rectangle(
                    dest,
                    new Point(TileManager.TileSize, TileManager.TileSize + TileManager.WallHeight));
            }
            else
            {
                source.Height = TileManager.TileSize;
                dest_rect = new Rectangle(
                    dest,
                    new Point(TileManager.TileSize, TileManager.TileSize));
            }

            spriteBatch.Draw(
                TextureAtlas.Atlas,
                destinationRectangle: dest_rect,
                sourceRectangle: source,
                rotation: 0f,
                origin: Vector2.Zero,
                effects: SpriteEffects.None,
                color: color,
                layerDepth: Parent.GetLayerDepth((int)Pos.Y));
        }
    }

    public abstract class BaseUtility : BaseTile
    {
    }

    public class HoloTile : BaseTile, Inventory
    {
        public BaseTile Child;
        public BaseTask Task;
        public Dictionary<Material, float> Materials = new Dictionary<Material, float>();

        public static RenderTarget2D RenderTarget;

        new public const string UName = "HOLO-";
        new public static readonly Color UAverageColor = Color.DarkGreen;
        public override float Resistance
        {
            get
            {
                if (Layer == Layer.FLOOR)
                    return 1f;
                else
                    return 0;
            }
        }

        public override string Name => UName + Child.Name;
        public override Layer Layer => Child.Layer;
        public override int BuildWork => Child.BuildWork;
        public override Material[] CostMaterials => Child.CostMaterials;
        public override float[] CostAmounts => Child.CostAmounts;
        public override Color AverageColor => UAverageColor;

        public HoloTile(BaseTile tile)
        {
            Child = tile;
            Child.IsHolo = true;
        }

        public override void SetTileMap(Tile tile, ITileMap parent)
        {
            Tile = tile;
            Child.Tile = tile;
            Parent = parent;
            Child.Parent = parent;
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

        public override void Draw(SpriteBatch spriteBatch, Color color, Point dest)
        {
            Child.Draw(spriteBatch, new Color(0.25f, 0.5f, 0.125f, 0.75f));
        }

        public override void Destroy()
        {
            switch (Layer)
            {
                case Layer.WALL:
                    Tile.HoloWall = null;
                    break;
                case Layer.FLOOR:
                    Tile.HoloFloor = null;
                    break;
            }
        }

        public float Withdraw(Material m, float a)
        {
            if (Materials.ContainsKey(m))
            {
                if(Materials[m] > a)
                {
                    Materials[m] -= a;
                    return a;
                }
                else
                {
                    float r = Materials[m];
                    Materials[m] = 0;
                    return r;
                }
            }
            return 0;
        }

        public float Deposit(Material m, float a)
        {
            if (Materials.ContainsKey(m))
            {
                Materials[m] += a;
            }
            else
            {
                Materials.Add(m, a);
            }

            bool satisfied = true;
            for(int i = 0; i < CostMaterials.Length; i++)
            {
                if (!Materials.ContainsKey(CostMaterials[i]) || Materials[CostMaterials[i]] < CostAmounts[i])
                {
                    satisfied = false;
                    break;
                }
            }
            if (satisfied)
            {
                ((TileMap)Parent).TaskManager.AddTask(new BuildTask(BuildWork, this, (TileMap)Parent));
            }

            return a;
        }
        
        public Dictionary<Material, float> GetMaterials()
        {
            return Materials;
        }

        public Point GetPosition()
        {
            return Tile.Pos;
        }
    }
}
