using System;
using Hivemind.Input;
using Hivemind.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

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

        public TileMap Parent;

        //Settings
        float ScrollSpeed = 0.002f;
        float Sponginess = 0.4f;


        public Camera(TileMap tileMap)
        {
            Parent = tileMap;
        }

        public void ApplyTransform()
        {
            var offset = Matrix.CreateTranslation(Hivemind.ScreenWidth / 2, Hivemind.ScreenHeight / 2, 0);
            var negoffset = Matrix.CreateTranslation(-Hivemind.ScreenWidth, -Hivemind.ScreenHeight, 0);
            var scalematrix = Matrix.CreateScale(Scale);
            var translate =
                Matrix.CreateTranslation(-Pos.X + Hivemind.ScreenWidth, -Pos.Y + Hivemind.ScreenHeight, 0);
            Translate = translate;
            ScaleOffset = negoffset * scalematrix * offset;
            TranslateScaleOffset = translate * negoffset * scalematrix * offset;
        }

        public void UpdateScale()
        {
            MouseState ms = Mouse.GetState();
            Vector2 mousePos = new Vector2(ms.X, ms.Y);
            Vector2 oldPointer = Unproject(mousePos);

            Scale += ScrollSpeed * GameInput.ScrollWheelChange();

            if (Scale < 0.5f)
                Scale = 0.5f;
            if (Scale > 3.0f)
                Scale = 3.0f;

            ApplyTransform();

            Vector2 newPointer = Unproject(mousePos);
            Pos += oldPointer - newPointer;

            ApplyTransform();
        }

        public void Move(Vector2 vel)
        {
            var speed = 16f / (float)Math.Sqrt(Scale);

            Pos += vel * speed;

            Rectangle r = GetScaledBounds();
            if (r.Left < 0)
                Pos.X = Hivemind.ScreenWidth / Scale / 2;
            else if (r.Left < TileManager.TileSize * 2)
                Pos.X += Sponginess * ((TileManager.TileSize * 2 - r.Left) / 2);
            if (r.Top < 0)
                Pos.Y = Hivemind.ScreenHeight / Scale / 2;
            else if (r.Top < TileManager.TileSize * 2)
                Pos.Y += Sponginess * ((TileManager.TileSize * 2 - r.Top) / 2);
            if (r.Right > Parent.Size * TileManager.TileSize)
                Pos.X = Parent.Size * TileManager.TileSize - Hivemind.ScreenWidth / Scale;
            else if (r.Right > Parent.Size * TileManager.TileSize - TileManager.TileSize * 2)
                Pos.X -= Sponginess * ((r.Right - (Parent.Size * TileManager.TileSize - TileManager.TileSize * 2)) / 2);
            if (r.Bottom > Parent.Size * TileManager.TileSize)
                Pos.Y = Parent.Size * TileManager.TileSize - Hivemind.ScreenHeight / Scale;
            else if(r.Bottom > Parent.Size * TileManager.TileSize - TileManager.TileSize * 2)
                Pos.Y -= Sponginess * ((r.Bottom - (Parent.Size * TileManager.TileSize - TileManager.TileSize * 2)) / 2);

            ApplyTransform();
        }

        public Vector2 Unproject(Vector2 v)
        {
            return Vector2.Transform(v, Matrix.Invert(TranslateScaleOffset));
        }

        public Rectangle GetBounds()
        {
            return new Rectangle((int)(Pos.X - Hivemind.ScreenWidth), (int)(Pos.Y - Hivemind.ScreenHeight), Hivemind.ScreenWidth * 2, Hivemind.ScreenHeight * 2);
        }

        public Rectangle GetScaledBounds()
        {
            return new Rectangle((int)(Pos.X - Hivemind.ScreenWidth / Scale / 2), (int)(Pos.Y - Hivemind.ScreenHeight / Scale / 2), (int)(Hivemind.ScreenWidth / Scale), (int)(Hivemind.ScreenHeight / Scale));
        }
    }
}