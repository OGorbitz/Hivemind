using Hivemind.GUI;
using Hivemind.Input;
using Hivemind.World;
using Hivemind.World.Entity;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Windows.Forms;

namespace Hivemind
{
    public class Hivemind : Game
    {
        private static Hivemind _instance;

        private static GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public static Texture2D _icon, ComputerLines;

        public static int ScreenWidth, ScreenHeight;

        public static GameTime CurrentGameTime;

        public Hivemind()
        {
            _instance = this;

            _graphics = new GraphicsDeviceManager(this);
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
            _graphics.ApplyChanges();

            var r = System.Windows.Forms.Screen.AllScreens[0].Bounds;

            _graphics.IsFullScreen = false;
            _graphics.PreferredBackBufferWidth = r.Width;
            _graphics.PreferredBackBufferHeight = r.Height;
            _graphics.ApplyChanges();

            ScreenWidth = r.Width;
            ScreenHeight = r.Height;

            Window.AllowAltF4 = true;
            Window.IsBorderless = true;
            Window.Position = new Point(r.Location.X, r.Location.Y);

            IsMouseVisible = true;

            Content.RootDirectory = "Content";

            Window.Title = "Hivemind";
        }

        public static void ToggleFullscreen()
        {
            Point p = _instance.Window.Position;
            Screen screen = System.Windows.Forms.Screen.FromPoint(new System.Drawing.Point(p.X, p.Y));
            _graphics.PreferredBackBufferWidth = screen.Bounds.Width;
            _graphics.PreferredBackBufferHeight = screen.Bounds.Height;
            ScreenWidth = screen.Bounds.Width;
            ScreenHeight = screen.Bounds.Height;
            _graphics.ApplyChanges();
            _graphics.ToggleFullScreen();
        }

        protected override void Initialize()
        {
            base.Initialize();

            GuiController.Init(GraphicsDevice);

            GameStateManager.SetState(GameState.MAIN_MENU);
        }

        protected override void LoadContent()
        {
            Myra.MyraEnvironment.Game = this;

            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _icon = Content.Load<Texture2D>("cpu");
            ComputerLines = Content.Load<Texture2D>("computer_lines");

            FloorMask.LoadContent(Content, GraphicsDevice);
            TextureAtlas.Init(GraphicsDevice);
            TileManager.LoadTiles(Content, GraphicsDevice);
            EntityManager.LoadAssets(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            CurrentGameTime = gameTime;
            // TODO: Add your update logic here
            GameInput.Update(gameTime);
            GuiController.Update(gameTime);

            WorldManager.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GuiController.Render(GraphicsDevice);

            switch (GameStateManager.State())
            {
                case GameState.TILEMAP:
                    if (GameInput.Editing) WorldManager.RenderEditor(_spriteBatch, GraphicsDevice, gameTime);
                    WorldManager.Draw(_spriteBatch, GraphicsDevice, gameTime);
                    if (GameInput.Editing) WorldManager.DrawEditor(_spriteBatch, GraphicsDevice, gameTime);
                    break;
                case GameState.RESEARCH:
                    //ResearchManager.Draw(spriteBatch, GraphicsDevice, gameTime);
                    break;
                case GameState.MAIN_MENU:
                    GraphicsDevice.Clear(new Color(0f, 0.05f, 0f));
                    var ms = gameTime.TotalGameTime.Seconds * 1000 + gameTime.TotalGameTime.Milliseconds;
                    var n = (int)(ms % 3000 / 3000f * 96f);
                    _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                    for (var x = -n; x <= GraphicsDevice.Viewport.Height; x += ComputerLines.Height * 3)
                        _spriteBatch.Draw(ComputerLines,
                            new Rectangle(new Point(0, x),
                                new Point(GraphicsDevice.Viewport.Width, ComputerLines.Height * 3)),
                            new Color(1f, 1f, 1f, 0.3f));
                    n = (int)(ms % 2000 / 2000f * 64f);
                    for (var x = n - 64; x <= GraphicsDevice.Viewport.Height; x += ComputerLines.Height * 2)
                        _spriteBatch.Draw(ComputerLines,
                            new Rectangle(new Point(0, x),
                                new Point(GraphicsDevice.Viewport.Width, ComputerLines.Height * 2)),
                            new Color(1f, 1f, 1f, 0.25f));
                    _spriteBatch.End();
                    break;
            }

            GuiController.Draw(GraphicsDevice);

            base.Draw(gameTime);
        }
    }
}
