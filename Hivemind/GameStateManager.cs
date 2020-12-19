using System.Collections.Generic;
using Hivemind.GUI;
using Hivemind.World;

namespace Hivemind
{
    internal enum GameState
    {
        MAIN_MENU,
        TILEMAP,
        RESEARCH
    }

    internal class GameStateManager
    {
        private static GameState gameState;

        private static Dictionary<int, TileMap> worlds = new Dictionary<int, TileMap>();
        private static int ActiveWorld, CWorldID = 0;

        private static bool exitRequested;

        public static void SetState(GameState state)
        {
            switch (state)
            {
                case GameState.MAIN_MENU:
                    GuiController.SetState(GUIState.MAIN_MENU);
                    break;
                case GameState.TILEMAP:
                    GuiController.SetState(GUIState.HUD_TILEMAP);
                    break;
                case GameState.RESEARCH:
                    GuiController.SetState(GUIState.HUD_RESEARCH);
                    break;
                default:
                    SetState(GameState.MAIN_MENU);
                    break;
            }


            gameState = state;
        }

        public static GameState State()
        {
            return gameState;
        }

        public static void Exit()
        {
            exitRequested = true;
        }

        public static bool ExitRequested()
        {
            return exitRequested;
        }
    }
}