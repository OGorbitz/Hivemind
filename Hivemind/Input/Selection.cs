using Hivemind.GUI;
using Hivemind.World;
using Hivemind.World.Entity;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D.UI;
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
        public void AddInfo(Panel panel);
    }

    public class Selection
    {
        public static List<ISelectable> Selected = new List<ISelectable>();

        public static int SelectionIndex = 0;
        public static bool ShouldFocus = false;

        public static int NumSelected => Selected.Count;

        public static bool UpdateCam()
        {
            if (ShouldFocus)
            {
                if (Selected.Count == 1)
                {
                    return Selected[0].SetFocused(true);
                }
            }
            return false;
        }

        public static void SelectFocus(ISelectable selection)
        {
            Selected.Clear();
            Selected.Add(selection);
            ShouldFocus = true;
            UpdateInfoPanel();
        }

        public static void Select(ISelectable selection)
        {
            Selected.Clear();
            Selected.Add(selection);
            ShouldFocus = false;
            UpdateInfoPanel();
        }

        public static void Select(List<ISelectable> selection)
        {
            Selected.Clear();
            Selected.AddRange(selection);
            ShouldFocus = false;
            UpdateInfoPanel();
        }

        public static void AddSelect(ISelectable selection)
        {
            Selected.Add(selection);
            ShouldFocus = false;
            UpdateInfoPanel();
        }

        public static void AddSelect(List<ISelectable> selection)
        {
            Selected.AddRange(selection);
            ShouldFocus = false;
            UpdateInfoPanel();
        }

        public static void ClearSelection()
        {
            Selected.Clear();
            ShouldFocus = false;
            UpdateInfoPanel();
        }

        public static void UpdateInfoPanel()
        {
            if (Selected.Count == 0)
            {
                GuiController.infoPanel.Visible = false;
            }
            else
            {
                GuiController.infoPanel.Visible = true;
                GuiController.infoPanel.Widgets.Clear();
                if(Selected.Count > 1)
                {
                    var info = new Label()
                    {
                        Text = Selected.Count + " units selected",
                        Font = GuiController.AutobusSmall,
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    GuiController.infoPanel.AddChild<Label>(info);
                    GuiController.infoPanel.Width = 300;
                    GuiController.infoPanel.Height = 100;
                }
                else
                {
                    ISelectable selected = Selected[0];
                    if (selected.GetType().IsSubclassOf(typeof(BaseEntity)))
                    {
                        BaseEntity entity = (BaseEntity)selected;

                        GuiController.infoPanel.Width = 450;
                        GuiController.infoPanel.Height = 350;
                        entity.AddInfo(GuiController.infoPanel);
                    }
                }
            }
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
