using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HearthLearning.DataReader;
using HearthLearning.ML;
using Hearthrock.Contracts;
using Hearthrock.Server.Score;
using Microsoft.Extensions.Logging;
using Microsoft.ML.Legacy;
using Microsoft.ML.Legacy.Data;
using Microsoft.ML.Legacy.Trainers;
using Microsoft.ML.Legacy.Transforms;

namespace Hearthrock.Server.Services
{
    public class ScoringService : IScoringService
    {
        public ScoringService(ILoggerFactory loggerFactory,Config cfg)
        {
            config = cfg;
            logger = loggerFactory.CreateLogger(this.GetType());
            var trainer = new Trainer();
            var filePath = Path.Combine(Environment.CurrentDirectory, "score.model");
            if (File.Exists(filePath))
            {
                model = trainer.LoadModel(filePath);
                logger.LogInformation("local model file loaded.");
            }
            else
            {
                logger.LogWarning("Local model file not found, start training.");
                Train();
            }
        }

        private readonly Config config;
        private readonly ILogger logger;
        private PredictionModel<SceneData, ScenePrediction> model;

        public int GetScore(SceneData data)
        {
            if (model == null)
            {
                logger.LogError("model not initialized.");
                throw new ModelNotInitializedException();
            }
            var prediction = model.Predict(data);
            logger.LogInformation($"scene:round {data.Round},prediction win:{prediction.Win}, score:{prediction.Score}, probability:{prediction.Probability}");
            return (int)(prediction.Score * 10000)* (prediction.Win?1:-1);
        }
        
        public void Train(bool swapPlayer=false)
        {
            var dr = new MysqlDbDataReader(config.PlayDBConnectionString);
            var tmatch = dr.GetMatches(1500, 0);
            var data = Helper.MatchToSceneData(tmatch,swapPlayer);
            var testData = Helper.MatchToSceneData(dr.GetMatches(100, 1500),swapPlayer);
            model = BuildAndTrain(data,testData);
            var filePath = Path.Combine(Environment.CurrentDirectory, "score.model");
            model.WriteAsync(filePath);
        }


        public PredictionModel<SceneData, ScenePrediction> BuildAndTrain(List<SceneData> data, List<SceneData> testdata)
        {
            var cs = CollectionDataSource.Create(data);
            var pipeline = new LearningPipeline();
            pipeline.Add(cs);
            pipeline.Add(new ColumnCopier(("win", "Label")));
            pipeline.Add(new CategoricalOneHotVectorizer("selfHeroClass", "opHeroClass"));
            pipeline.Add(new ColumnConcatenator("Features"
                , "selfHeroClass"
                , "opHeroClass"
                , "round"
                , "selfHeroHealth"
                , "opHeroHealth"
                , "selfMinionsHealth"
                , "opMinionsHealth"
                , "selfMinionsAttackDamage"
                , "opMinionsAttackDamage"
                , "selfHasWindFury"
                , "opHasWindFury"
                , "selfHasLifeSteal"
                , "opHasLifeSteal"
                , "selfTauntMinionsHealth"
                , "opTauntMinionsHealth"
                , "selfHeroAttackDamage"
                , "opHeroAttackDamage"
                , "diffHeroHealth"
                , "diffMinionsHealth"
                , "diffAttackDamage"
                , "diffTauntMinionsHealth"
            ));
            var classifier = new LogisticRegressionBinaryClassifier();

            pipeline.Add(classifier);
            model = pipeline.Train<SceneData, ScenePrediction>();

            var eva = new Microsoft.ML.Legacy.Models.BinaryClassificationEvaluator();
            var m = eva.Evaluate(model, CollectionDataSource.Create(testdata));
            Console.WriteLine($"acc:{m.Accuracy}");
            Console.WriteLine($"auc:{m.Auc}");
            return model;
        }

        public void TrainAsync(bool swapPlayer=false)
        {
            Task.Run(new Action(() => { Train(swapPlayer);}));
        }

    }

    public class ModelNotInitializedException : Exception
    {

    }
}
