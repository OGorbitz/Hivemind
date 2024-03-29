﻿using Hivemind.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using static Hivemind.Utility.PowerNetwork;

namespace Hivemind.World.Entity
{
    public class BasicGenerator : TileEntity, IPowerNode
    {
        new public const string UType = "BasicGenerator";
        public override string Type => UType;
        new public const string UDescription = "Just a plain old basic generator.\nWhat is this doing on an alien planet?";
        public override string Description => GetDescription();
        new public readonly Point USize = new Point(1);
        public override Point Size => USize;
        new public static Texture2D USpriteSheet;

        public static Texture2D UIcon;
        public override Texture2D SpriteSheet => USpriteSheet;

        public static NodeType NodeType = NodeType.PRODUCER;
        public static float PowerOutput = 1000;

        public PowerNetwork PowerNetwork;
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

        public void OnNetworkJoin(PowerNetwork powerNetwork)
        {
            PowerNetwork = powerNetwork;
        }

        public void OnNetworkLeave()
        {
            PowerNetwork = null;
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

        public override void OnPlace()
        {
            var t = TileMap.GetTile(Pos.ToPoint());
            if (t != null)
            {
                if (t.PowerCable != null && t.PowerCable.PowerNetwork != null)
                {
                    t.PowerCable.PowerNetwork.AddNode(this);
                }
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

        public string GetDescription()
        {
            string nodes = "/c[Red]ERROR! " + "\n" + "Not connected to a power network!";

            if(PowerNetwork != null)
                nodes = "There are /c[Green]" + PowerNetwork.Nodes.Count + "/c[White] nodes in this network.";

            return UDescription + "\n\n" + nodes;
        }

        public override void Destroy()
        {
            if (PowerNetwork != null)
                PowerNetwork.RemoveNode(this);
            base.Destroy();
        }

        public List<IPowerNode> GetConnections()
        {
            List<IPowerNode> nodes = new List<IPowerNode>();
            
            for (int i = (int)Pos.X; i < Pos.X + Size.X; i++)
            {
                for (int j = (int)Pos.Y; j < Pos.Y + Size.Y; j++)
                {
                    var t = TileMap.GetTile(new Point(i, j));
                    if (t == null || t.PowerCable == null)
                        continue;
                    
                    nodes.Add(t.PowerCable);
                }
            }
            
            return nodes;
        }

        public PowerNetwork GetPowerNetwork()
        {
            return PowerNetwork;
        }
    }
}