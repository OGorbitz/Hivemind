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
    public class SmallDrone : BaseEntity
    {
        public const string UType = "SmallDrone";
        public const int USpeed = 40;
        public static Texture2D UIcon;

        public override string Type => UType;

        public Vector2 Vel = Vector2.Zero;
        public bool wait = false;

        public TimeSpan NextAction;

        public SmallDrone(Vector2 pos) : base(pos)
        {
            Controller.AddAnimation("IDLE", new[] { 0, 1}, 3, true);
            Controller.AddAnimation("LEFT", new[] { 2, 3, 4, 5, 6}, 5, true);
            Controller.AddAnimation("RIGHT", new[] { 7, 8, 9, 10, 11}, 5, true);
            Controller.AddAnimation("DOWN", new[] { 12, 13, 14 }, 4, true);
            Controller.AddAnimation("UP", new[] { 15, 16, 17 }, 4, true);
            Controller.SetAnimation("IDLE");
        }

        public SmallDrone(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Controller.AddAnimation("IDLE", new[] { 0, 1}, 3, true);
            Controller.AddAnimation("LEFT", new[] { 2, 3, 4, 5, 6}, 5, true);
            Controller.AddAnimation("RIGHT", new[] { 7, 8, 9, 10, 11}, 5, true);
            Controller.AddAnimation("UP", new[] { 12, 13, 14 }, 4, true);
            Controller.AddAnimation("DOWN", new[] { 15, 16, 17 }, 4, true);
            Controller.SetAnimation("IDLE");
        }

        public static void LoadAssets(ContentManager content)
        {
            var sprites = new Texture2D[18];
            for (var i = 0; i < 18; i++) sprites[i] = content.Load<Texture2D>("Entity/Robot/SmallDrone/" + (i + 1));
            EntityManager.sprites.Add("SmallDrone", sprites);

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
                    NextAction = gameTime.TotalGameTime + new TimeSpan(0, 0, 0, 0, milliseconds: (int)(Helper.Random() * 4000 + 2000));
                    wait = false;
                    double rand = Helper.Random() * 2 * Math.PI;
                    Vel = new Vector2((float)Math.Cos(rand), (float)Math.Sin(rand));
                    Vel *= USpeed;
                }
                else
                {
                    NextAction = gameTime.TotalGameTime + new TimeSpan(0, 0, 0, 0, milliseconds: (int)(Helper.Random() * 2000 + 1000));
                    wait = true;
                    Vel = Vector2.Zero;
                }
            }

            Pos += Vel * (float)(gameTime.ElapsedGameTime.TotalMilliseconds / 1000);

            Vector2 v = new Vector2(Vel.X, Vel.Y);
            v.Normalize();
            double angle = Math.Atan2(v.X, - v.Y);
            if (angle < -(3f / 4f) * Math.PI || angle > (3f / 4f) * Math.PI)
                Controller.SetAnimation("DOWN");
            if (angle >= (-3f / 4f) * Math.PI && angle < (-1f / 4f) * Math.PI)
                Controller.SetAnimation("LEFT");
            if (angle >= (-1f / 4f) * Math.PI && angle < (1f / 4f) * Math.PI)
                Controller.SetAnimation("UP");
            if (angle >= (1f / 4f) * Math.PI && angle < (3f / 4f) * Math.PI)
                Controller.SetAnimation("RIGHT");
            if (Vel == Vector2.Zero)
                Controller.SetAnimation("IDLE");

            base.Update(gameTime);
        }
    }
}
