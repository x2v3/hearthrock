using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hearthrock.Pegasus.Internal
{
    public class RockGameStateMonitor
    {

        public event EventHandler<GameOverEventArgs> GameOver;

        public bool AddGameOverListener()
        {
            var gs = GameState.Get();
            if (gs == null)
            {
                return false;
            }
            gs.UnregisterGameOverListener(GameOverCallback, null);
            return gs.RegisterGameOverListener(GameOverCallback, null);
        }

        public void RemoveGameOverListener()
        {
            var gs = GameState.Get();
            if (gs != null)
            {

                gs.UnregisterGameOverListener(GameOverCallback, null);
            }
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
