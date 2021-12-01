using Hivemind.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using static Hivemind.Utility.PowerNetwork;

namespace Hivemind.World.Entity.Tile
{
    public class BasicGenerator : TileEntity, IPowerNode
    {
        public const string UType = "BasicGenerator";
        public override string Type => UType;
        public const string UDescription = "Just a plain old basic generator.\nWhat is this doing on an alien planet?";
        public override string Description => UDescription;
        public readonly Point USize = new Point(1);
        public override Point Size => USize;
        public static Texture2D USpriteSheet;

        public static Texture2D UIcon;
        public override Texture2D SpriteSheet => USpriteSheet;

        public static NodeType NodeType = NodeType.PRODUCER;
        public static float PowerOutput = 10;

        public PowerNetwork PNetwork;
        public BasicGenerator(Point position) : base(position)
        {

            Controller.AddAnimation("OFF", new[] { 0 }, 1, true);
            Controller.AddAnimation("ON", new[] { 1 }, 1, true);
            Controller.SetAnimation("OFF");
        }

        public BasicGenerator(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public NodeType GetNodeType()
        {
            return NodeType;
        }

        public float GetPower()
        {
            return PowerOutput;
        }
        public void SetNetwork(PowerNetwork powerNetwork)
        {
            PNetwork = powerNetwork;
        }

        public void OnNetworkJoin(PowerNetwork powerNetwork)
        {
        }

        public void OnNetworkJoin()
        {
            throw new NotImplementedException();
        }

        public void OnNetworkLeave()
        {
        }

        public void PowerOff()
        {
            Controller.SetAnimation("OFF");
        }

        public void PowerOn()
        {
            Controller.SetAnimation("ON");
        }

        public void UpdatePower()
        {
        }

        public virtual void OnPlace()
        {
            var t = TileMap.GetTile(Pos.ToPoint());
            if (t != null && t.PowerCable != null)
            {
                PNetwork.AddNode(this);
            }
        }

        public static void LoadAssets(ContentManager content)
        {
            USpriteSheet = content.Load<Texture2D>("Entity/TileEntity/Mechanical/BasicGenerator");
            UIcon = USpriteSheet;

            EntityManager.AddConstructor(UType, Constructor);
        }

        public static BaseEntity Constructor(Vector2 position)
        {
            return new BasicGenerator(position.ToPoint());
        }
    }
}
