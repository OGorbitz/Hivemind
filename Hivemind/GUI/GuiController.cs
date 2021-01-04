using System;
using System.IO;
using Hivemind.Input;
using Hivemind.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using SpriteFontPlus;

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
        LENGTH
    }

    internal class GuiController
    {
        private static RenderTarget2D _renderTarget2D;

        public static Texture2D[] Symbols;
        public static SpriteFont KarnivorSmall, KarnivorMedium, KarnivorLarge;
        public static int TooltipTime = 1000;

        private static Desktop _desktop;

        private static Grid _mainMenu, _mainMenuCredits, _tilemapHud;

        private static ConsoleText _creditText;
        public static Label Credits, HUDText;


        private static GUIState CurrentState;

        public static void Init(GraphicsDevice graphicsDevice)
        {
            _desktop = new Desktop();

            KarnivorSmall = TtfFontBaker.Bake(File.ReadAllBytes(@"Content\Fonts\KARNIVOR.ttf"), 32, 1024, 1024,
                new[]
                {
                    CharacterRange.BasicLatin,
                    CharacterRange.Latin1Supplement,
                    CharacterRange.LatinExtendedA,
                    CharacterRange.Cyrillic
                }).CreateSpriteFont(graphicsDevice); 
            KarnivorMedium = TtfFontBaker.Bake(File.ReadAllBytes(@"Content\Fonts\KARNIVOR.ttf"), 64, 1024, 1024,
                new[]
                {
                    CharacterRange.BasicLatin,
                    CharacterRange.Latin1Supplement,
                    CharacterRange.LatinExtendedA,
                    CharacterRange.Cyrillic
                }).CreateSpriteFont(graphicsDevice);
            KarnivorLarge = TtfFontBaker.Bake(File.ReadAllBytes(@"Content\Fonts\KARNIVOR.ttf"), 128, 1024, 1024,
                new[]
                {
                                CharacterRange.BasicLatin,
                                CharacterRange.Latin1Supplement,
                                CharacterRange.LatinExtendedA,
                                CharacterRange.Cyrillic
                }).CreateSpriteFont(graphicsDevice);

            InitMainMenu();
            InitHUD_Tilemap();
            InitHUD_Research();
        }

        public static void SetState(GUIState state)
        {
            CurrentState = state;

            switch (state)
            {
                case GUIState.MAIN_MENU:
                    _desktop.Root = _mainMenu;
                    //MusicController.PlayMusic(Music.AWAKENING);
                    break;
                case GUIState.MAIN_MENU_CREDITS:
                    _creditText = new ConsoleText(1000, false, Hivemind.CurrentGameTime);
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
                    break;
                case GUIState.MAIN_MENU_CREDITS:
                    Credits.Text = _creditText.GetLines(50);
                    break;
                case GUIState.HUD_TILEMAP:
                    var mouse = Mouse.GetState();
                    var wpos = WorldManager.GetActiveTileMap().Cam.Unproject(mouse.Position.ToVector2());
                    var tpos = TileMap.GetTileCoords(wpos);
                    var tile = WorldManager.GetActiveTileMap().GetTile(tpos, Layer.WALL);
                    string name;
                    if (tile == null)
                        tile = WorldManager.GetActiveTileMap().GetFloor(tpos);
                    if (tile == null)
                        name = "Null";
                    else
                        name = tile.Name;
                    HUDText.Text = "Camera position: (" + WorldManager.GetActiveTileMap().Cam.Pos.X + ", " + WorldManager.GetActiveTileMap().Cam.Pos.Y + ")\n" +
                        "Camera scale: " + WorldManager.GetActiveTileMap().Cam.Scale + "\n" +
                        "Pointed Block: " + name;
                    break;
                case GUIState.HUD_RESEARCH:
                    break;
            }
        }

        public static void Render(GraphicsDevice graphicsDevice)
        {
            //UserInterface.Active.Draw(spriteBatch);
        }

        public static void Draw(GraphicsDevice graphicsDevice)
        {
            _desktop.Render();
        }

        //Different GUI inits

        public static void InitMainMenu()
        {
            _mainMenu = new Grid
            {
                ShowGridLines = true,
                RowSpacing = 8,
                ColumnSpacing = 8
            };

            _mainMenu.RowsProportions.Add(new Proportion(ProportionType.Part));
            _mainMenu.RowsProportions.Add(new Proportion(ProportionType.Pixels) { Value = 500 });
            _mainMenu.RowsProportions.Add(new Proportion(ProportionType.Part));


            var panel = new Panel
            {
                Id = "panel",
                Padding = new Thickness(10)
            };

            var title = new Label
            {
                Id = "label",
                Text = "HIVEMIND",
                GridColumn = 0,
                GridRow = 0,
                Font = KarnivorLarge,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _mainMenu.Widgets.Add(title);


            var verticalMenu = new VerticalMenu()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                LabelFont = KarnivorMedium,
                GridColumn = 0,
                GridRow = 1,
                Border = new SolidBrush(Color.Transparent),
                Background = new SolidBrush(Color.Transparent),
                LabelHorizontalAlignment = HorizontalAlignment.Center,
                SelectionHoverBackground = new SolidBrush(Color.ForestGreen),
                SelectionBackground = new SolidBrush(Color.DarkGreen)
            };
            var menuItem = new MenuItem()
            {
                Id = "playGame",
                Text = "Play Game"
            };
            menuItem.Selected += (s, a) =>
            {
                WorldManager.SetActiveTileMap(new TileMap(200));
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

            _mainMenu.Widgets.Add(verticalMenu);

            // ComboBox
            var combo = new ComboBox
            {
                GridColumn = 0,
                GridRow = 1
            };

            combo.Items.Add(new ListItem("Red", Color.Red));
            combo.Items.Add(new ListItem("Green", Color.Green));
            combo.Items.Add(new ListItem("Blue", Color.Blue));
            combo.HorizontalAlignment = HorizontalAlignment.Center;
            _mainMenu.Widgets.Add(combo);

            // Button
            var button = new TextButton
            {
                GridColumn = 0,
                GridRow = 1,
                Text = "Show"
            };

            button.Click += (s, a) =>
            {
                var messageBox = Dialog.CreateMessageBox("Message", "Some message!");
                messageBox.ShowModal(_desktop);
            };

            _mainMenu.Widgets.Add(button);

            // Spin button
            var spinButton = new SpinButton
            {
                GridColumn = 0,
                GridRow = 2,
                Width = 100,
                Nullable = true
            };
            _mainMenu.Widgets.Add(spinButton);


            _mainMenuCredits = new Grid();

            Credits = new Label()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Font = KarnivorSmall,
                Padding = new Thickness(50)
            };
            _mainMenuCredits.Widgets.Add(Credits);
        }

        private static void InitHUD_Tilemap()
        {
            _tilemapHud = new Grid()
            {
                ShowGridLines = true,
                RowSpacing = 8,
                ColumnSpacing = 8
            };

            HUDText = new Label()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Font = KarnivorSmall,
                Padding = new Thickness(25)
            };

            _tilemapHud.Widgets.Add(HUDText);

            /*var DebugPanel = new Panel(new Vector2(300, 100), PanelSkin.None, Anchor.TopLeft);
            var MouseCoords = new Paragraph("Pos:", Anchor.TopLeft);
            DebugPanel.Padding = Vector2.Zero;
            
            MouseCoords.BeforeDraw = (Entity e) =>
            {
                var p = (Paragraph) e;
                var v = GameInput.WorldPos;
                v = TileMap.GetTileCoords(v);
                p.Text = "Pos: " + v.X + ", " + v.Y;
            };
            DebugPanel.AddChild(MouseCoords);HUD_TileMap.AddEntity(DebugPanel);

            var ResourcePanel = new Panel(new Vector2(300, 100), PanelSkin.None, Anchor.TopRight);
            ResourcePanel.Padding = Vector2.Zero;


            var CPURow = new Panel(new Vector2(300, 40), PanelSkin.None, Anchor.TopRight);
            CPURow.Padding = Vector2.Zero;
            var CPUIcon = new Image(symbols[(int) Symbols.PROCESSING_POWER], new Vector2(32, 32),
                anchor: Anchor.CenterRight);
            CPUIcon.ToolTipText = "Total available processing power";
            CPURow.AddChild(CPUIcon);
            var CPU = new Paragraph("0", Anchor.CenterRight, offset: new Vector2(40, 0));
            CPU.BeforeDraw = entity =>
            {
                var h = (Paragraph) entity;
                h.Text = "" + World.World.GetActiveTileMap().GetProcessingPower();
            };
            CPU.FillColor = Color.White;
            CPU.OutlineColor = Color.Black;
            CPURow.AddChild(CPU);
            ResourcePanel.AddChild(CPURow);

            var PowerRow = new Panel(new Vector2(300, 40), PanelSkin.None, Anchor.TopRight, new Vector2(0, 40));
            PowerRow.Padding = Vector2.Zero;
            var PowerIcon = new Image(symbols[(int) Symbols.POWER], new Vector2(32, 32), anchor: Anchor.CenterRight);
            PowerIcon.BeforeDraw = entity =>
            {
                var i = (Image) entity;
                if (World.World.GetActiveTileMap().GetPower() < 0 &&
                    UserInterface.Active.CurrGameTime.TotalGameTime.Seconds % 2 > 0)
                    i.Texture = symbols[(int) Symbols.POWER_LOW];
                else
                    i.Texture = symbols[(int) Symbols.POWER];
            };
            PowerIcon.ToolTipText = "Total available energy";
            PowerRow.AddChild(PowerIcon);
            var Power = new Paragraph("0", Anchor.CenterRight, offset: new Vector2(40, 0));
            Power.BeforeDraw = entity =>
            {
                var h = (Paragraph) entity;
                h.Text = "" + World.World.GetActiveTileMap().GetPower();
            };
            Power.FillColor = Color.White;
            Power.OutlineColor = Color.Black;
            PowerRow.AddChild(Power);
            ResourcePanel.AddChild(PowerRow);

            HUD_TileMap.AddEntity(ResourcePanel);


            var BuildPanelHeight = 65;
            var BuildPanel = new Panel(new Vector2(Game1.ScreenWidth + 10, BuildPanelHeight + 4), PanelSkin.Default,
                Anchor.BottomCenter, new Vector2(0, -2));
            BuildPanel.Padding = new Vector2(2, 2);
            HUD_TileMap.AddEntity(BuildPanel);

            var Tiles = new Panel(new Vector2(400, 500), PanelSkin.Default, Anchor.BottomLeft,
                new Vector2(0, BuildPanelHeight));


            Tiles.BeforeUpdate = e =>
            {
                if (e.IsMouseOver)
                    e.IsFocused = true;
                else
                    e.IsFocused = false;
            };

            Tiles.PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll;
            Tiles.Visible = false;


            var WCinderblock = new Icon(IconType.None, Anchor.AutoInline, offset: new Vector2(10));
            WCinderblock.Texture = Wall_Cinderblock.UIcon;
            WCinderblock.Size = new Vector2(Wall_Cinderblock.UIcon.Bounds.Size.X);
            WCinderblock.OnClick = e =>
            {
                GameInput.SelectedTile = Wall_Cinderblock.UName;
                GameInput.selectedType = PlacingType.TILE;
            };
            WCinderblock.BeforeDraw = e =>
            {
                var i = (Icon) e;
                if (GameInput.SelectedTile == Wall_Cinderblock.UName)
                    i.DrawBackground = true;
                else
                    i.DrawBackground = false;
            };
            Tiles.AddChild(WCinderblock);

            var WDirt = new Icon(IconType.None, Anchor.AutoInline, offset: new Vector2(10));
            WDirt.Texture = Wall_Dirt.UIcon;
            WDirt.Size = new Vector2(Wall_Dirt.UIcon.Bounds.Size.X);
            WDirt.OnClick = e =>
            {
                GameInput.SelectedTile = Wall_Dirt.UName;
                GameInput.selectedType = PlacingType.TILE;
            };
            WDirt.BeforeDraw = e =>
            {
                var i = (Icon) e;
                if (GameInput.SelectedTile == Wall_Dirt.UName)
                    i.DrawBackground = true;
                else
                    i.DrawBackground = false;
            };
            Tiles.AddChild(WDirt);

            WDirt = new Icon(IconType.None, Anchor.AutoInline, offset: new Vector2(10));
            WDirt.Texture = Wall_Dirt.UIcon;
            WDirt.Size = new Vector2(Wall_Dirt.UIcon.Bounds.Size.X);
            WDirt.OnClick = e =>
            {
                GameInput.SelectedTile = Wall_Door.UName;
                GameInput.selectedType = PlacingType.TILE;
            };
            WDirt.BeforeDraw = e =>
            {
                var i = (Icon) e;
                if (GameInput.SelectedTile == Wall_Door.UName)
                    i.DrawBackground = true;
                else
                    i.DrawBackground = false;
            };
            Tiles.AddChild(WDirt);

            HUD_TileMap.AddEntity(Tiles);

            var Floors = new Panel(new Vector2(400, 500), PanelSkin.Default, Anchor.BottomLeft,
                new Vector2(0, BuildPanelHeight));


            Floors.BeforeUpdate = e =>
            {
                if (e.IsMouseOver)
                    e.IsFocused = true;
                else
                    e.IsFocused = false;
            };

            Floors.PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll;
            Floors.Visible = false;

            WDirt = new Icon(IconType.None, Anchor.AutoInline, offset: new Vector2(10));
            WDirt.Texture = Floor_Dirt.UIcon;
            WDirt.Size = new Vector2(Floor_Dirt.UIcon.Bounds.Size.X);
            WDirt.OnClick = e =>
            {
                GameInput.SelectedTile = Floor_Dirt.UName;
                GameInput.selectedType = PlacingType.TILE;
            };
            WDirt.BeforeDraw = e =>
            {
                var i = (Icon) e;
                if (GameInput.SelectedTile == Floor_Dirt.UName)
                    i.DrawBackground = true;
                else
                    i.DrawBackground = false;
            };
            Floors.AddChild(WDirt);

            WDirt = new Icon(IconType.None, Anchor.AutoInline, offset: new Vector2(10));
            WDirt.Texture = Floor_Grass.UIcon;
            WDirt.Size = new Vector2(Floor_Grass.UIcon.Bounds.Size.X);
            WDirt.OnClick = e =>
            {
                GameInput.SelectedTile = Floor_Grass.UName;
                GameInput.selectedType = PlacingType.TILE;
            };
            WDirt.BeforeDraw = e =>
            {
                var i = (Icon) e;
                if (GameInput.SelectedTile == Floor_Grass.UName)
                    i.DrawBackground = true;
                else
                    i.DrawBackground = false;
            };
            Floors.AddChild(WDirt);

            WDirt = new Icon(IconType.None, Anchor.AutoInline, offset: new Vector2(10));
            WDirt.Texture = Floor_Concrete.UIcon;
            WDirt.Size = new Vector2(Floor_Concrete.UIcon.Bounds.Size.X);
            WDirt.OnClick = e =>
            {
                GameInput.SelectedTile = Floor_Concrete.UName;
                GameInput.selectedType = PlacingType.TILE;
            };
            WDirt.BeforeDraw = e =>
            {
                var i = (Icon) e;
                if (GameInput.SelectedTile == Floor_Concrete.UName)
                    i.DrawBackground = true;
                else
                    i.DrawBackground = false;
            };
            Floors.AddChild(WDirt);

            HUD_TileMap.AddEntity(Floors);

            var Entities = new Panel(new Vector2(400, 500), PanelSkin.Default, Anchor.BottomLeft,
                new Vector2(0, BuildPanelHeight));


            Entities.BeforeUpdate = e =>
            {
                if (e.IsMouseOver)
                    e.IsFocused = true;
                else
                    e.IsFocused = false;
            };

            Entities.PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll;
            Entities.Visible = false;

            var WEntity = new Icon(IconType.None, Anchor.AutoInline, offset: new Vector2(10));
            WEntity.Texture = Entity_BasicComputer.UIcon;
            WEntity.Size = new Vector2(Wall_Dirt.UIcon.Bounds.Size.X);
            WEntity.OnClick = e =>
            {
                GameInput.SelectedTile = Entity_BasicComputer.UType;
                GameInput.selectedType = PlacingType.TILE_ENTITY;
            };
            WEntity.BeforeDraw = e =>
            {
                var i = (Icon) e;
                if (GameInput.SelectedTile == Entity_BasicComputer.UType)
                    i.DrawBackground = true;
                else
                    i.DrawBackground = false;
            };
            Entities.AddChild(WEntity);

            WEntity = new Icon(IconType.None, Anchor.AutoInline, offset: new Vector2(10));
            WEntity.Texture = Entity_BasicComputerClump.UIcon;
            WEntity.Size = new Vector2(Wall_Dirt.UIcon.Bounds.Size.X);
            WEntity.OnClick = e =>
            {
                GameInput.SelectedTile = Entity_BasicComputerClump.UType;
                GameInput.selectedType = PlacingType.TILE_ENTITY;
            };
            WEntity.BeforeDraw = e =>
            {
                var i = (Icon) e;
                if (GameInput.SelectedTile == Entity_BasicComputerClump.UType)
                    i.DrawBackground = true;
                else
                    i.DrawBackground = false;
            };
            Entities.AddChild(WEntity);

            WEntity = new Icon(IconType.None, Anchor.AutoInline, offset: new Vector2(10));
            WEntity.Texture = Entity_PowerBreaker.UIcon;
            WEntity.Size = new Vector2(Wall_Dirt.UIcon.Bounds.Size.X);
            WEntity.OnClick = e =>
            {
                GameInput.SelectedTile = Entity_PowerBreaker.UType;
                GameInput.selectedType = PlacingType.TILE_ENTITY;
            };
            WEntity.BeforeDraw = e =>
            {
                var i = (Icon) e;
                if (GameInput.SelectedTile == Entity_PowerBreaker.UType)
                    i.DrawBackground = true;
                else
                    i.DrawBackground = false;
            };
            Entities.AddChild(WEntity);

            WEntity = new Icon(IconType.None, Anchor.AutoInline, offset: new Vector2(10));
            WEntity.Texture = symbols[(int) Symbols.X_ICON];
            WEntity.Size = new Vector2(Wall_Dirt.UIcon.Bounds.Size.X);
            WEntity.OnClick = e =>
            {
                GameInput.SelectedTile = Entity_MineralDeposit.UType;
                GameInput.selectedType = PlacingType.TILE_ENTITY;
            };
            WEntity.BeforeDraw = e =>
            {
                var i = (Icon) e;
                if (GameInput.SelectedTile == Entity_MineralDeposit.UType)
                    i.DrawBackground = true;
                else
                    i.DrawBackground = false;
            };
            Entities.AddChild(WEntity);

            WEntity = new Icon(IconType.None, Anchor.AutoInline, offset: new Vector2(10));
            WEntity.Texture = Entity_Rat.UIcon;
            WEntity.Size = new Vector2(Wall_Dirt.UIcon.Bounds.Size.X);
            WEntity.OnClick = e =>
            {
                GameInput.SelectedTile = Entity_Rat.UType;
                GameInput.selectedType = PlacingType.ENTITY;
            };
            WEntity.BeforeDraw = e =>
            {
                var i = (Icon) e;
                if (GameInput.SelectedTile == Entity_Rat.UType)
                    i.DrawBackground = true;
                else
                    i.DrawBackground = false;
            };
            Entities.AddChild(WEntity);


            HUD_TileMap.AddEntity(Entities);

            var BuildTile = new Button("Walls", ButtonSkin.Default, Anchor.AutoInlineNoBreak,
                new Vector2(150, BuildPanelHeight));
            BuildTile.OnClick = e =>
            {
                Console.WriteLine("Build Wall");
                GameInput.selectedType = PlacingType.TILE;
                Tiles.Visible = !Tiles.Visible;
                Floors.Visible = false;
                Entities.Visible = false;
                GameInput.Editing = Tiles.Visible;
            };
            BuildPanel.AddChild(BuildTile);


            var BuildFloor = new Button("Floors", ButtonSkin.Default, Anchor.AutoInlineNoBreak,
                new Vector2(150, BuildPanelHeight));
            BuildFloor.OnClick = e =>
            {
                Console.WriteLine("Build Floor");
                Tiles.Visible = false;
                Floors.Visible = !Floors.Visible;
                Entities.Visible = false;
                GameInput.Editing = Floors.Visible;
            };
            BuildPanel.AddChild(BuildFloor);

            var BuildEntity = new Button("Objects", ButtonSkin.Default, Anchor.AutoInlineNoBreak,
                new Vector2(200, BuildPanelHeight));
            BuildEntity.OnClick = e =>
            {
                Console.WriteLine("Build Entity");
                Tiles.Visible = false;
                Floors.Visible = false;
                Entities.Visible = !Entities.Visible;
                GameInput.Editing = Entities.Visible;
            };
            BuildPanel.AddChild(BuildEntity);

            var Research = new Button("Research", ButtonSkin.Default, Anchor.CenterRight,
                new Vector2(200, BuildPanelHeight));
            Research.OnClick = btn => { GameStateManager.SetState(GameState.RESEARCH); };
            BuildPanel.AddChild(Research);


            var eventsPanel = new Panel(new Vector2(400, 530), PanelSkin.Default, Anchor.CenterLeft,
                new Vector2(-10, 0));
            eventsPanel.Draggable = true;

            // events log (single-time events)
            eventsPanel.AddChild(new Label("Events Log:"));
            var eventsLog = new SelectList(new Vector2(-1, 280));
            eventsLog.ExtraSpaceBetweenLines = -8;
            eventsLog.ItemsScale = 0.5f;
            eventsLog.Locked = true;
            eventsPanel.AddChild(eventsLog);

            // current events (events that happen while something is true)
            eventsPanel.AddChild(new Label("Current Events:"));
            var eventsNow = new SelectList(new Vector2(-1, 100));
            eventsNow.ExtraSpaceBetweenLines = -8;
            eventsNow.ItemsScale = 0.5f;
            eventsNow.Locked = true;
            eventsPanel.AddChild(eventsNow);

            // paragraph to show currently active panel
            targetEntityShow = new RichParagraph("test", Anchor.Auto, Color.White, 0.75f);
            var a = new TypeWriterAnimator();
            a.SpeedFactor = 10;
            a.TextToType = @"Testing typewriter animation";
            var animator = targetEntityShow.AttachAnimator(a);

            targetEntityShow.OnClick = e =>
            {
                Paragraph p = (RichParagraph) e;
                animator.Reset();
            };
            eventsPanel.AddChild(targetEntityShow);

            // add the events panel
            HUD_TileMap.AddEntity(eventsPanel);

            // whenever events log list size changes, make sure its not too long. if it is, trim it.
            eventsLog.OnListChange = entity =>
            {
                var list = (SelectList) entity;
                if (list.Count > 100) list.RemoveItem(0);
            };

            HUD_TileMap.OnClick = entity =>
            {
                eventsLog.AddItem("Click: " + entity.GetType().Name);
                eventsLog.scrollToEnd();
            };
            HUD_TileMap.OnRightClick = entity =>
            {
                eventsLog.AddItem("RightClick: " + entity.GetType().Name);
                eventsLog.scrollToEnd();
            };
            HUD_TileMap.OnMouseDown = entity =>
            {
                eventsLog.AddItem("MouseDown: " + entity.GetType().Name);
                eventsLog.scrollToEnd();
            };
            HUD_TileMap.OnRightMouseDown = entity =>
            {
                eventsLog.AddItem("RightMouseDown: " + entity.GetType().Name);
                eventsLog.scrollToEnd();
            };
            HUD_TileMap.OnMouseEnter = entity =>
            {
                eventsLog.AddItem("MouseEnter: " + entity.GetType().Name);
                eventsLog.scrollToEnd();
            };
            HUD_TileMap.OnMouseLeave = entity =>
            {
                eventsLog.AddItem("MouseLeave: " + entity.GetType().Name);
                eventsLog.scrollToEnd();
            };
            HUD_TileMap.OnMouseReleased = entity =>
            {
                eventsLog.AddItem("MouseReleased: " + entity.GetType().Name);
                eventsLog.scrollToEnd();
            };
            HUD_TileMap.OnMouseWheelScroll = entity =>
            {
                eventsLog.AddItem("Scroll: " + entity.GetType().Name);
                eventsLog.scrollToEnd();
            };
            HUD_TileMap.OnStartDrag = entity =>
            {
                eventsLog.AddItem("StartDrag: " + entity.GetType().Name);
                eventsLog.scrollToEnd();
            };
            HUD_TileMap.OnStopDrag = entity =>
            {
                eventsLog.AddItem("StopDrag: " + entity.GetType().Name);
                eventsLog.scrollToEnd();
            };
            HUD_TileMap.OnFocusChange = entity =>
            {
                eventsLog.AddItem("FocusChange: " + entity.GetType().Name);
                eventsLog.scrollToEnd();
            };
            HUD_TileMap.OnValueChange = entity =>
            {
                if (entity.Parent == eventsLog) return;
                eventsLog.AddItem("ValueChanged: " + entity.GetType().Name);
                eventsLog.scrollToEnd();
            };*/
        }

        private static void InitHUD_Research()
        {
            /*HUD_Research_Tooltip_Panel = new Panel(new Vector2(600, 600), anchor: Anchor.TopLeft);
            HUD_Research_Tooltip_Panel.AfterDraw += e => { e.Visible = false; };
            HUD_Research_Tooltip_Panel.BeforeDraw += ent =>
            {
                ent.Size = new Vector2(
                    HUD_Research_Tooltip_Header.GetCharacterActualSize().X * HUD_Research_Tooltip_Header.Text.Length +
                    300, 250);
                HUD_Research_Tooltip_Header.Size =
                    new Vector2(
                        HUD_Research_Tooltip_Header.GetCharacterActualSize().X *
                        HUD_Research_Tooltip_Header.Text.Length + 20, HUD_Research_Tooltip_Header.Size.Y);

                // get dest rect and calculate tooltip position based on size and mouse position
                var destRect = HUD_Research_Tooltip_Panel.GetActualDestRect();
                destRect.Height += HUD_Research_Tooltip_Text.GetActualDestRect().Height;
                var position =
                    UserInterface.Active.GetTransformedCursorPos(new Vector2(-ent.Size.X / 2, -ent.Size.Y - 10));

                // make sure tooltip is not out of screen boundaries
                var screenBounds = UserInterface.Active.Root.GetActualDestRect();
                if (position.Y < screenBounds.Top) position.Y = screenBounds.Top;
                if (position.Y > screenBounds.Bottom - destRect.Height)
                    position.Y = screenBounds.Bottom - destRect.Height;
                if (position.X < screenBounds.Left) position.X = screenBounds.Left;
                if (position.X > screenBounds.Right - destRect.Width) position.X = screenBounds.Right - destRect.Width;

                // update tooltip position
                HUD_Research_Tooltip_Panel.SetAnchorAndOffset(Anchor.TopLeft,
                    position / UserInterface.Active.GlobalScale);
            };
            HUD_Research_Tooltip_Panel.Visible = false;

            HUD_Research_Tooltip_Header = new Header("Tooltip", Anchor.TopLeft);
            HUD_Research_Tooltip_Header.FillColor = Color.White;
            HUD_Research_Tooltip_Header.Padding = new Vector2(0, 0);
            HUD_Research_Tooltip_Header.Offset = new Vector2(0, 5);
            HUD_Research_Tooltip_Panel.AddChild(HUD_Research_Tooltip_Header);

            HUD_Research_Tooltip_Icon1 = new Image(ResearchManager.textures[(int) ResearchTexture.MISC],
                new Vector2(64), anchor: Anchor.TopRight, offset: new Vector2(-15, -15));
            HUD_Research_Tooltip_Icon1.FillColor = Color.Red;
            HUD_Research_Tooltip_IconT1 = new Paragraph("0", Anchor.Center, offset: new Vector2(0, -12));
            HUD_Research_Tooltip_Icon2 = new Image(ResearchManager.textures[(int) ResearchTexture.DESKTOP],
                new Vector2(64), anchor: Anchor.TopRight, offset: new Vector2(64 + 10 - 15, -15));
            HUD_Research_Tooltip_Icon2.FillColor = Color.Blue;
            HUD_Research_Tooltip_IconT2 = new Paragraph("0", Anchor.Center, offset: new Vector2(0, -12));
            HUD_Research_Tooltip_Icon3 = new Image(ResearchManager.textures[(int) ResearchTexture.SERVER],
                new Vector2(64), anchor: Anchor.TopRight, offset: new Vector2(2 * (64 + 10) - 15, -15));
            HUD_Research_Tooltip_Icon3.FillColor = Color.Aquamarine;
            HUD_Research_Tooltip_IconT3 = new Paragraph("0", Anchor.Center, offset: new Vector2(0, -12));

            HUD_Research_Tooltip_Panel.AddChild(HUD_Research_Tooltip_Icon1);
            HUD_Research_Tooltip_Icon1.AddChild(HUD_Research_Tooltip_IconT1);
            HUD_Research_Tooltip_Panel.AddChild(HUD_Research_Tooltip_Icon2);
            HUD_Research_Tooltip_Icon2.AddChild(HUD_Research_Tooltip_IconT2);
            HUD_Research_Tooltip_Panel.AddChild(HUD_Research_Tooltip_Icon3);
            HUD_Research_Tooltip_Icon3.AddChild(HUD_Research_Tooltip_IconT3);

            HUD_Research_Tooltip_Panel.AddChild(new HorizontalLine());

            HUD_Research_Tooltip_Text = new RichParagraph("Tooltip", size: new Vector2(0, 0));
            HUD_Research_Tooltip_Text.TextStyle = FontStyle.Italic;

            HUD_Research_Tooltip_Panel.AddChild(HUD_Research_Tooltip_Text);
            HUD_Research.AddEntity(HUD_Research_Tooltip_Panel);

            var ResearchPanel = new Panel(new Vector2(400, Game1.ScreenHeight + 10), PanelSkin.Default,
                Anchor.CenterLeft);
            var sb = new VerticalScrollbar(0, 100, Anchor.CenterRight, adjustMaxAutomatically: true);
            ResearchPanel.AddChild(sb);

            ResearchPanel.AfterDraw = e =>
            {
                var transform = Matrix.CreateTranslation(new Vector3(20, 20, 0));

                spriteBatch.Begin(transformMatrix: transform, samplerState: SamplerState.PointClamp);

                var i = 0;
                foreach (ResearchNode n in ResearchManager.nodes)
                {
                    var c = n.GetColor();
                    spriteBatch.Draw(ResearchManager.textures[(int) n.Tex], new Rectangle(0, i * 80, 64, 64), c);
                    spriteBatch.DrawString(clacon, n.Name, new Vector2(80, 0 + i * 80), Color.Blue);
                    spriteBatch.DrawString(clacon, "24.189.103.234", new Vector2(80, 40 + i * 80), Color.LightGray);
                    i++;
                }

                spriteBatch.End();
            };

            HUD_Research.AddEntity(ResearchPanel);*/
        }
    }
}