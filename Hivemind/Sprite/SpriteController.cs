using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.Sprite
{
    public class SpriteController
    {
        private readonly SortedDictionary<string, Animation> Animations;
        private string CurrentAnimation = "DEFAULT";

        public SpriteController()
        {
            Animations = new SortedDictionary<string, Animation>();
            AddAnimation("DEFAULT", new[] { 0 }, 1, false);
        }
        
        /// <summary>
        /// Adds an animation to the controller
        /// </summary>
        /// <param name="name">Animation name</param>
        /// <param name="frames">Array containing index numbers to loop through</param>
        /// <param name="speed">Speed, in animation frames per second</param>
        /// <param name="loops">Boolean value specifying if the animation should loop</param>
        public void AddAnimation(string name, int[] frames, float speed, bool loops)
        {
            if (!Animations.ContainsKey(name)) Animations.Add(name, new Animation(name, frames, speed, loops));
        }

        public void SetAnimation(string name)
        {
            if (Animations.ContainsKey(name) && CurrentAnimation != name)
            {
                CurrentAnimation = name;
                Animations[CurrentAnimation].DirtyTime = true;
            }
        }

        public int GetFrame(GameTime gameTime)
        {
            var a = Animations[CurrentAnimation];

            return a.GetFrame(gameTime);
        }
    }
}
