using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pinkerton.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinkerton.Services
{
    public class DependencyInjectorService
    {
        public IHost ConfigureHostProvider()
        {
            var _host = Host.CreateDefaultBuilder().ConfigureServices(services =>
            {
                services.AddSingleton<BotDbContext>();
            }).Build();

            return _host;
        }
    }
}
