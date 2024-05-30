using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Pinkerton.Data;
using Pinkerton.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pinkerton.Factories
{
    public class PinkertonDbContextFactory : IDesignTimeDbContextFactory<BotDbContext>
    {
        public BotDbContext CreateDbContext(string[]? args = null)
        {

            var json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "config.json"));
            var conStr = JsonSerializer.Deserialize<ConfigJson>(json)!.ConnectionString;
            var options = new DbContextOptionsBuilder<BotDbContext>();
            options.UseNpgsql(conStr);

            return new BotDbContext(options.Options);

        }
    }
}
