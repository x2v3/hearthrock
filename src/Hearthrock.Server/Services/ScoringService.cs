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

        private Config config;
        private ILogger logger;
        private PredictionModel<SceneData, ScenePrediction> model;

        public int GetScore(SceneData data)
        {
            if (model == null)
            {
                logger.LogError("model not initialized.");
                throw new ModelNotInitializedException();
            }
            var prediction = model.Predict(data);
            logger.LogInformation($"scene:round {data.Round},prediction win:{prediction.Win}, score:{prediction.Score}");
            return (int)(prediction.Score * 10000);
        }
        
        public void Train()
        {
            
            var dr = new MysqlDbDataReader(config.PlayDBConnectionString);
            var t = new Trainer();
            var tmatch = dr.GetMatches(1500, 0);
            var data = Helper.MatchToSceneData(tmatch);
            var testData = Helper.MatchToSceneData(dr.GetMatches(100, 1500));
            model = t.BuildAndTrain(data,testData);
            var filePath = Path.Combine(Environment.CurrentDirectory, "score.model");
            model.WriteAsync(filePath);
        }

    }

    public class ModelNotInitializedException : Exception
    {

    }
}
