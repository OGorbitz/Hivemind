using Hivemind.World;
using Hivemind.World.Entity;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.Input
{
    public interface Selectable
    {
        public abstract void DrawSelected(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, GameTime gameTime);
        public abstract void Command(Vector2 position);
    }

    public class Selection
    {
        public static List<Selectable> Selected = new List<Selectable>();
        public static int SelectionIndex = 0;

        public static void Select(Selectable selection)
        {
            Selected.Clear();
            Selected.Add(selection);
        }

        public static void Select(List<Selectable> selection)
        {
            Selected.Clear();
            Selected.AddRange(selection);
        }

        public static void AddSelect(Selectable selection)
        {
            Selected.Add(selection);
        }

        public static void AddSelect(List<Selectable> selection)
        {
            Selected.AddRange(selection);
        }

        public static void DrawSelectionRectangles(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            foreach(Selectable s in Selected)
            {
                s.DrawSelected(spriteBatch, graphicsDevice, gameTime);
            }
        }
    }
}
