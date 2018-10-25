using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hearthrock.Contracts;

namespace Hearthrock.Server.ML
{
    
    public class Match
    {
        public bool Win { get;set;}
        public string Session { get; set; }
        public List<RockScene> Turns { get;set; }
        public int SelfHeroClass { get; set; }
        public int OpponentHeroClass { get; set; }
        public int Id { get; set; }
    }
}
