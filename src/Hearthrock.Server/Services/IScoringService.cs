using HearthLearning.ML;
using Hearthrock.Contracts;

namespace Hearthrock.Server.Services
{
    public interface IScoringService
    {
        int GetScore(SceneData data);
    }
}