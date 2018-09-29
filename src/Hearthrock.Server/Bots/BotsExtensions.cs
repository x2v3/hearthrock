using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hearthrock.Bot;
using Hearthrock.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Hearthrock.Server.Bots
{
    public static class BotsExtensions
    {
        public static IServiceCollection AddSimpleMonteCarloBot(this IServiceCollection svcCollection)
        {
            return svcCollection.AddSingleton<IRockBot,SimpleMonteCarloBot>();
        }

        public static IServiceCollection AddBuiltinBot(this IServiceCollection svcCollection)
        {
            return svcCollection.AddSingleton<IRockBot, RockBot>();
        }
        
        public static IServiceCollection AddMyExperimentBot(this IServiceCollection svcCollection)
        {
            return svcCollection.AddSingleton<IRockBot,MyExperimentBot>();
        }

        
        public static IServiceCollection AddPunchFaceBot(this IServiceCollection svcCollection)
        {
            return svcCollection.AddSingleton<IRockBot,PunchFaceBot>();
        }
    }
}
