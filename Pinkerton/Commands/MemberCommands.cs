using Guilded.Base;
using Guilded.Base.Embeds;
using Guilded.Commands;
using Guilded.Permissions;
using Guilded.Servers;
using Humanizer;
using Humanizer.Localisation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Pinkerton.Factories;
using Pinkerton.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Common;

namespace Pinkerton.Commands
{
    public class MemberCommands: CommandModule
    {
        private PinkertonDbContextFactory _dbfactory = new();
        private static readonly string? timePattern = "hh : mm : ss tt";

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

            try
            {
                await invokator.CreateMessageAsync(embed);
            }
            catch(Exception e)
            {
                await invokator.ReplyAsync("A{I error : bad gateway");
            }
            
        }
        #endregion

        #region BOT INFO
        [Command(Aliases = [ "botinfo" ])]
        [Description("get the bot information")]
        public async Task BotInfo(CommandEvent invokator)
        {
            using var db = _dbfactory.CreateDbContext();
            var embed = new Embed();
            try
            {

                var serverId = invokator.ServerId;
                var botId = invokator.ParentClient.Id;
                var servers = db.Servers.ToList();
                var serverCount = servers.Count();
                var bot = await invokator.ParentClient.GetMemberAsync((HashId)serverId!, (HashId)botId!);
                var status = bot.User.Status!.Content ?? "not set";
                embed.SetTitle("Pinkerton Info");
                embed.AddField("Creator", "<@mq1ezklm>", true);
                embed.AddField("Created At", $"{bot.CreatedAt.ToString(timePattern)}", true);
                embed.AddField("Servers", serverCount, true);
                embed.AddField("Prefix", "p?", true);
                embed.AddField("Status", $"{status}", true);

                embed.SetDescription($"[Add Pinkerton](https://www.guilded.gg/b/62e8141a-2ec7-4807-9a5d-addfb6d735b1)");
                await invokator.ReplyAsync(embed);
            }
            catch(Exception e)
            {
                await invokator.ReplyAsync("unable to retrieve bot info at this time.");
            }
            


        }
        #endregion

        #region USERINFO
        [Command(Aliases = [ "userinfo", "whois", "lookup" ])]
        [Description("get the mentioned user info")]
        public async Task UserInfo(CommandEvent invokator, Member? member = null)
        {
            var serverId = invokator.ServerId;
            var userId = invokator.Message.CreatedBy;
            using var memService = new MemberProviderService();
            var embed = new Embed();
            try
            {
                using var db = _dbfactory.CreateDbContext();
                if (member is null)
                {
                    
                    var _member = await invokator.ParentClient.GetMemberAsync((HashId)serverId!, userId);
                    var roles = await memService.GetMemberRolesAsync(invokator.ParentClient, (HashId)serverId!, userId);
                    var serverMember = db.Members
                        .Where(x => x.ServerId!.Equals(serverId.ToString())
                        && x.MemberId!.Equals(_member.Id.ToString()))
                        .Include(x => x.Infractions)
                        .FirstOrDefault();
                    if (serverMember is not null)
                    {
                        var infractions = new StringBuilder();
                        var count = 1;
                        foreach (var i in serverMember.Infractions!)
                        {
                            infractions.Append($"{count}. {i.Reason}\r\n");
                            count++;
                        }
                        var warnings = serverMember.Warnings;
                        var createdAt = _member.CreatedAt;
                        var joinedAt = _member.JoinedAt;
                       
                        var newJoinedAt = $"{DateTimeOffset.UtcNow.Subtract(joinedAt).Humanize(2, minUnit: TimeUnit.Day, maxUnit: TimeUnit.Year)}";
                        var newCreatedAt = $"{DateTimeOffset.UtcNow.Subtract(createdAt).Humanize(2, minUnit: TimeUnit.Day, maxUnit: TimeUnit.Year)}";
                        embed.SetTitle("USER INFO");
                        embed.SetDescription($"info for <@{serverMember.MemberId}>(`{_member.Id}`) requested by <@{userId}>(`{userId}`)");
                        embed.SetThumbnail(new EmbedMedia(_member.Avatar!.AbsoluteUri));
                        embed.SetColor(Color.DarkSeaGreen);
                        embed.AddField("Username", serverMember.Name!, true);
                        embed.AddField("Joined", $"{newJoinedAt} ago", true);
                        embed.AddField("Created", $"{newCreatedAt} ago", true);
                        var rs = new StringBuilder();
                        foreach (var r in member!.RoleIds)
                        {
                            rs.Append($"<@{r}> ");
                        }
                        embed.AddField("Roles", rs, true);
                        embed.AddField("Warnings", warnings, true);
                        embed.AddField("Infractions", infractions, true);
                        embed.SetFooter("Pinkerton always watching ");
                        embed.SetTimestamp(DateTime.Now);

                        await invokator.ReplyAsync(embed);

                        //serverMember.Roles = roles.Value;
                        //db.Members.Update(serverMember);
                        //await db.SaveChangesAsync();

                    }
                    else
                    {
                        await invokator.ReplyAsync($"I could not find user info for {member!.Name}");
                    }
                }
                else
                {
                    var roles = await memService.GetMemberRolesAsync(invokator.ParentClient, (HashId)serverId!, member.Id);
                    var newRoles = string.Join(",", roles.Value);

                    var serverMember = db.Members
                                         .Where(x => x.ServerId!.Equals(serverId.ToString())
                                         && x.MemberId!.Equals(member.Id.ToString()))
                                         .Include(x => x.Infractions)
                                         .FirstOrDefault();
                    if (serverMember is not null)
                    {
                        var infractions = new StringBuilder();
                        var count = 1;
                        foreach (var i in serverMember.Infractions!)
                        {
                            infractions.Append($"{count}. {i.Reason}\r\n");
                            count++;
                        }
                        var warnings = serverMember.Warnings;
                        var createdAt = member.CreatedAt;
                        var joinedAt = member.JoinedAt;
                        //var rs = string.Join(", ", serverMember.Roles!);
                        var newJoinedAt = $"{DateTimeOffset.UtcNow.Subtract(joinedAt).Humanize(2, minUnit: TimeUnit.Day, maxUnit: TimeUnit.Year)}";
                        var newCreatedAt = $"{DateTimeOffset.UtcNow.Subtract(createdAt).Humanize(2, minUnit: TimeUnit.Day, maxUnit: TimeUnit.Year)}";
                        embed.SetTitle("USER INFO");
                        embed.SetDescription($"info for <@{serverMember.MemberId}>(`{member.Id}`) requested by <@{userId}>(`{userId}`)");
                        embed.SetThumbnail(new EmbedMedia(member.Avatar!.AbsoluteUri));
                        embed.SetColor(Color.DarkSeaGreen);
                        embed.AddField("Username", serverMember.Name!, true);
                        embed.AddField("Joined", $"{newJoinedAt} ago", true);
                        embed.AddField("Created", $"{newCreatedAt} ago", true);
                        var rs = new StringBuilder();
                        foreach (var r in member!.RoleIds)
                        {
                            rs.Append($"<@{r.ToString()}> ");
                        }
                        embed.AddField("Roles", rs, true);
                        embed.AddField("Warnings", warnings, true);
                        embed.AddField("Infractions", infractions, true);
                        embed.SetFooter("Pinkerton always watching ");
                        embed.SetTimestamp(DateTime.Now);

                        await invokator.ReplyAsync(embed);

                        //serverMember.Roles = roles.Value;
                        //db.Members.Update(serverMember);
                        //await db.SaveChangesAsync();

                    }
                    else
                    {
                        var createdAt = member.CreatedAt;
                        var joinedAt = member.JoinedAt;
                        //var rs = string.Join(", ", serverMember.Roles!);
                        var newJoinedAt = $"{DateTimeOffset.UtcNow.Subtract(joinedAt).Humanize(2, minUnit: TimeUnit.Day, maxUnit: TimeUnit.Year)}";
                        var newCreatedAt = $"{DateTimeOffset.UtcNow.Subtract(createdAt).Humanize(2, minUnit: TimeUnit.Day, maxUnit: TimeUnit.Year)}";
                        embed.SetTitle("USER INFO");
                        embed.SetDescription($"info for <@{member.Id}>(`{member.Id}`) requested by <@{userId}>(`{userId}`)");
                        embed.SetThumbnail(new EmbedMedia(member.Avatar!.AbsoluteUri));
                        embed.AddField("Username", member.Name!, true);
                        embed.AddField("Joined", newJoinedAt, true);
                        embed.AddField("Account Created", newCreatedAt, true);
                        var rs = new StringBuilder();
                        foreach (var r in member!.RoleIds)
                        {
                            rs.Append($"<@{r}> ");
                        }
                        embed.AddField("Roles", rs, true);
                        await invokator.ReplyAsync(embed);
                    } 
                    
                }
                
            }
            catch(Exception e)
            {
                await invokator.ReplyAsync($"{e.Message}");
            }
        }
        #endregion

        #region COMMAND LIST

        #endregion

        #region PING
        [Command(Aliases = [ "ping" ])]
        [Description("ping the Guilded server to time the response")]
        public async Task Ping(CommandEvent invokator)
        {
            var sw = new Stopwatch();
            var db = _dbfactory.CreateDbContext();
            sw.Start();
            
            var mems = db.Members.Where(x => x.MemberId!.Equals(invokator.Message.CreatedBy.ToString())).FirstOrDefault();
            var msg = await invokator.ReplyAsync("PONG!");

            sw.Stop();
            await Task.Delay(250);

            var embed = new Embed();
            var duration = sw.Elapsed;
            await msg.UpdateAsync($"PONG! took {duration.Milliseconds} ms");
        }
        #endregion

        #region HELP
        [Command(Aliases = [ "help", "h" ])]
        [Description("list of commands for Pinkerton")]
        public async Task Help(CommandEvent invokator, string? helpArg = null)
        {
            if (helpArg is null)
            {
                var embed = new Embed();
                embed.SetDescription("I can not help if I don't know what you need help with, please provide a help arg\r\n```ex:\rp?help vfw```\rthis will" +
                    "explain how to use the command to view filtered words list.");
                embed.SetColor(Color.DarkRed);
                embed.SetFooter("Pinkerton watching everything ");
                embed.SetTimestamp(DateTime.Now);
                await invokator.ReplyAsync(embed);

            }
            else
            {
                var embed = new Embed();
                switch (helpArg)
                {
                    case "list":
                        //TODO: work on list commands
                        embed.SetTitle("COMMAND LIST");
                        embed.SetDescription("**MEMBER COMMANDS**\r\n" +
                                             "Uptime - get the time Pinkerton has been online\r\n" +
                                             "Bot Info - get the bot information\r\n" +
                                             "User Info - get a specific member's information\r\n" +
                                             "Support Ticket - (wip) generate a support ticket\r\n\r\n" +
                                             "MOD COMMANDS\r\n" +
                                             "Warn - warn a member\r\n" +
                                             "mute - mute a member for a set amount of time\r\n" +
                                             "ban - ban a mentioned member from the server\r\n" +
                                             "slc - set log channel\r\n" +
                                             "vfw - view filter words list\r\n" +
                                             "rfw - remove a word from the filtered word list\r\n" +
                                             "purge - delete messages from chat\r\n" +
                                             "error lookup - get error code description\r\n" +
                                             "```there is a secret code to gain XP, can you find it?```");
                        embed.SetColor(Color.SteelBlue);
                        embed.SetFooter("Pinkerton watching everything ");
                        embed.SetTimestamp(DateTime.Now);
                        await invokator.ReplyAsync(embed);
                        break;
                    case "sfw":
                        embed.SetTitle($"{helpArg.ToUpper()} COMMAND USAGE");
                        embed.SetDescription("`p?sfw <words>`\r\nthis will add a single word or multiple words to the filtered word list\r\r" +
                            "if Pinkerton detects the usage of any word in the list the message author will get an infraction.\r\n`5 infractions = 1 warning`\r\n" +
                            "on the members 4th warning they will be kicked from the server.\r\n\r\n" +
                            "\r\nexample usage```p?sfw hello howdy hi```");
                        embed.SetColor(Color.SteelBlue);
                        embed.SetFooter("Pinkerton watching everything ");
                        embed.SetTimestamp(DateTime.Now);
                        await invokator.ReplyAsync(embed);
                        break;
                    case "vfw":
                        embed.SetTitle($"{helpArg.ToUpper()} COMMAND USAGE");
                        embed.SetDescription("`p?vfw`\r\nthis will show all the filtered words in the list\r\neach page will have 10 results.\r\n" +
                            "example usage```p?vfw```\r\nif there are multiple pages```p?vfw 2```");
                        embed.SetFooter("Pinkerton watching everything ");
                        embed.SetTimestamp(DateTime.Now);
                        await invokator.ReplyAsync(embed);
                        break;
                    case "slc":
                        embed.SetTitle($"{helpArg.ToUpper()} COMMAND USAGE");
                        embed.SetDescription("`p?slc`\r\nthis will set the server's log channel\r\n" +
                            "Pinkerton will send log messages to this channel. if no log channel is set, Pinkerton will use the server's default channel.\r\n\r\n" +
                            "\r\nexample usage```p?slc <channel ID>```" +
                            "\r\nto get a channels Id, right click on the channel name - copy channel Id");
                        embed.SetFooter("Pinkerton watching everything ");
                        embed.SetTimestamp(DateTime.Now);
                        await invokator.ReplyAsync(embed);
                        break;
                    case "mute":
                        embed.SetTitle($"{helpArg.ToUpper()} USAGE");
                        embed.SetDescription("WIP(Work in Progress)");
                        embed.SetFooter("Pinkerton watching everything ");
                        embed.SetTimestamp(DateTime.Now);
                        await invokator.ReplyAsync(embed);
                        break;
                    case "xp":
                        embed.SetTitle($"{helpArg.ToUpper()} USAGE");
                        embed.SetDescription("there is a secret code to gain 1000 xp\r\n" +
                            "hint: combine these two phrases to make a single phrase\r\n" +
                            "```heavy is the hand that wears the crown``` ```the creator of Pinkerton```");
                        embed.SetFooter("Pinkerton watching everything ");
                        embed.SetTimestamp(DateTime.UtcNow);
                        await invokator.ReplyAsync(embed);
                        break;
                    default:
                        embed.SetTitle($"❌ UNRECOGNIZED COMMAND ARG ❌");
                        embed.SetDescription("I don't recognize this command, for a list of commands use `p?help list`");
                        embed.SetFooter("Pinkerton watching everything ");
                        embed.SetTimestamp(DateTime.Now);
                        await invokator.ReplyAsync(embed);
                        break;
                }
                
                
            }
           
        }
        #endregion

        #region CREATE SUPPORT TICKET
        [Command(Aliases = [ "support", "st" ])]
        [Description("generates a support thread wher emoderation can help with a server issue")]
        public async Task SupportTicket(CommandEvent invokator, string[]? reason = null)
        {
            var serverId = invokator.ServerId;
            var authorId = invokator.Message.CreatedBy;
            var embed = new Embed();
            string? _reason = null;
            try
            {
                if (reason is not null)
                     _reason = string.Join(" ", reason);

                var botId = invokator.ParentClient.Id;
                var bot = await invokator.ParentClient.GetMemberAsync((HashId)serverId!, (HashId)botId!);
                var author = await invokator.ParentClient.GetMemberAsync((HashId)serverId!, authorId);
                if (bot is null)
                    await invokator.ReplyAsync("I'm sorry , I cannot execute this command!");
                else
                {
                    var permissions = await invokator.ParentClient.GetMemberPermissionsAsync((HashId)serverId!, (HashId)botId!);
                    if (!permissions.Contains(Permission.CreateThreads))
                    {
                        embed.SetTitle("❌ PERMISSION DENIED ❌");
                        embed.SetDescription("I do not have the permissions needed to create threads, have the server admin grant me the permissions [CanCreateThreads] | [CanCreateThreadMessages]");
                        embed.SetColor(Color.BlueViolet);
                        embed.SetFooter("Pinkerton watching everything ");
                        embed.SetTimestamp(DateTime.Now);
                        await invokator.ReplyAsync(embed);
                    }
                    else
                    {
                        var category = await invokator.ParentClient.CreateCategoryAsync((HashId)serverId!, "Support");
                        var channel = await invokator.ParentClient.CreateChannelAsync((HashId)serverId!, "support", Guilded.Servers.ChannelType.Chat, null, null, null, category.Id, null, null);
                        await author.AddRoleAsync(37516083);
                        await invokator.ParentClient.CreateMessageAsync(channel.Id, $"this channel was created for {_reason}");

                    }
                }
            }
            catch(Exception e)
            {
                await invokator.ReplyAsync("unable to create channel due to missing permissions.");
            }
        }
        #endregion

        #region VERIFY YOUTUBE
        [Command(Aliases = [ "verify" ])]
        [Description("verify that a member has a youtube channel")]
        public async Task Verify(CommandEvent invokator, Member? member, string channelLink, string id)
        {
            var youTube = new YoutubeClient();
            //var videos = await youTube.Videos.GetAsync(channelLink);
            var channel = youTube.Search.GetChannelsAsync(channelLink).GetAwaiter().GetResult();
            
           // var author = channel.Where(x => x.Id.Equals(id));
            if (channel is not null)
                await invokator.ReplyAsync("member is verified!");
            else
                await invokator.ReplyAsync("member is NOT verified!");
        }
        #endregion


    }
}
