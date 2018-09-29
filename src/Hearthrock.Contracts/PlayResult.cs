using System;
using System.Collections.Generic;
using System.Text;

namespace Hearthrock.Contracts
{
    public class PlayResult
    {
        public string Session { get; set; }
        public bool Won { get; set; }
        public string PlayerName { get; set; }
    }
}
