using Hivemind.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Hivemind.World.Entity.Tile
{
    public class SpaseShip : TileEntity, IPowerNode
    {
        new public const string UType = "Entity_SpaseShip";
        new public readonly Point USize = new Point(3, 3);
        public Texture2D USPriteSheet;
        public override Texture2D SpriteSheet => USpriteSheet;

        new public static float USightDistance = 10f;
        public override float SightDistance => USightDistance;
        new public static string UDescription = "Neural Network Core";
        public override string Description => GetDescription();

        public override string Type => UType;
        public override Point Size => USize;

        public static Texture2D UIcon;

        public float LandingPosition = 1000f;
        public PowerNetwork PowerNetwork;

        public SpaseShip(Point p) : base(p)
        {
            Controller.AddAnimation("IDLE", new[] { 0 }, 1, false);
            Controller.SetAnimation("IDLE");
        }

        public SpaseShip(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Controller.AddAnimation("IDLE", new[] { 0 }, 1, false);
            Controller.SetAnimation("IDLE");
        }

        public override void OnPlace()
        {
            PowerNetwork = new PowerNetwork(TileMap);
            PowerNetwork.Core = this;

            base.OnPlace();
        }

        public override void Update(GameTime gameTime)
        {
            PowerNetwork.Update(gameTime);

            base.Update(gameTime);
        }


        public static void LoadAssets(ContentManager content)
        {
            USpriteSheet = content.Load<Texture2D>("Entity/TileEntity/Mechanical/SpaseShip");

            UIcon = USpriteSheet;
        }

        public string GetDescription()
        {
            string nodes = "/c[Red]ERROR! " + "\n" + "Not connected to a power network!";

            if (PowerNetwork != null)
                nodes = "There are /c[Green]" + PowerNetwork.Nodes.Count + "/c[White] nodes in this network.";

            return UDescription + "\n\n" + nodes;
        }

        public override void Destroy()
        {
            base.Destroy();

            //Implement losing mechanic
        }

        public NodeType GetNodeType()
        {
            return NodeType.CONSUMER;
        }

        public float GetPower()
        {
            return 0f;
        }

        public void UpdatePower()
        {
            
        }

        public void OnNetworkJoin(PowerNetwork powerNetwork)
        {
            
        }

        public void OnNetworkLeave()
        {
            
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
