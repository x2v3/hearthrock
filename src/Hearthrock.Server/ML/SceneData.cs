using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML.Runtime.Api;

namespace Hearthrock.Server.ML
{
    
    public class SceneData
    {
        [Column("win","win")]
        public float Win { get; set; }
        [Column("selfHeroClass","selfHeroClass")]
        public float SelfHeroClass { get; set; }
        [Column("opHeroClass","opHeroClass")]
        public float OpHeroClass { get; set; }
        [Column("round","round")]
        public float Round { get; set; }
        [Column("selfHeroHealth","selfHeroHealth")]
        public float SelfHeroHealth { get;set; }
        [Column("opHeroHealth","opHeroHealth")]
        public float OpHeroHealth { get; set; }
        [ColumnName("selfMinionsHealth")]
        public float SelfMinionsHealth { get; set; }
        [ColumnName("opMinionsHealth")]
        public float OpMinionsHealth { get; set; }
        [ColumnName("selfMinionsAttackDamage")]
        public float SelfMinionsAttackDamage { get; set; }
        [ColumnName("opMinionsAttackDamage")]
        public float OpMinionsAttackDamage { get; set; }
        [ColumnName("selfHasWindFury")]
        public float SelfHasWindFury { get; set; }
        [ColumnName("opHasWindFury")]
        public float OpHasWindFury { get; set; }
        [ColumnName("selfHasLifeSteal")]
        public float SelfHasLifeSteal { get; set; }
        [ColumnName("opHasLifeSteal")]
        public float OpHasLifeSteal { get; set; }
        [ColumnName("selfTauntMinionsHealth")]
        public float SelfTauntMinionHealth { get; set; }
        [ColumnName("opTauntMinionsHealth")]
        public float OpTauntMinionHealth { get; set; }
        [ColumnName("selfHeroAttackDamage")]
        public float SelfHeroAttackDamage { get; set; }
        [ColumnName("opHeroAttackDamage")]
        public float OpHeroAttackDamage { get; set; }
        [ColumnName("diffHeroHealth")]
        public float DiffHeroHealth => SelfHeroHealth - OpHeroHealth;
        [ColumnName("diffMinionsHealth")]
        public float DiffMinionsHealth => SelfMinionsHealth - OpMinionsHealth;
        [ColumnName("diffAttackDamage")]
        public float DiffAttackDamage =>
            SelfHeroAttackDamage + SelfMinionsAttackDamage - OpHeroAttackDamage - OpMinionsAttackDamage;
        [ColumnName("diffTauntMinionsHealth")]
        public float DiffTauntMinionsHealth => SelfTauntMinionHealth - OpTauntMinionHealth;
    }

    public class ScenePrediction
    {
        [ColumnName("PredictedLabel")]
        public uint Win { get; set; }
        //[ColumnName("Probability")]
        [NoColumn]
        public float Probability { get; set; }
        [ColumnName("Score")]
        public float[] Score { get; set; }

    }
    
}
