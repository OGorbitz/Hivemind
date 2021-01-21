using Hivemind.World;
using Hivemind.World.Entity;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.Input
{
    public interface ISelectable
    {
        public void DrawSelected(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, GameTime gameTime);
        public void Command(Vector2 position);
        public bool SetFocused(bool focused);
    }

    public class Selection
    {
        public static List<ISelectable> Selected = new List<ISelectable>();
        public static int SelectionIndex = 0;

        public static bool UpdateCam()
        {
            if(Selected.Count == 1)
            {
                return Selected[0].SetFocused(true);
            }
            return false;
        }

        public static void Select(ISelectable selection)
        {
            Selected.Clear();
            Selected.Add(selection);
        }

        public static void Select(List<ISelectable> selection)
        {
            Selected.Clear();
            Selected.AddRange(selection);
        }

        public static void AddSelect(ISelectable selection)
        {
            Selected.Add(selection);
        }

        public static void AddSelect(List<ISelectable> selection)
        {
            Selected.AddRange(selection);
        }

        public static void DrawSelectionRectangles(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            foreach(ISelectable s in Selected)
            {
                s.DrawSelected(spriteBatch, graphicsDevice, gameTime);
            }
        }
    }
}
