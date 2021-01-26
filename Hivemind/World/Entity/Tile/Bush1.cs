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
            var sprites = new Texture2D[1];
            for(var i = 0; i < 1; i++)
            {
                sprites[i] = content.Load<Texture2D>("Entity/TileEntity/Flora/Bush" + (i + 1));
            }
            EntityManager.sprites.Add(UType, sprites);

            UIcon = sprites[0];
        }
    }
}
