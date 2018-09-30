using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hearthrock.Pegasus.Internal
{
    public class RockGameStateMonitor
    {
        public event EventHandler<GameOverEventArgs> GameOver;

        private GameState currentGameState;

        public bool AddGameOverListener()
        {
            var gs = GameState.Get();
            if (gs == null)
            {
                currentGameState?.UnregisterGameOverListener(GameOverCallback);
                return false;
            }
            if (currentGameState != gs)
            {
                currentGameState?.UnregisterGameOverListener(GameOverCallback);
                currentGameState = gs;
                return gs.RegisterGameOverListener(GameOverCallback);
            }
            else
            {
                return true;
            }
        }

        public void RemoveGameOverListener()
        {
            currentGameState?.UnregisterGameOverListener(GameOverCallback);
        }

        private void GameOverCallback(TAG_PLAYSTATE playstate, object userdata)
        {
            if (playstate == TAG_PLAYSTATE.LOSING || playstate == TAG_PLAYSTATE.LOST)
            {
                GameOver?.Invoke(this, new GameOverEventArgs() { Won = false });
            }
            else if (playstate == TAG_PLAYSTATE.WINNING || playstate == TAG_PLAYSTATE.WON)
            {
                GameOver?.Invoke(this, new GameOverEventArgs { Won = true });
            }
        }

        public class GameOverEventArgs : EventArgs
        {
            public bool Won { get; set; }
        }
    }
}