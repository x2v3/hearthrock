using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Hearthrock.Server.Services
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddActionLogService(this IServiceCollection serviceCollection, string connStr)
        {
            var svc = new ActionLogService(connStr);
            return serviceCollection.AddSingleton<ActionLogService>(svc);
        }
    }
}
