using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hearthrock.Pegasus.Internal
{
    public class RockGameStateMonitor
    {
        public bool AddGameOverListener()
        {
            return GameState.Get().RegisterGameOverListener(GameOverCallback, null);
        }

        private void GameOverCallback(TAG_PLAYSTATE playstate, object userdata)
        {
            
            UIStatus.Get().AddInfo("callback:"+playstate.ToString());
            if (playstate == TAG_PLAYSTATE.LOSING || playstate == TAG_PLAYSTATE.LOST)
            {
            }
            else
            {
                
            }
        }
    }
}
