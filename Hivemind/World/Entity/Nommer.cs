using Hivemind.Utility;
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
    class Nommer : BaseEntity
    {
        public const string UType = "Nommer";
        public const int USpeed = 40;
        public static Texture2D UIcon;

        public override string Type => UType;

        public Vector2 Vel = Vector2.Zero;
        public bool wait = false;

        public TimeSpan NextAction;

        public Nommer(Vector2 pos) : base(pos)
        {
            Controller.AddAnimation("IDLE", new[] { 0, 0, 1 }, 4, true);
            //Controller.AddAnimation("LEFT", new[] { 2, 3, 4, 5, 6 }, 5, true);
            //Controller.AddAnimation("RIGHT", new[] { 7, 8, 9, 10, 11 }, 5, true);
            Controller.AddAnimation("DOWN", new[] { 4, 0, 5, 0 }, 6, true);
            Controller.AddAnimation("UP", new[] { 10, 6, 11, 6 }, 6, true);
            Controller.SetAnimation("IDLE");
        }

        public Nommer(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Controller.AddAnimation("IDLE", new[] { 0, 0, 1 }, 3, true);
            //Controller.AddAnimation("LEFT", new[] { 2, 3, 4, 5, 6 }, 5, true);
            //Controller.AddAnimation("RIGHT", new[] { 7, 8, 9, 10, 11 }, 5, true);
            Controller.AddAnimation("UP", new[] { 4, 0, 5, 0 }, 4, true);
            Controller.AddAnimation("DOWN", new[] { 10, 6, 11, 6 }, 4, true);
            Controller.SetAnimation("IDLE");
        }

        public static void LoadAssets(ContentManager content)
        {
            var sprites = new Texture2D[12];
            for (var i = 0; i < sprites.Length; i++) sprites[i] = content.Load<Texture2D>("Entity/Alien/Nommer/" + (i + 1));
            EntityManager.sprites.Add(UType, sprites);

            UIcon = sprites[0];
        }

        public override void Update(GameTime gameTime)
        {
            if (NextAction == null)
                NextAction = gameTime.TotalGameTime + new TimeSpan(0, 0, 0, 0, milliseconds: (int)(Helper.Random() * 4000 + 4000));
            if (gameTime.TotalGameTime > NextAction)
            {
                if (wait)
                {
                    NextAction = gameTime.TotalGameTime + new TimeSpan(0, 0, 0, 0, milliseconds: (int)(Helper.Random() * 4000 + 4000));
                    wait = false;
                    double rand = Helper.Random() * 2 * Math.PI;
                    Vel = new Vector2((float)Math.Cos(rand), (float)Math.Sin(rand));
                    Vel *= USpeed;
                }
                else
                {
                    NextAction = gameTime.TotalGameTime + new TimeSpan(0, 0, 0, 0, milliseconds: (int)(Helper.Random() * 4000 + 4000));
                    wait = true;
                    Vel = Vector2.Zero;
                }
            }

            Pos += Vel * (float)(gameTime.ElapsedGameTime.TotalMilliseconds / 1000);

            if (Vel.Y > 0)
                Controller.SetAnimation("DOWN");
            if (Vel.Y < 0)
                Controller.SetAnimation("UP");
            if (Vel.Y == 0)
                Controller.SetAnimation("IDLE");

            base.Update(gameTime);
        }
    }
}
