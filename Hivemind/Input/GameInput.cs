using System;
using System.Collections.Generic;
using Hivemind.GUI;
using Hivemind.World;
using Hivemind.World.Entity;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Hivemind.Input
{
    internal enum Action
    {
        BUILD,
        DESTROY,
        SELECT
    }

    internal enum PlacingType
    {
        TILE,
        ENTITY,
        TILE_ENTITY
    }

    internal class GameInput
    {
        public static Vector2 MousePos => mousepos;
        public static Vector2 WorldPos => worldpos;

        public static bool ctrl, Editing;
        private static Vector2 mousepos, worldpos, tileclick;
        private static bool wireview = false;
        private static int scrollWheelValue, scrollWheelChange;

        private static Action CurrentAction = Action.SELECT;
        //public static string SelectedTile = Wall_Cinderblock.UName;
        public static PlacingType selectedType = PlacingType.TILE;
        public static int Rotation;

        private static bool FSCREEN = false;

        private static bool DMOUSE_LEFT = false;
        private static bool DMOUSE_RIGHT = false;

        private static readonly Keys KEY_UP = Keys.W;
        private static bool DKEY_UP = false;
        private static readonly Keys KEY_LEFT = Keys.A;
        private static bool DKEY_LEFT = false;
        private static readonly Keys KEY_DOWN = Keys.S;
        private static bool DKEY_DOWN = false;
        private static readonly Keys KEY_RIGHT = Keys.D;
        private static bool DKEY_RIGHT = false;

        private static readonly Keys KEY_ROTATE = Keys.R;
        private static bool DKEY_ROTATE;

        private static readonly Keys KEY_BACK = Keys.Escape;
        private static bool DKEY_BACK;


        public static int ScrollWheelChange()
        {
            return scrollWheelChange;
        }

        public static void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.F4) && Keyboard.GetState().IsKeyDown(Keys.LeftAlt))
            {
                GameStateManager.Exit();
                return;
            }

            if (!FSCREEN)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Enter) && Keyboard.GetState().IsKeyDown(Keys.LeftAlt))
                {
                    Hivemind.ToggleFullscreen();
                    FSCREEN = true;
                }
            }
            else
            {
                if(!(Keyboard.GetState().IsKeyDown(Keys.Enter) && Keyboard.GetState().IsKeyDown(Keys.LeftAlt)))
                {
                    FSCREEN = false;
                }
            }
            

            scrollWheelChange = Mouse.GetState().ScrollWheelValue - scrollWheelValue;
            scrollWheelValue = Mouse.GetState().ScrollWheelValue;

            var vel = new Vector2();
            if (Keyboard.GetState().IsKeyDown(KEY_UP)) vel.Y += -1;
            if (Keyboard.GetState().IsKeyDown(KEY_DOWN)) vel.Y += 1;
            if (Keyboard.GetState().IsKeyDown(KEY_LEFT)) vel.X += -1;
            if (Keyboard.GetState().IsKeyDown(KEY_RIGHT)) vel.X += 1;


            var NotOverHUD = false;
            if (!GuiController.IsMouseOverGUI())
                NotOverHUD = true;

            if (Keyboard.GetState().IsKeyDown(KEY_BACK))
            {
                if (!DKEY_BACK)
                {
                    DKEY_BACK = true;

                    switch (GameStateManager.State())
                    {
                        case GameState.MAIN_MENU:
                            if (GuiController.GetState() == GUIState.MAIN_MENU_CREDITS)
                            {
                                GuiController.SetState(GUIState.MAIN_MENU);
                                GuiController.Credits.Text = "HELP";
                            }
                            break;
                        case GameState.TILEMAP:
                            GameStateManager.SetState(GameState.MAIN_MENU);
                            break;
                        case GameState.RESEARCH:
                            GameStateManager.SetState(GameState.TILEMAP);
                            break;
                    }
                }
            }
            else
            {
                DKEY_BACK = false;
            }

            if (Keyboard.GetState().IsKeyDown(KEY_ROTATE))
            {
                if (!DKEY_ROTATE)
                {
                    DKEY_ROTATE = true;
                    Rotation += 1;
                    if (Rotation >= 4)
                        Rotation = 0;
                }
            }
            else
            {
                DKEY_ROTATE = false;
            }

            ctrl = false;
            if (Keyboard.GetState().IsKeyDown(Keys.LeftControl)) ctrl = true;

            var mouse = Mouse.GetState();
            mousepos = mouse.Position.ToVector2();

            switch (GameStateManager.State())
            {
                case GameState.MAIN_MENU:

                    break;
                case GameState.TILEMAP:
                    if (NotOverHUD)
                        WorldManager.GetActiveTileMap().Cam.UpdateScale();


                    var tm = WorldManager.GetActiveTileMap();
                    var worldpos = tm.Cam.Unproject(mousepos);
                    var tilepos = TileMap.GetTileCoords(worldpos);


                    if (NotOverHUD)
                    {
                        if (!DMOUSE_LEFT)
                        {
                            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                            {
                                //New mouse click
                                DMOUSE_LEFT = true;
                                var pointed = WorldManager.GetActiveTileMap().GetEntities(new Rectangle(worldpos.ToPoint(), new Point(1, 1)));

                                List<ISelectable> selected = new List<ISelectable>();
                                foreach (MovingEntity e in pointed)
                                {
                                    if (e.GetBounds().Contains(worldpos))
                                        selected.Add(e);
                                }

                                if (selected.Count > 0)
                                {
                                    if (Keyboard.GetState().IsKeyDown(Keys.LeftControl))
                                    {
                                        Selection.AddSelect(selected);
                                    }
                                    else
                                    {
                                        Selection.Select(selected);
                                    }
                                }
                                else
                                {
                                    var e = WorldManager.GetActiveTileMap().GetTileEntity(tilepos);
                                    if (e != null)
                                    {
                                        if (Keyboard.GetState().IsKeyDown(Keys.LeftControl))
                                        {
                                            Selection.AddSelect(e);
                                        }
                                        else
                                        {
                                            Selection.Select(e);
                                        }
                                    }
                                    else
                                    {
                                        Selection.Selected.Clear();
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (Mouse.GetState().LeftButton == ButtonState.Released)
                                DMOUSE_LEFT = false;
                        }
                    }

                    if(Selection.Selected.Count == 1)
                    {
                        Type[] interfaces = Selection.Selected[0].GetType().GetInterfaces();
                        bool Controllable = false;
                        for(int i = 0; i < interfaces.Length; i++)
                        {
                            if (interfaces[i] == typeof(IControllable))
                            {
                                Controllable = true;
                            }
                        }
                        if (Controllable)
                        {
                            ((IControllable)Selection.Selected[0]).ControllerMove(vel);
                        }
                    }

                    if(!Selection.UpdateCam())
                        WorldManager.GetActiveTileMap().Cam.Move(vel);

                    break;
                case GameState.RESEARCH:
                    break;
            }
        }
    }
}