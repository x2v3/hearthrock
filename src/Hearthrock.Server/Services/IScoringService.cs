using Hearthrock.Contracts;
using Hearthrock.Server.ML;

namespace Hearthrock.Server.Services
{
    public interface IScoringService
    {
        int GetScore(SceneData data);
    }
}