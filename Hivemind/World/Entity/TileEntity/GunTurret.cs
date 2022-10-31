using Hivemind.Utility;
using Hivemind.World.Entity.Projectile;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.World.Entity
{
    public class GunTurret : TileEntity, IPowerNode
    {
        new public const string UType = "GunTurret";
        public override string Type => UType;
        new public const string UDescription = "A basic gun turret.\nHopefully this does more than piss them off";
        public override string Description => GetDescription();
        new public readonly Point USize = new Point(1);
        public override Point Size => USize;
        new public static Texture2D USpriteSheet;

        public static Texture2D UIcon;
        public override Texture2D SpriteSheet => USpriteSheet;

        public static NodeType NodeType = NodeType.PRODUCER;
        public static float PowerUsage = 100;

        public PowerNetwork PNetwork;

        public TimeSpan LastShot;

        public GunTurret(Point position) : base(position)
        {
            
        }

        public NodeType GetNodeType()
        {
            throw new NotImplementedException();
        }

        public float GetPower()
        {
            return -PowerUsage;
        }

        public void UpdatePower()
        {
            throw new NotImplementedException();
        }

        public void OnNetworkJoin(PowerNetwork powerNetwork)
        {
            throw new NotImplementedException();
        }

        public void OnNetworkLeave()
        {
            throw new NotImplementedException();
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

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (LastShot == TimeSpan.MinValue)
                LastShot = gameTime.TotalGameTime;
            else if ((gameTime.TotalGameTime - LastShot).TotalMilliseconds > 1000)
            {
                LastShot = gameTime.TotalGameTime;
                BulletProjectile p = new BulletProjectile(Pos * TileManager.TileSize + new Vector2(TileManager.TileSize / 2), new Vector2(1000), this);
                TileMap.AddProjectile(p);
            }
        }

        public static void LoadAssets(ContentManager content)
        {
            USpriteSheet = content.Load<Texture2D>("Entity/TileEntity/Mechanical/GunTurret");
            UIcon = USpriteSheet;

            EntityManager.AddConstructor(UType, Constructor);
        }
        public static BaseEntity Constructor(Vector2 position)
        {
            return new GunTurret(position.ToPoint());
        }

        public string GetDescription()
        {
            string nodes = "/c[Red]ERROR! " + "\n" + "Not connected to a power network!";

            if (PNetwork != null)
                nodes = "There are /c[Green]" + PNetwork.Nodes.Count + "/c[White] nodes in this network.";

            return UDescription + "\n\n" + nodes;
        }

        public List<IPowerNode> GetConnections()
        {
            throw new NotImplementedException();
        }

        public PowerNetwork GetPowerNetwork()
        {
            throw new NotImplementedException();
        }
    }
}
