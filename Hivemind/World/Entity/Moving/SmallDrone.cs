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
    public class SmallDrone : MovingEntity, IControllable
    {
        public const string UType = "SmallDrone";
        public readonly Point USize = new Point(48);
        public const int USpeed = 100;
        public static Texture2D UIcon;

        public override string Type => UType;
        public override Point Size => USize;

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
            Vector2 CheckedVel = Vel * (float)(gameTime.ElapsedGameTime.TotalMilliseconds / 1000);

            CheckedVel = Collision.CheckWorld(CheckedVel, GetBounds());

            Pos += CheckedVel;

            Vector2 v = new Vector2(CheckedVel.X, CheckedVel.Y);
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

        public void ControllerMove(Vector2 vel)
        {
            Vel = vel;
            Vel *= USpeed;

            Parent.Cam.Pos = Pos;
            Parent.Cam.ApplyTransform();
        }
    }
}
