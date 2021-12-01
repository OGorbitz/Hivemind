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
    public class Bush1 : TileEntity
    {
        public const string UType = "Entity_Bush1";
        public readonly Point USize = new Point(1, 1);
        public static Texture2D USpriteSheet;
        public override Texture2D SpriteSheet => USpriteSheet;
        public override string Type => UType;
        public override Point Size => USize;

        public static Texture2D UIcon;

        public Bush1(Point p) : base(p)
        {
            Controller.AddAnimation("IDLE", new[] { 0 }, 1, true);
            Controller.SetAnimation("IDLE");
        }

        public Bush1(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Controller.AddAnimation("IDLE", new[] { 0 }, 1, true);
            Controller.SetAnimation("IDLE");
        }

        public static void LoadAssets(ContentManager content)
        {
            USpriteSheet = content.Load<Texture2D>("Entity/TileEntity/Flora/Bush");
            UIcon = USpriteSheet;
        }
    }
}
