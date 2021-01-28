﻿using Hivemind.GUI;
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

            GuiController.Init(GraphicsDevice, Content);

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
            CurrentGameTime = gameTime;

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

            GuiController.Draw(_spriteBatch, GraphicsDevice);

            base.Draw(gameTime);
        }
    }
}
