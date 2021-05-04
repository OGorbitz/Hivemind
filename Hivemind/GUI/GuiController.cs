using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FontStashSharp;
using Hivemind.Input;
using Hivemind.Utility;
using Hivemind.World;
using Hivemind.World.Entity.Moving;
using Hivemind.World.Tiles;
using Hivemind.World.Tiles.Floor;
using Hivemind.World.Tiles.Wall;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;

namespace Hivemind.GUI
{
    internal enum GUIState
    {
        MAIN_MENU,
        MAIN_MENU_OPTIONS,
        HUD_TILEMAP,
        HUD_RESEARCH,
        MAIN_MENU_CREDITS
    }

    internal enum Symbols
    {
        POWER,
        POWER_LOW,
        PROCESSING_POWER,
        X_ICON,
    }

    internal class GuiController
    {
        private static RenderTarget2D _renderTarget2D;

        public static Texture2D[] Symbols;

        public static FontSystem Autobus;
        public static SpriteFontBase AutobusSmallest => Autobus.GetFont(12);
        public static SpriteFontBase AutobusSmaller => Autobus.GetFont(20);
        public static SpriteFontBase AutobusSmall => Autobus.GetFont(36);
        public static SpriteFontBase AutobusMedium => Autobus.GetFont(48);
        public static SpriteFontBase AutobusLarge => Autobus.GetFont(128);

        public static Texture2D ShapeSingle, ShapeLine, ShapeRectangle;

        public static int TooltipTime = 1000;

        private static Desktop _desktop;

        private static Panel _mainMenu, _mainMenuCredits;
        private static Panel _tilemapHud;

        public static Minimap Minimap;

        public static Panel infoPanel;
        public static VerticalStackPanel HardwareSelectionPanel;
        public static ImageButton SelectedShape;

        private static ConsoleText _creditText, _menuBackground;
        public static Label Credits, MenuBackground;
        public static string DebugText;

        public static long TotalFrames { get; private set; }
        public static float TotalSeconds { get; private set; }
        public static float AverageFramesPerSecond { get; private set; }
        public static float CurrentFramesPerSecond { get; private set; }
        public const int MAXIMUM_SAMPLES = 100;
        private static Queue<float> _sampleBuffer = new Queue<float>();

        public static GUITab Buildables;

        private static GUIState CurrentState;

        public static void Init(GraphicsDevice graphicsDevice, ContentManager content)
        {
            _desktop = new Desktop();

            Autobus = new FontSystem(StbTrueTypeSharpFontLoader.Instance, graphicsDevice, strokeAmount: 1);
            Autobus.AddFont(File.ReadAllBytes(@"Content\Fonts\Autobus.ttf"));

            ShapeSingle = content.Load<Texture2D>("GUI/ShapeSingle");
            ShapeLine = content.Load<Texture2D>("GUI/ShapeLine");
            ShapeRectangle = content.Load<Texture2D>("GUI/ShapeRectangle");

            InitMainMenu();
            Minimap = new Minimap(new Point(200), graphicsDevice);
            InitHUD_Tilemap();
        }

        public static void SetState(GUIState state)
        {
            CurrentState = state;

            switch (state)
            {
                case GUIState.MAIN_MENU:
                    _desktop.Root = _mainMenu;
                    _menuBackground = new ConsoleText(1000, true);
                    int row = 0;
                    int[] nlines = new int[4];
                    for(int i = 0; i < 100; i++)
                    {
                        string line;
                        switch (row)
                        {
                            case 0:
                                line = String.Format(@"\c[#555555]Checking sector {0:X} RNN modules for corruption:", (int)(Helper.Random() * int.MaxValue));
                                break;
                            case 1:
                                line = String.Format(@"\c[#555555]    Repairing block {0}:", (int)(Helper.Random() * 4096));
                                break;
                            case 2:
                                line = String.Format(@"\c[#555555]        Refactoring RNN node {0:X} weight and bias values:", (int)(Helper.Random() * 4096));
                                break;
                            case 3:
                                string working = @"\c[#225500]Nominal";
                                float r = Helper.Random();
                                if (r > 0.9f)
                                    working = @"\c[#3A0000]Fault Detected - Weight value higher than expected";
                                if (r > 0.95f)
                                    working = @"\c[#3A0000]Fault Detected - Invalid bias value";
                                line = String.Format(@"\c[#555555]            {0:X}: " + working, (int)(Helper.Random() * 4096));
                                break;
                            default:
                                line = "\n";
                                break;
                        }
                        _menuBackground.AddLine(line + "\n", 50, false);

                        nlines[row]--;
                        if (nlines[row] > 0) 
                        { 
                            if(row < 3)
                            {
                                row++;
                                nlines[row] = (int)(Helper.Random() * 2 * row + 2);
                            }
                        }
                        else
                        {
                            if (row > 0)
                            {
                                row--;
                            }
                            else
                            {
                                row++;
                                nlines[row] = (int)(Helper.Random() * 2 * row + 2);
                            }
                        }
                    }


                    //MusicController.PlayMusic(Music.AWAKENING);
                    break;
                case GUIState.MAIN_MENU_CREDITS:
                    _creditText = new ConsoleText(1000, false);
                    _creditText.AddLine("~$ cd /home", 350, false);
                    _creditText.AddLine("/home$ cd admin", 250, false);
                    _creditText.AddLine("/home/admin$ cd hivemind", 350, false);
                    _creditText.AddLine("/home/admin/hivemind$ cd docs", 150, false);
                    _creditText.AddLine("/home/admin/hivemind/docs$ echo Credits.txt", 500, false);
                    _creditText.AddLine("", 0, false);
                    _creditText.AddLine("Created by:", 0, false);
                    _creditText.AddLine("Ozzie Gorbitz", 2000, false);
                    _creditText.AddLine("", 0, false);
                    _creditText.AddLine("This game is the product of countless hours of labor.", 2000, false);
                    _creditText.AddLine("I hope you enjoy playing it. Thank you for giving it your time!", 2000, false);
                    _creditText.AddLine("", 0, false);
                    _creditText.AddLine("Please be gentle with your reviews, and be sure to report any bugs!", 2000, false);
                    _creditText.AddLine("", 0, false);
                    _creditText.AddLine("Press Escape to exit", 0, true);

                    _desktop.Root = _mainMenuCredits;
                    break;
                case GUIState.HUD_TILEMAP:
                    _desktop.Root = _tilemapHud;
                    break;
                case GUIState.HUD_RESEARCH:
                    //UserInterface.Active = HUD_Research;
                    break;
            }
        }

        public static GUIState GetState()
        {
            return CurrentState;
        }

        public static bool IsMouseOverGUI()
        {
            return _desktop.IsMouseOverGUI;
        }

        public static void Update(GameTime gameTime)
        {
            switch (CurrentState)
            {
                case GUIState.MAIN_MENU:
                    MenuBackground.Text = _menuBackground.GetLines((int)(_mainMenu.ContainerBounds.Height / AutobusSmaller.FontSize));
                    break;
                case GUIState.MAIN_MENU_CREDITS:
                    Credits.Text = _creditText.GetLines(50);
                    break;
                case GUIState.HUD_TILEMAP:
                    var mouse = Mouse.GetState();
                    var wpos = WorldManager.GetActiveTileMap().Cam.Unproject(mouse.Position.ToVector2());
                    var tpos = TileMap.GetTileCoords(wpos);
                    var t = WorldManager.GetActiveTileMap().GetTile(tpos);
                    BaseTile tile = t.Wall;
                    string name;
                    if (tile == null)
                        tile = t.Floor;
                    if (tile == null)
                        name = "Null";
                    else
                    {
                        string visible = "(U)";
                        if (t.Visibility == Visibility.KNOWN)
                            visible = "(K)";
                        if (t.Visibility == Visibility.VISIBLE)
                            visible = "(V)";
                        name = visible + tile.Name + tile.Pos.ToString();
                    }

                    string Room = "";
                    if (t.Room != null)
                    {
                        float crushedrock = 0;
                        foreach (DroppedMaterial m in t.Room.Materials)
                        {
                            if (m.Type == Material.CrushedRock.Name)
                                crushedrock += m.Amount;
                        }
                        Room = " Room Size: " + t.Room.Size + " Containing " + crushedrock + " Crushed Rock";
                    }

                    DebugText = "Camera position: (" + WorldManager.GetActiveTileMap().Cam.Pos.X + ", " + WorldManager.GetActiveTileMap().Cam.Pos.Y + ")\n" +
                        "Camera scale: " + WorldManager.GetActiveTileMap().Cam.Scale + "\n" +
                        "Pointed Block: " + name + Room + "\n" +
                        "BufferPos: " + WorldManager.GetActiveTileMap().BufferPosition.ToString() + " BufferOffset: " + WorldManager.GetActiveTileMap().BufferOffset.ToString();
                    break;
                case GUIState.HUD_RESEARCH:
                    break;
            }

            CurrentFramesPerSecond = 1000f / gameTime.ElapsedGameTime.Milliseconds;

            _sampleBuffer.Enqueue(CurrentFramesPerSecond);

            if (_sampleBuffer.Count > MAXIMUM_SAMPLES)
            {
                _sampleBuffer.Dequeue();
                AverageFramesPerSecond = _sampleBuffer.Average(i => i);
            }
            else
            {
                AverageFramesPerSecond = CurrentFramesPerSecond;
            }

            TotalFrames++;
            TotalSeconds += gameTime.ElapsedGameTime.Milliseconds * 1000f;
        }

        public static void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            if(CurrentState == GUIState.MAIN_MENU || CurrentState == GUIState.MAIN_MENU_CREDITS)
            {
                graphicsDevice.Clear(new Color(0f, 0.09f, 0f));
                var ms = Hivemind.CurrentGameTime.TotalGameTime.Seconds * 1000 + Hivemind.CurrentGameTime.TotalGameTime.Milliseconds;
                var n = (int)(ms % 3000 / 3000f * 96f);
                spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                for (var x = -n; x <= graphicsDevice.Viewport.Height; x += Hivemind.ComputerLines.Height * 5)
                    spriteBatch.Draw(Hivemind.ComputerLines,
                        new Rectangle(new Point(0, x),
                            new Point(graphicsDevice.Viewport.Width, Hivemind.ComputerLines.Height * 5)),
                        new Color(1f, 1f, 1f, 0.3f));
                n = (int)(ms % 2000 / 2000f * 64f);
                for (var x = n - 64; x <= graphicsDevice.Viewport.Height; x += Hivemind.ComputerLines.Height * 2)
                    spriteBatch.Draw(Hivemind.ComputerLines,
                        new Rectangle(new Point(0, x),
                            new Point(graphicsDevice.Viewport.Width, Hivemind.ComputerLines.Height * 3)),
                        new Color(1f, 1f, 1f, 0.25f));
                spriteBatch.End();
            }

            _desktop.Render();

            if(CurrentState == GUIState.HUD_TILEMAP)
            {
                spriteBatch.Begin();
                AutobusSmaller.DrawText(spriteBatch, DebugText, new Vector2(25), Color.White);
                AutobusSmaller.DrawText(spriteBatch, "" + AverageFramesPerSecond + " FPS", new Vector2(Hivemind.ScreenWidth - 25 - AutobusSmaller.MeasureString("" + AverageFramesPerSecond + " FPS", Vector2.One).X, 25), Color.White);
                if(Hivemind.DebugMode)
                    AutobusSmall.DrawText(spriteBatch, "DEBUG", new Vector2(Hivemind.ScreenWidth/2 - AutobusSmall.MeasureString("DEBUG", Vector2.One).X / 2, 25), Color.Red);
                spriteBatch.End();
            }
        }

        //Different GUI inits

        public static void InitMainMenu()
        {
            _mainMenu = new Panel();

            MenuBackground = new Label()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Font = AutobusSmaller,
                Padding = new Thickness(25, 0)
            };;
            _mainMenu.AddChild<Label>(MenuBackground);

            var menuGrid = new Grid
            {
                RowSpacing = 8,
                ColumnSpacing = 8
            };
            _mainMenu.AddChild<Grid>(menuGrid);

            menuGrid.RowsProportions.Add(new Proportion(ProportionType.Part));
            menuGrid.RowsProportions.Add(new Proportion(ProportionType.Pixels) { Value = 500 });
            menuGrid.RowsProportions.Add(new Proportion(ProportionType.Part));

            var title = new Label
            {
                Id = "label",
                Text = "HIVEMIND",
                GridColumn = 0,
                GridRow = 0,
                Font = AutobusLarge,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            menuGrid.Widgets.Add(title);


            var verticalMenu = new VerticalMenu()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                LabelFont = AutobusMedium,
                GridColumn = 0,
                GridRow = 1,
                Border = new SolidBrush(Color.Transparent),
                Background = new SolidBrush(Color.Transparent),
                LabelHorizontalAlignment = HorizontalAlignment.Center,
                SelectionHoverBackground = new SolidBrush(new Color(0.2f, 0.2f, 0.2f, 0.15f)),
                SelectionBackground = new SolidBrush(new Color(0.1f, 0.1f, 0.1f, 0.15f)),
            };
            var menuItem = new MenuItem()
            {
                Id = "playGame",
                Text = "Play Game",
            };
            menuItem.Selected += (s, a) =>
            {
                WorldManager.SetActiveTileMap(new TileMap(320));
                GameStateManager.SetState(GameState.TILEMAP);
            };
            verticalMenu.Items.Add(menuItem);
            menuItem = new MenuItem()
            {
                Id = "options",
                Text = "Options"
            };
            verticalMenu.Items.Add(menuItem);
            menuItem = new MenuItem()
            {
                Id = "credits",
                Text = "Credits"
            };
            menuItem.Selected += (s, a) =>
            {
                GuiController.SetState(GUIState.MAIN_MENU_CREDITS);
            };
            verticalMenu.Items.Add(menuItem);

            menuGrid.Widgets.Add(verticalMenu);


            _mainMenuCredits = new Panel();
            Credits = new Label()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Font = AutobusSmall,
                Padding = new Thickness(50)
            };
            _mainMenuCredits.Widgets.Add(Credits);
        }

        private static void InitHUD_Tilemap()
        {
            _tilemapHud = new Panel();

            var minimap = new Image()
            {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Right,
                Width = 200,
                Height = 200,
                Border = new SolidBrush(new Color(0.5f, 0.5f, 0.5f, 1)),
                BorderThickness = new Thickness(4),
            };
            minimap.BeforeRender += (e) => {
                minimap.Background = new TextureRegion(Minimap.RenderedMap);
                Minimap.Redraw();
            };

            _tilemapHud.AddChild<Image>(minimap);

            var buttonPanel = new Panel()
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                Background = new SolidBrush(new Color(0.2f, 0.2f, 0.2f)),
                Border = new SolidBrush(new Color(0.05f, 0.05f, 0.05f, 1)),
                BorderThickness = new Thickness(0, 2, 0, 0),
            };

            _tilemapHud.AddChild<Panel>(buttonPanel);

            var buttonGrid = new Grid();
            buttonGrid.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, 50));
            buttonGrid.ColumnsProportions.Add(new Proportion());
            buttonGrid.ColumnsProportions.Add(new Proportion());
            buttonGrid.ColumnsProportions.Add(new Proportion());


            buttonPanel.AddChild<Grid>(buttonGrid);

            Color buttonBorderColor = new Color(0.15f, 0.15f, 0.15f, 1);

            var Hardware = new TextButton()
            {
                Text = "Hardware",
                Font = AutobusSmall,
                Padding = new Thickness(10),
                PaddingBottom = 5,
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Left,
                Border = new SolidBrush(buttonBorderColor),
                BorderThickness = new Thickness(2, 0),
                GridColumn = 1
            };
            buttonGrid.AddChild<TextButton>(Hardware);

            var Tasks = new TextButton()
            {
                Text = "Tasks",
                Font = AutobusSmall,
                Padding = new Thickness(10),
                PaddingBottom = 5,
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Left,
                Border = new SolidBrush(buttonBorderColor),
                BorderThickness = new Thickness(0, 0, 2, 0),
                GridColumn = 2
            };

            Tasks.Click += (s, a) =>
            {

            };

            buttonGrid.AddChild<TextButton>(Tasks);


            HardwareSelectionPanel = new VerticalStackPanel
            {
                Padding = new Thickness(10),
                VerticalAlignment = VerticalAlignment.Bottom,
                Background = new SolidBrush(new Color(0.15f, 0.15f, 0.15f, 1)),
                Border = new SolidBrush(new Color(0.05f, 0.05f, 0.05f, 1)),
                BorderThickness = new Thickness(2),
                Left = 25,
                Visible = false
            };
            HardwareSelectionPanel.BeforeRender += (s) =>
            {
                HardwareSelectionPanel.Top = -buttonPanel.Bounds.Height + HardwareSelectionPanel.BorderThickness.Height;
            };
            _tilemapHud.AddChild<VerticalStackPanel>(HardwareSelectionPanel);
            Buildables = new GUITab(3, HardwareSelectionPanel, new Rectangle(0, 0, Wall_Cinderblock.UIcon.Height + 8, Wall_Cinderblock.UIcon.Height + 8), 10);

            Buildables.AddButton(typeof(Wall_Cinderblock), Wall_Cinderblock.UIcon);
            Buildables.AddButton(typeof(Wall_Dirt), Wall_Dirt.UIcon);
            Buildables.AddButton(typeof(Floor_Concrete), Floor_Concrete.UIcon);
            Buildables.AddButton(typeof(Floor_Dirt), Floor_Dirt.UIcon);
            Buildables.AddButton(typeof(Floor_Grass), Floor_Grass.UIcon);
            Buildables.AddButton(typeof(Wall_Cinderblock), Wall_Cinderblock.UIcon);
            Buildables.AddButton(typeof(Wall_Cinderblock), Wall_Cinderblock.UIcon);
            Buildables.AddButton(typeof(Wall_Cinderblock), Wall_Cinderblock.UIcon);
            Buildables.AddButton(typeof(Wall_Cinderblock), Wall_Cinderblock.UIcon);
            Buildables.AddButton(typeof(Wall_Cinderblock), Wall_Cinderblock.UIcon);
            Buildables.AddButton(typeof(Wall_Cinderblock), Wall_Cinderblock.UIcon);
            Buildables.AddButton(typeof(Wall_Cinderblock), Wall_Cinderblock.UIcon);


            SelectedShape = new ImageButton()
            {
                Image = new TextureRegion(ShapeLine, new Rectangle(0, 0, ShapeLine.Width, ShapeLine.Height)),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Background = new SolidBrush(Color.Transparent),
                FocusedBackground = new SolidBrush(Color.Transparent),
                DisabledBackground = new SolidBrush(Color.Transparent),
                PressedBackground = new SolidBrush(Color.Transparent),
                OverBackground = new SolidBrush(Color.Transparent),
                Left = -15,
                Visible = false
            };
            SelectedShape.BeforeRender += (s) =>
            {
                SelectedShape.Top = -buttonGrid.Bounds.Height - 15;
            };

            _tilemapHud.AddChild<ImageButton>(SelectedShape);

            var shapeGrid = new Grid()
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                Height = ShapeLine.Height * 3,
                Left = -15
            };
            shapeGrid.RowsProportions.Add(new Proportion());
            shapeGrid.RowsProportions.Add(new Proportion());
            shapeGrid.RowsProportions.Add(new Proportion());
            _tilemapHud.AddChild<Grid>(shapeGrid);

            shapeGrid.BeforeRender += (s) =>
            {
                Rectangle r = new Rectangle(shapeGrid.Bounds.Left - 50, shapeGrid.Bounds.Top - 50, shapeGrid.Bounds.Width + 50, shapeGrid.Bounds.Height + 200);
                if (!r.Contains(_desktop.MousePosition) || !SelectedShape.Visible)
                    shapeGrid.Visible = false;
            };

            var shape = new ImageButton()
            {
                Image = new TextureRegion(ShapeSingle, new Rectangle(0, 0, ShapeLine.Width, ShapeLine.Height)),
                Background = new SolidBrush(Color.Transparent),
                FocusedBackground = new SolidBrush(Color.Transparent),
                DisabledBackground = new SolidBrush(Color.Transparent),
                PressedBackground = new SolidBrush(Color.Transparent),
                OverBackground = new SolidBrush(Color.Transparent),
                HorizontalAlignment = HorizontalAlignment.Right,
                GridRow = 0
            };
            shape.Click += (s, a) =>
            {
                SelectedShape.Image = ((ImageButton)s).Image;
                Editing.Shape = EditShape.SINGLE;
            };
            shapeGrid.AddChild<ImageButton>(shape);

            shape = new ImageButton()
            {
                Image = new TextureRegion(ShapeLine, new Rectangle(0, 0, ShapeLine.Width, ShapeLine.Height)),
                Background = new SolidBrush(Color.Transparent),
                FocusedBackground = new SolidBrush(Color.Transparent),
                DisabledBackground = new SolidBrush(Color.Transparent),
                PressedBackground = new SolidBrush(Color.Transparent),
                OverBackground = new SolidBrush(Color.Transparent),
                HorizontalAlignment = HorizontalAlignment.Right,
                GridRow = 1
            };
            shape.Click += (s, a) =>
            {
                SelectedShape.Image = ((ImageButton)s).Image;
                Editing.Shape = EditShape.LINE;
            };
            shapeGrid.AddChild<ImageButton>(shape);

            shape = new ImageButton()
            {
                Image = new TextureRegion(ShapeRectangle, new Rectangle(0, 0, ShapeLine.Width, ShapeLine.Height)),
                Background = new SolidBrush(Color.Transparent),
                FocusedBackground = new SolidBrush(Color.Transparent),
                DisabledBackground = new SolidBrush(Color.Transparent),
                PressedBackground = new SolidBrush(Color.Transparent),
                OverBackground = new SolidBrush(Color.Transparent),
                HorizontalAlignment = HorizontalAlignment.Right,
                GridRow = 2
            };
            shape.Click += (s, a) =>
            {
                SelectedShape.Image = ((ImageButton)s).Image;
                Editing.Shape = EditShape.RECTANGLE;
            };
            shapeGrid.AddChild<ImageButton>(shape);




            SelectedShape.Click += (s, a) =>
            {
                shapeGrid.Visible = !shapeGrid.Visible;
                shapeGrid.Top = SelectedShape.Bounds.Top - shapeGrid.Bounds.Height;
            };

            Hardware.Click += (s, a) =>
            {
                HardwareSelectionPanel.Visible = !HardwareSelectionPanel.Visible;
                if (HardwareSelectionPanel.Visible)
                {
                    GameInput.CurrentAction = Input.Action.BUILD;
                    infoPanel.Visible = false;
                    SelectedShape.Visible = true;
                }
                else
                {
                    GameInput.CurrentAction = Input.Action.SELECT;
                    SelectedShape.Visible = false;
                }
            };

            infoPanel = new Panel()
            {
                Width = 250,
                Background = new SolidBrush(new Color(0.15f, 0.15f, 0.15f, 0.5f)),
                Border = new SolidBrush(new Color(0.05f, 0.05f, 0.05f, 0.5f)),
                BorderThickness = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Left,
                Left = 25,
                Padding = new Thickness(25),
                Visible = false
            };
            infoPanel.BeforeRender += (s) =>
            {
                infoPanel.Top = -buttonPanel.Bounds.Height + infoPanel.BorderThickness.Height;
            };
            _tilemapHud.AddChild<Panel>(infoPanel);


            buttonPanel.BringToFront();
        }
    }
}