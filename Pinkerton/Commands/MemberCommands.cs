using Guilded.Base.Embeds;
using Guilded.Commands;
using Pinkerton.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Pinkerton.Commands
{
    public class MemberCommands: CommandModule
    {
        #region UPTIME
        [Command(Aliases = [ "uptime", "alive", "online" ])]
        [Description("get the total time the bot has been online since the last restart")]
        public async Task Uptime(CommandEvent invokator)
        {
            var uptime = BotTimerService.GetBotUptime();
            var sw = Stopwatch.StartNew();
            //using var db = dbFactory.CreateDbContext();
            //var user = db!.ServerMembers!.First();
            //sw.Stop();
            //var dbLatency = sw.ElapsedMilliseconds;

            sw.Start();
            var ping = new Ping();
            await ping.SendPingAsync("google.com");
            sw.Stop();
            var pingTime = sw.ElapsedMilliseconds;

            var embed = new Embed()
            {
                Title = $"{invokator.ParentClient.Name} has been online for {uptime}",
                Footer = new EmbedFooter($"{invokator.ParentClient.Name} always watching..."),
                Timestamp = DateTime.Now
            };
            //embed.AddField("Db Latency", $"{dbLatency}ms", true);
            //embed.AddField("Ping Reply", $"{pingTime}ms", true);

            await invokator.CreateMessageAsync(embed);
        }
        #endregion
    }
}
