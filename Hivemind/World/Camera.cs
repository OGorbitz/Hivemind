using System;
using Hivemind.Input;
using Microsoft.Xna.Framework;

namespace Hivemind
{
    public class Camera
    {
        public Vector2 Pos = new Vector2(0);
        public float Scale = 1.0f;

        public bool uplvl, downlvl;

        public Matrix Translate { get; private set; }
        public Matrix ScaleOffset { get; private set; }
        public Matrix TranslateScaleOffset { get; private set; }

        public void ApplyTransform()
        {
            var offset = Matrix.CreateTranslation(GraphicsDeviceManager.DefaultBackBufferWidth / 2, GraphicsDeviceManager.DefaultBackBufferHeight / 2, 0);
            var negoffset = Matrix.CreateTranslation(-GraphicsDeviceManager.DefaultBackBufferWidth, -GraphicsDeviceManager.DefaultBackBufferHeight, 0);
            var scalematrix = Matrix.CreateScale(Scale);
            var translate =
                Matrix.CreateTranslation(-Pos.X + GraphicsDeviceManager.DefaultBackBufferWidth, -Pos.Y + GraphicsDeviceManager.DefaultBackBufferHeight, 0);
            Translate = translate;
            ScaleOffset = negoffset * scalematrix * offset;
            TranslateScaleOffset = translate * negoffset * scalematrix * offset;
        }

        public void UpdateScale()
        {
            var scrollspeed = 0.002f;

            Scale += scrollspeed * GameInput.ScrollWheelChange();

            if (Scale < 0.5f)
                Scale = 0.5f;
            if (Scale > 3.0f)
                Scale = 3.0f;
            ApplyTransform();
        }

        public void Move(Vector2 vel)
        {
            var speed = 16f / (float)Math.Sqrt(Scale);

            Pos += vel * speed;

            ApplyTransform();
        }

        public Vector2 Unproject(Vector2 v)
        {
            return Vector2.Transform(v, Matrix.Invert(TranslateScaleOffset));
        }
    }
}