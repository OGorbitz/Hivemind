using Hivemind.GUI;
using Hivemind.Input;
using Hivemind.Utility;
using Hivemind.World;
using Hivemind.World.Entity;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public static float CurrentFPS = 0;

        public static long TotalFrames { get; private set; }
        public static float TotalSeconds { get; private set; }
        public static float AverageFramesPerSecond { get; private set; }
        public static float CurrentFramesPerSecond { get; private set; }
        private static double last = 0, now = 0;
        public const int MAXIMUM_SAMPLES = 10;
        private static Queue<float> _sampleBuffer = new Queue<float>();

        public static ContentManager CManager;

        public static bool DebugMode = false;

        public Hivemind()
        {
            _instance = this;

            var r = System.Windows.Forms.Screen.AllScreens[1].Bounds;
            ScreenWidth = r.Width;
            ScreenHeight = r.Height;

            _graphics = new GraphicsDeviceManager(this);
            Window.AllowAltF4 = true;
            Window.IsBorderless = true;
            Window.Position = new Point(r.Location.X, r.Location.Y);
            Window.Title = "Hivemind";
            
            IsMouseVisible = true;

            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
            _graphics.SynchronizeWithVerticalRetrace = false;

            IsFixedTimeStep = false;

            _graphics.PreferredBackBufferWidth = r.Width;
            _graphics.PreferredBackBufferHeight = r.Height;
            _graphics.ApplyChanges();

            CManager = Content;

            Content.RootDirectory = "Content";
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

            GuiController.Init(GraphicsDevice, Content);

            Helper.Init(GraphicsDevice);

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
            Fog.Init(Content, GraphicsDevice);
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
            CurrentGameTime = gameTime;

            last = now;
            now = DateTime.Now.Subtract(DateTime.MinValue).TotalSeconds;

            CurrentFramesPerSecond = (float)(1.0 / (now - last));

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


            switch (GameStateManager.State())
            {
                case GameState.TILEMAP:
                    WorldManager.Draw(_spriteBatch, GraphicsDevice, gameTime);
                    break;
                case GameState.RESEARCH:
                    //ResearchManager.Draw(spriteBatch, GraphicsDevice, gameTime);
                    break;
                case GameState.MAIN_MENU:

                    break;
            }

            GuiController.Draw(_spriteBatch, GraphicsDevice, gameTime);

            //base.Draw(gameTime);
        }
    }
}
