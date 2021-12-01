using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Hivemind.World.Entity.Tile
{
    class SpaseShip : TileEntity
    {
        public const string UType = "Entity_SpaseShip";
        public readonly Point USize = new Point(3, 3);
        public Texture2D USPriteSheet;
        public override Texture2D SpriteSheet => USpriteSheet;

        public override string Type => UType;
        public override Point Size => USize;

        public static Texture2D UIcon;

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

        public static void LoadAssets(ContentManager content)
        {
            USpriteSheet = content.Load<Texture2D>("Entity/TileEntity/Mechanical/SpaseShip");

            UIcon = USpriteSheet;
        }

        public override void Destroy()
        {
            base.Destroy();

            //Implement losing mechanic
        }
    }
}
