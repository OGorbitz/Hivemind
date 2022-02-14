using Microsoft.Xna.Framework;
using System;

namespace Hivemind.Sprite
{
    class Animation
    {
        public int CurrentFrame;
        public int[] Frames;
        public TimeSpan LastKeyFrame;
        public bool Loops, DirtyTime;
        public string Name;
        public float Speed;

        private bool _finished = false;

        public Animation(string name, int[] frames, float speed, bool loops)
        {
            Name = name;
            Frames = frames;
            Speed = speed;
            Loops = loops;
            CurrentFrame = 0;
            LastKeyFrame = new TimeSpan();
        }

        public void Restart()
        {
            DirtyTime = true;
            CurrentFrame = 0;
            _finished = false;
        }

        public int GetFrame(GameTime gameTime)
        {
            if (DirtyTime)
            {
                LastKeyFrame = gameTime.TotalGameTime;
                DirtyTime = false;
            }

            if ((gameTime.TotalGameTime - LastKeyFrame).TotalMilliseconds > 1000 / Speed * Frames.Length)
                LastKeyFrame = gameTime.TotalGameTime;

            if ((gameTime.TotalGameTime - LastKeyFrame).TotalMilliseconds > 1000 / Speed)
            {
                LastKeyFrame += new TimeSpan(0, 0, 0, 0, (int)(1000 / Speed));

                CurrentFrame++;
                if (CurrentFrame >= Frames.Length)
                {
                    if (Loops)
                        CurrentFrame = 0;
                    else
                    {
                        CurrentFrame--;
                        _finished = true;
                    }
                }
            }

            return Frames[CurrentFrame];
        }

        public bool Finished
        {
            get
            {
                return _finished;
            }
        }
    }
}
