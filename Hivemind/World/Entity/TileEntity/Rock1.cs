using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Hivemind.World.Entity
{
    [Serializable]
    class Rock1 : TileEntity
    {
        public const string UType = "Entity_Rock1";
        public readonly Point USize = new Point(1, 1);

        public override string Type => UType;
        public override Point Size => USize;
        public static Texture2D USpriteSheet;
        public override Texture2D SpriteSheet => USpriteSheet;

        public static Texture2D UIcon;

        public Rock1(Point p) : base(p)
        {
            Controller.AddAnimation("IDLE", new[] { 0 }, 1, true);
            Controller.SetAnimation("IDLE");
        }

        public Rock1(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Controller.AddAnimation("IDLE", new[] { 0 }, 1, true);
            Controller.SetAnimation("IDLE");
        }

        public static void LoadAssets(ContentManager content)
        {
            USpriteSheet = content.Load<Texture2D>("Entity/TileEntity/Natural/Rock");
            UIcon = USpriteSheet;
        }
    }
}
