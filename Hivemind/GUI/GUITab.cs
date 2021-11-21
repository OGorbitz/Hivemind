using Hivemind.Input;
using Hivemind.World.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using System;
using System.Collections.Generic;
using System.Text;
using static Hivemind.World.Tiles.TileConstructor;

namespace Hivemind.GUI
{
    class GUITab
    {
        private int _width;
        private Rectangle _buttonSize;
        private List<Widget> _items;
        private VerticalStackPanel _panel;

        public int Width
        {
            get
            {
                return _width;
            }
            set
            {
                _width = value;
                Reorder();
            }
        }

        public GUITab(int width, VerticalStackPanel panel, Rectangle buttonSize, int spacing)
        {
            _width = width;
            _buttonSize = buttonSize;
            _panel = panel;
            _items = new List<Widget>();
            _panel.Width = spacing + (buttonSize.Width + spacing) * width;
            _panel.Spacing = spacing;
            Reorder();
        }

        public void AddItem(Widget widget)
        {
            _items.Add(widget);
            Reorder();
        }

        public void AddButton(PlacingType placingType, string tileName, Texture2D icon)
        {
            var i = new ImageButton()
            {
                Image = new TextureRegion(icon, new Rectangle(0, 0, icon.Width, icon.Height)),
                Width = _buttonSize.Width,
                Height = _buttonSize.Height,
                DisabledBackground = new SolidBrush(new Color(0.3f, 0.6f, 0, 1f))
            };
            i.Click += (s, e) =>
            {
                Editing.SetType(placingType, tileName);
            };
            i.BeforeRender += (s) =>
            {
                if (Editing.SelectedTileName == tileName)
                    i.Enabled = false;
                else
                    i.Enabled = true;
            };
            AddItem(i);
        }

        private void Reorder()
        {
            _panel.Widgets.Clear();
         
            for(int i = 0; i < _items.Count / Width; i++)
            {
                var stack = new HorizontalStackPanel
                {
                    Spacing = _panel.Spacing
                };
                _panel.AddChild<HorizontalStackPanel>(stack);

                for (int j = 0; j < _width; j++)
                {
                    stack.Proportions.Add(new Proportion());

                    int n = i * _width + j;
                    if (n < _items.Count)
                    {
                        stack.AddChild<Widget>(_items[n]);
                    }
                }
            }

            if(_panel.Widgets.Count > 0)
            {
                _panel.Widgets[0].BeforeRender += (s) =>
                {
                    _panel.Width = _panel.Widgets[0].Bounds.Width + _panel.Padding.Width + _panel.BorderThickness.Width;
                };
            }
        }

    }
}
