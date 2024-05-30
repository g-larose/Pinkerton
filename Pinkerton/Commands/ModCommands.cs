using Guilded.Base;
using Guilded.Base.Embeds;
using Guilded.Commands;
using Guilded.Permissions;
using Guilded.Servers;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Pinkerton.Factories;
using Pinkerton.Models;
using Pinkerton.Paginator;
using Pinkerton.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinkerton.Commands
{
    public class ModCommands: CommandModule
    {
        private PinkertonDbContextFactory _dbFactory = new();

        #region PURGE
        [Command(Aliases = ["purge", "remove", "delete", "clean"])]
        [Description("deletes a set amount of messages from the channel")]
        public async Task Purge(CommandEvent invokator, uint amount = 0)
        {
            try
            {
                var authorId = invokator.Message.CreatedBy;
                var serverId = invokator.ServerId;
                var author = await invokator.ParentClient.GetMemberAsync((HashId)serverId!, authorId);
                var permissions = await invokator.ParentClient.GetMemberPermissionsAsync((HashId)serverId!, authorId);

                if (permissions.Contains(Permission.ManageMessages))
                {
                    if (amount < 1 || amount > 99)
                    {
                        amount = 10;
                        var embed = new Embed();
                        var emotes = new uint[] { 2278387, 2278386 };
                        embed.SetDescription("no amount was specified, default amount is set to 99\r\n" +
                            "would you like to proceed");
                        var warningMsg = await invokator.ReplyAsync(embed);
                        foreach (var emote in emotes)
                        {
                            await warningMsg.AddReactionAsync(emote);
                        }

                        invokator.ParentClient.MessageReactionAdded
                                     .Where(e => e.CreatedBy == invokator.CreatedBy)
                                     .Subscribe(async reaction =>
                                     {

                                         if (reaction.Name == "check-mark")
                                         {
                                             if (permissions.Contains(Permission.ManageMessages))
                                             {
                                                 var messages = await invokator.ParentClient.GetMessagesAsync(invokator.ChannelId, false, amount);
                                                 foreach (var msg in messages)
                                                 {
                                                     await msg.DeleteAsync();
                                                     await Task.Delay(100);
                                                 }
                                                 embed.SetDescription($"<@{authorId}> used the purge command and deleted {amount} messages");
                                                 embed.SetFooter("Pinkerton ");
                                                 embed.SetTimestamp(DateTime.Now);
                                                 await invokator.CreateMessageAsync(embed);
                                             }
                                             
                                         }
                                         else
                                         {
                                             embed.SetTitle("❌command canceled❌");
                                             embed.SetDescription($"<@{authorId}> canceled the purge command");
                                             embed.SetThumbnail(new EmbedMedia("https://cdn.gilcdn.com/MediaChannelUpload/c8a5997177bb399a00120fafb60a52ad-Full.webp?w=160&h=160"));
                                             embed.SetFooter("Pinkerton ");
                                             embed.SetTimestamp(DateTime.Now);
                                             await invokator.CreateMessageAsync(embed);
                                             return;
                                         }
                                     });

                    }
                    else
                    {
                        var embed = new Embed();
                        var messages = await invokator.ParentClient.GetMessagesAsync(invokator.ChannelId, false, amount);
                        foreach (var msg in messages)
                        {
                            await msg.DeleteAsync();
                            await Task.Delay(100);
                        }
                        embed.SetDescription($"<@{authorId}> used the purge command and deleted {amount} messages");
                        embed.SetFooter("Pinkerton ");
                        embed.SetTimestamp(DateTime.Now);
                        await invokator.CreateMessageAsync(embed);
                    }

                }
                else
                {
                    await invokator.ReplyAsync($"{author.Name} you do not have the correct permissions to perform this command, command ignored");
                }
            }
            catch (Exception e)
            {
                var test = e.Message;
            }
            
        }
        #endregion

        #region ADD WARNING
        [Command(Aliases = [ "warn" ])]
        [Description("warns a member")]
        public async Task WarnMember(CommandEvent invokator, Member? member = null, string[]? reason = null)
        {
            ModCommandProviderService modService = new(_dbFactory, invokator.ParentClient);
            var serverId = invokator.ServerId;
            var cmdAuthorId = invokator.Message.CreatedBy;
            Server? server = null;
            var embed = new Embed();
            try
           {
                var cmdAuthor = await invokator.ParentClient.GetMemberAsync((HashId)serverId!, cmdAuthorId);
                var permissions = await invokator.ParentClient.GetMemberPermissionsAsync((HashId)serverId!, cmdAuthorId);

                if (!permissions.Contains(Permission.ManageChannels))
                {
                    await invokator.ReplyAsync("you do not have the correct permissions to execute this command");
                }
                else
                {
                    if (member is null || reason is null)
                    {
                        embed.SetTitle("❓ MISSING ARGS ❓");
                        embed.SetThumbnail(new EmbedMedia("https://t.ly/bKG1L"));
                        embed.SetDescription("I was expecting <member> <reason> format but didn't receive that!\r" +
                            "please provide all arguments so I can execute the command [warn]");
                        embed.SetFooter("MODiX watching everything ");
                        embed.SetTimestamp(DateTime.Now);
                        await invokator.ReplyAsync(embed);
                    }
                    else
                    {
                        var _reason = string.Join(" ", reason);
                        var warningResult = await modService.AddWarningAsync(serverId.ToString()!, member.Id.ToString(), _reason);
                        server = await invokator.ParentClient.GetServerAsync((HashId)serverId!);
                        if (warningResult.IsOk)
                        {
                            embed.SetTitle("⚠ WARNING ⚠");
                            embed.SetDescription($"<@{member.Id}> this is a warning for `{_reason}`, on your 4th warning you will be kicked from {server.Name}");
                            await invokator.CreateMessageAsync(true, false, embed);
                            var newEmbed = new Embed();
                            newEmbed.SetDescription($"<@{cmdAuthorId}> ✔ SUCCESS ✔ warning added to {member.Name} succesfully!");
                            await invokator.ReplyAsync(true, false, newEmbed);
                        }
                        else
                        {
                            embed.SetTitle("❌ FAILURE ❌");
                            embed.SetThumbnail(new EmbedMedia("https://t.ly/bKG1L"));
                            embed.SetDescription($"Failed to add warning {warningResult.Error.ErrorMessage} : please refer `{warningResult.Error.ErrorCode}` for further review.");
                            embed.SetFooter("MODiX watching everything ");
                            embed.SetTimestamp(DateTime.Now);
                            await invokator.ReplyAsync(true , false, embed);
                        }

                    }
                }
            }
            catch(Exception e)
            {
                server = await invokator.ParentClient.GetServerAsync((HashId)serverId!);
                var error = new SystemError()
                {
                    ErrorCode = SystemErrors.GetError(e.Message, Guid.NewGuid()),
                    ErrorMessage = e.Message,
                    ServerId = serverId.ToString(),
                    ServerName = server!.Name 
                };
                embed.SetTitle("❌ FAILURE ❌");
                embed.SetThumbnail(new EmbedMedia("https://tinyurl.com/ycketxyh"));
                embed.SetDescription($"Failed to add warning {error.ErrorMessage} : please refer `{error.ErrorCode}` for further review.");
                embed.SetFooter("MODiX watching everything ");
                embed.SetTimestamp(DateTime.Now);
                await invokator.ReplyAsync(true, false, embed);

                using var db = _dbFactory.CreateDbContext();
                db.Errors.Add(error);
                await db.SaveChangesAsync();
            }
        }
        #endregion

        #region REMOVE WARNING
        [Command(Aliases = [ "removememberwarning", "rmw" ])]
        [Description("remove a member's warning by ID")]
        public async Task RemoveMemberWarning(CommandEvent invokator, Member? member= null)
        {
            var embed = new Embed();
            var serverId = invokator.ServerId;
            var authorId = invokator.Message.CreatedBy;
            if (member is null)
            {
                embed.SetTitle("❌ MISSING ARGUMENTS ❌");
                embed.SetDescription("I was expecting a mentioned member but found nothig.\r" +
                    "example command `p?rmw @member`");
                embed.SetFooter("Pinkerton ");
                embed.SetTimestamp(DateTime.Now);
                await invokator.ReplyAsync(embed);
            }
            else
            {
                try
                {
                    var modService = new ModCommandProviderService(_dbFactory, invokator.ParentClient);
                    var result = modService.RemoveWarning(serverId.ToString()!, member.Id.ToString());
                    if (result.IsOk)
                    {
                        embed.SetTitle("👍 SUCCESS 👍");
                        embed.SetDescription($"<@{authorId}> warning removed successfully!");
                        embed.SetFooter("Pinkerton watching everything ");
                        embed.SetTimestamp(DateTime.Now);
                        await invokator.ReplyAsync(true, false, embed);

                        var memEmbed = new Embed();
                        memEmbed.SetTitle("👍 SUCCESS 👍");
                        memEmbed.SetDescription($"<@{member.Id}> a warning was removed successfully!");
                        memEmbed.SetFooter("Pinkerton watching everything ");
                        memEmbed.SetTimestamp(DateTime.Now);
                        await member.ParentClient.CreateMessageAsync((Guid)invokator.Message.ChannelId, memEmbed);
                    }
                    else
                    {
                        embed.SetTitle("❌ FAILURE ❌");
                        embed.SetDescription($"<@{authorId}> {result.Error.ErrorMessage} Code: `{result.Error.ErrorCode}`");
                        embed.SetFooter("Pinkerton watching everything ");
                        embed.SetTimestamp(DateTime.Now);
                        await invokator.ReplyAsync(true, false, embed);
                    }
                }
                catch(Exception e)
                {
                    var error = new SystemError()
                    {
                        ErrorCode = SystemErrors.GetError(e.Message, Guid.NewGuid()),
                        ErrorMessage = e.Message,
                        ServerId = serverId.ToString()
                    };
                    embed.SetTitle("❌ FAILURE ❌");
                    embed.SetDescription($"<@{authorId}> {error.ErrorMessage} : `{error.ErrorCode}`");
                    embed.SetFooter("Pinkerton ");
                    embed.SetTimestamp(DateTime.Now);
                    await invokator.ReplyAsync(true, false, embed);

                }
            }
        }
        #endregion

        #region VIEW MEMBER WARNINGS
        [Command(Aliases = [ "viewmemberinfractions", "vmi" ])]
        [Description("view a list of member infractions")]
        public async Task ViewInfractions(CommandEvent invokator, Member? member = null)
        {
            var embed = new Embed();
            var authorId = invokator.Message.CreatedBy;
            var serverId = invokator.ServerId;
            if (member is null)
            {
                
                embed.SetTitle("❌ Failure ❌");
                embed.SetDescription($"<@{authorId}> you did not provide a member for me to lookup.");
                embed.SetFooter("Pinkerton ");
                embed.SetTimestamp(DateTime.Now);
                await invokator.ReplyAsync(embed);
            }
            else
            {
                ModCommandProviderService modService = new(_dbFactory, invokator.ParentClient);
                var infractions = modService.GetMemberInfractions(serverId.ToString()!, member.Id.ToString());

                if (infractions.IsOk)
                {
                    Paginator<Infraction> paginator = new Paginator<Infraction>();
                    List<List<Infraction>> paginatedList = paginator.Paginate(infractions.Value, 10);
                    var builder = new StringBuilder();
                    var count = 1;
                    foreach (var item in paginatedList)
                    {
                        foreach (var inf in item)
                        {
                            builder.Append($"{count}. {inf.Reason} - ID: {inf.Identifier}\r\n");
                            count++;
                        }
                        
                    }
                    embed.SetTitle("Infraction List");
                    embed.SetDescription(builder.ToString());
                    embed.SetFooter("Pinkerton ");
                    embed.SetTimestamp(DateTime.Now);
                    await invokator.ReplyAsync(embed);
                }
                else
                {
                    embed.SetTitle("❌ FAILURE ❌");
                    embed.SetDescription(infractions.Error.ErrorMessage!);
                    embed.SetFooter("Pinkerton ");
                    embed.SetTimestamp(DateTime.Now);
                    await invokator.ReplyAsync(embed);
                }
            }
        }
        #endregion

        #region SET FILTERED WORDS
        [Command(Aliases = [ "setfilteredwords", "sfw" ])]
        [Description("set's or add's to the filtered words list | permissions - PERMISSION.MANAGECHANNEL")]
        public async Task SetFilteredWords(CommandEvent invokator, string[]? words = null)
        {
            using var modCommandProvider = new ModCommandProviderService(_dbFactory, invokator.ParentClient);
            var serverId = invokator.ServerId;
            try
            {
                using var db = _dbFactory.CreateDbContext();
                var server = await invokator.ParentClient.GetServerAsync((HashId)serverId!);
                if (words is null)
                    await invokator.ReplyAsync("I was expecting a list of words to add to the filtered words list, found []");
                else
                {
                    var _server = db.Servers.Where(x => x.ServerId!.Equals(serverId.ToString()))
                                            .FirstOrDefault();
                    if (_server is not null)
                    {
                        var result = await modCommandProvider.SetFilteredWordsAsync(serverId.ToString()!, [.. words]);
                        if (result.IsOk)
                            await invokator.ReplyAsync("Success! word filter updated.");
                        else
                        {
                            await invokator.ReplyAsync($"word filter not set : {result.Error.ErrorMessage} : please refer `{result.Error.ErrorCode}` to a moderator for further review");
                        }
                    }
                    else
                    {
                        var newServer = new ServerConfig()
                        {
                            ServerId = serverId.ToString(),
                            CreatedAt = server.CreatedAt,
                            DefaultChannelId = server.DefaultChannelId,
                            FilteredWords = [],
                            Messages = null,
                            OwnerId = server.OwnerId.ToString(),
                            ServerName = server.Name,
                        };
                        db.Servers.Add(newServer);
                        await db.SaveChangesAsync();

                        var result = await modCommandProvider.SetFilteredWordsAsync(serverId.ToString()!, [.. words]);
                        if (result.IsOk)
                            await invokator.ReplyAsync("Success! word filter updated.");
                        else
                        {
                            await invokator.ReplyAsync($"word filter not set : {result.Error.ErrorMessage} : please refer `{result.Error.ErrorCode}` to a moderator for further review");
                        }
                    }
                }
            }
            catch(Exception e)
            {
                await invokator.ReplyAsync($"word filter not set : {e.Message}");
            }
           
        }
        #endregion

        #region REMOVE FILTERED WORDS
        [Command(Aliases = [ "removefilteredwords", "rfw" ])]
        [Description("removes words to the server's filtered words list")]
        public async Task RemoveFilteredWords(CommandEvent invokator, string[]? words = null)
        {
            var serverId = invokator.ServerId;
            var memberId = invokator.Message.CreatedBy;

            try
            {
                
                if (words is not null)
                {
                    var permissions = await invokator.ParentClient.GetMemberPermissionsAsync((HashId)serverId!, memberId);

                    if (permissions.Contains(Permission.ManageServer))
                    {
                        using var db = _dbFactory.CreateDbContext();
                        var filteredWords = db.Servers.Where(x => x.ServerId!.Equals(serverId.ToString()))
                            .Select(x => x.FilteredWords)
                            .FirstOrDefault();
                        var index = 0;
                        var newArray = new List<string>();
                        if (filteredWords is not null)
                        {
                            var removed = false;
 
                            foreach (var rWord in filteredWords)
                            {
                                if (!words.Contains(rWord))
                                {
                                    newArray.Add(rWord);
                                    removed = true;
                                    index++;
                                }
                                else
                                {
                                    index++;
                                }
                                       
                            }

                            if (removed)
                            {
                                var s = db.Servers.Where(x => x.ServerId!.Equals(serverId.ToString())).FirstOrDefault();
                                if (s is not null)
                                {
                                    s.FilteredWords = newArray.ToArray();
                                    db.Servers.Update(s);
                                    await db.SaveChangesAsync();
                                    await invokator.ReplyAsync("command executed successfully, all words were removed from the filtered word list.");
                                }
                            }
                               
                        }
                    }
                    else
                        await invokator.ReplyAsync("you do not have the correct permission to execute this command - PERMMISION | `[ MANAGESERVER ]`");
                    
                }
                else
                    await invokator.ReplyAsync("no word to remove was provided, command ignored!");
                
            }
            catch(Exception e)
            {
                await invokator.ReplyAsync(e.Message);
            }
        }
        #endregion

        #region SET FILTERED WORDS ENABLED
        [Command(Aliases = [ "enablefilteredwords", "efw" ])]
        [Description("enables filtered words - if true all words in the banned list will be flagged and removed.")]
        public async Task EnableFilteredWords(CommandEvent invokator, bool enable = false)
        {
            var serverId = invokator.ServerId;
            var memberId = invokator.Message.CreatedBy;

            try
            {
                using var db = _dbFactory.CreateDbContext();
                var server = db.Servers.Where(x => x.ServerId!.Equals(serverId.ToString())).FirstOrDefault();
                server!.IsFilterEnabled = enable;
                db.Servers.Update(server);
                await db.SaveChangesAsync();
                await invokator.ReplyAsync($"filter mode set to: {enable}");
            }
            catch(Exception e)
            {
                await invokator.ReplyAsync($"{e.Message}");
            }
        }
        #endregion

        #region VIEW WORD FILTER LIST
        [Command(Aliases = [ "viewfilteredwords", "vfw" ])]
        [Description("shows the filtered word list for the server.")]
        public async Task ShowFilteredWords(CommandEvent invokator, int page = 1)
        {
            var serverId = invokator.ServerId;
            var memberId = invokator.Message.CreatedBy;
            Server? server = null;
            var embed = new Embed();

            try
            {
                using var modCommandProvider = new ModCommandProviderService(_dbFactory, invokator.ParentClient);
                var member = await invokator.ParentClient.GetMemberAsync((HashId)serverId!, memberId);
                server = await invokator.ParentClient.GetServerAsync((HashId)serverId!);
                var perms = await invokator.ParentClient.GetMemberPermissionsAsync((HashId)serverId!, memberId);

                if (!perms.Contains(Permission.ManageServer))
                {
                    embed.SetTitle("❌ Incorrect Permissions ❌");
                    embed.SetDescription($"<@{memberId}> you don't have the correct permissions to execute this command.");
                    embed.SetThumbnail(new EmbedMedia("https://rb.gy/wuds7l"));
                    embed.SetFooter("Pinkerton securing ");
                    embed.SetTimestamp(DateTime.Now);
                    await invokator.ReplyAsync(embed);
                }
                else
                {
                    var result = await modCommandProvider.GetServerFilteredWords(serverId.ToString()!);
                    if (result.IsOk)
                    {
                        Paginator<string> paginator = new Paginator<string>();
                        List<List<string>> paginatedList = paginator.Paginate(result.Value, 10);
                        var listBuilder = new StringBuilder();
                        var count = 1;
                        foreach (var item in paginatedList[page - 1])
                        {
                            listBuilder.Append($"{count}. {item}\r\n");
                            count++;
                        }
                        embed.SetTitle("Filtered Words");
                        embed.SetDescription(listBuilder.ToString());
                        embed.SetFooter($"viewing page {page} of {paginatedList.Count} ");
                        embed.SetTimestamp(DateTime.Now);
                        await invokator.ReplyAsync(embed);

                    }
                    else
                    {
                        var error = new SystemError()
                        {
                            ErrorCode = SystemErrors.GetError("List Empty", Guid.NewGuid()),
                            ErrorMessage = "the list was empty.",
                            ServerId = serverId.ToString(),
                            ServerName = server.Name
                        };
                        embed.SetTitle("❌ System Error ❌");
                        embed.SetDescription($"{error.ErrorMessage} : please refer the error code below to a moderator for further review.\r\n" +
                            $"`{error.ErrorCode}`\r\n\r\nto set the filtered word list run command\r\n" +
                            "`p?sfw <words>`");
                        embed.SetThumbnail(new EmbedMedia("https://tinyurl.com/ycketxyh"));
                        embed.SetFooter("Pinkerton securing ");
                        embed.SetTimestamp(DateTime.Now);
                        await invokator.ReplyAsync(embed);
                    }
                }
                
            }
            catch(Exception e)
            {
                var error = new SystemError()
                {
                    ErrorCode = SystemErrors.GetError(e.Message, Guid.NewGuid()),
                    ErrorMessage = e.Message,
                    ServerId = serverId.ToString(),
                    ServerName = server!.Name
                };
                embed.SetTitle("❌ System Error ❌");
                embed.SetDescription($"{error.ErrorMessage} : please refer `{error.ErrorCode}` to a moderator for further review.");
                embed.SetThumbnail(new EmbedMedia("https://tinyurl.com/ycketxyh"));
                embed.SetFooter("Pinkerton securing ");
                embed.SetTimestamp(DateTime.Now);
                await invokator.ReplyAsync(embed);
            }
        }
        #endregion

        #region SET LOG CHANNEL
        [Command(Aliases = [ "setlogchannel", "slc" ])]
        [Description("set the server log channel - PERMISSIONS [ PERMISSION.MANAGESERVER ]")]
        public async Task SetLogChannel(CommandEvent invokator, Guid? id = null)
        {
            var serverId = invokator.ServerId;
            var memberId = invokator.Message.CreatedBy;
            Server? server = null;
            using var modProvider = new ModCommandProviderService(_dbFactory, invokator.ParentClient);
            var embed = new Embed();
            try
            {
                if (id is not null)
                {
                    var result = await modProvider.SetServerLogChannelIdAsync(serverId.ToString()!, (Guid)id!);
                    if (result.IsOk)
                        await invokator.ReplyAsync("Log Channel Id set successfully!");
                    else
                    {
                        embed.SetTitle("❌ Unable To Set LogChannel ❌");
                        embed.SetDescription($"{result.Error.ErrorMessage} : refer `{result.Error.ErrorCode}` to a moderator for further review.");
                        embed.SetFooter("Pinkerton securing ");
                        embed.SetTimestamp(DateTime.Now);
                        await invokator.ReplyAsync(embed);
                    }
                }
                else
                {
                    embed.SetTitle("❌ Unable To Set LogChannel ❌");
                    embed.SetDescription("No log channel Id provided!");
                    embed.SetFooter("Pinkerton securing ");
                    embed.SetTimestamp(DateTime.Now);
                    await invokator.ReplyAsync(embed);
                }
            }
            catch(Exception e)
            {
                server = await invokator.ParentClient.GetServerAsync((HashId)serverId!);
                var error = new SystemError()
                {
                    ErrorCode = SystemErrors.GetError(e.Message, Guid.NewGuid()),
                    ErrorMessage = e.Message,
                    ServerId = serverId.ToString(), 
                    ServerName = server!.Name
                };
                embed.SetTitle("❌ Unable To Set LogChannel ❌");
                embed.SetDescription("No log channel Id provided!");
                embed.SetFooter("Pinkerton securing ");
                embed.SetTimestamp(DateTime.Now);
                await invokator.ReplyAsync(embed);
            }
        }
        #endregion

        #region BAN MEMBER
        [Command(Aliases = [ "ban", "banmember" ])]
        [Description("ban the mentioned member")]
        public async Task BanMember(CommandEvent invokator, Member? member = null, string[]? reason = null)
        {
            if (member is null)
            {
                var embed = new Embed();
                embed.SetTitle("❓ MISSING INFORMATION ❓");
                embed.SetDescription("I was expecting a mention member but didn't reveive that.\r\nexample\r\n```p?ban <member> harrassment```");
                embed.SetFooter("Pinkerton ");
                embed.SetTimestamp(DateTime.Now);
                await invokator.ReplyAsync(embed);
            }
            else
            {
                var serverId = invokator.ServerId;
                var authorId = invokator.Message.CreatedBy;
                var _reason = "";
                try
                {
                    if (reason is null)
                        _reason = "no reason given";
                    else
                        _reason = string.Join(" ", reason!);
                    var permissions = await invokator.ParentClient.GetMemberPermissionsAsync((HashId)serverId!, authorId);
                    if (permissions.Contains(Permission.ManageServer))
                    {
                        await invokator.ParentClient.AddMemberBanAsync((HashId)serverId!, member.Id, _reason);
                        await invokator.ReplyAsync("member has been banned", null, null, true, false);
                        using var db = _dbFactory.CreateDbContext();
                        var serverMember = db.Members.Where(x => x.ServerId!.Equals(serverId) && x.MemberId!.Equals(member.Id.ToString()))
                            .FirstOrDefault();
                        db.Remove(serverMember!);
                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        var embed = new Embed();
                        embed.SetTitle("❓ MISSING PERMISSIONS ❓");
                        embed.SetThumbnail(new EmbedMedia("https://t.ly/6p91J"));
                        embed.SetDescription($"<@{authorId}> you do not have to correct permissions to execute this command!");
                        embed.SetFooter("Pinkerton ");
                        embed.SetTimestamp(DateTime.Now);
                        await invokator.ReplyAsync(embed);
                    }
                }
                catch(Exception e)
                {
                    var embed = new Embed();
                    embed.SetTitle("❓ ERROR ❓");
                    embed.SetThumbnail(new EmbedMedia("https://rb.gy/wuds7l"));
                    embed.SetDescription($"{e.Message}");
                    embed.SetColor(Color.DarkRed);
                    embed.SetFooter("Pinkerton ");
                    embed.SetTimestamp(DateTime.Now);
                    await invokator.ReplyAsync(embed);
                }
            }
        }
        #endregion

        #region MUTE //TODO: fix the mute command
        [Command(Aliases = [ "mute", "silence", "gag", "muzzle" ])]
        [Description("mutes a member for a set duration")]
        public async Task Mute(CommandEvent invokator, Member? member = null, string? duration = null, string[]? reason = null)
        {
            var authorId = invokator.Message.CreatedBy;
            var serverId = invokator.ServerId;
            var author = await invokator.ParentClient.GetMemberAsync((HashId)serverId!, authorId);
            var perms = await invokator.ParentClient.GetMemberPermissionsAsync((HashId)serverId, authorId);

            if (!perms.Contains(Permission.ManageChannels))
            {
                var embed = new Embed();
                embed.SetTitle("❌ MISSING PERMISSION ❌");
                embed.SetDescription($"<@{authorId}> you do not have the correct permissions to execute this command.");
                embed.SetThumbnail(new EmbedMedia("https://t.ly/6p91J"));
                embed.SetFooter("Pinkerton watching everything ");
                embed.SetTimestamp(DateTime.Now);
                await invokator.ReplyAsync(embed);
                return;
            }
            else
            {
                if (member is null)
                {
                    var embed = new Embed();
                    embed.SetTitle("❓ MISSING ARGUMENTS ❓");
                    embed.SetDescription("I was expecting a mentionable server member but none were provided.\r\nexample command\r\n```p?mute <member> <duration> <reason>\r\n" +
                        "prefix -> mentionable member -> duration -> reason```");
                    embed.SetFooter("Pinkerton ");
                    embed.SetTimestamp(DateTime.Now);
                    await invokator.ReplyAsync(embed);
                }
                else
                {
                    if (member.Name.Equals("async<MODiXLabs>") || member.Name.Equals("marrowkai"))
                    {
                        await invokator.ReplyAsync("I am unable to mute supplied member, I do not have the authority to mute my creator or the server owner");
                        return;
                    }
                    var memberId = member.Id;
                    var roles = member.RoleIds;
                    
                    try
                    {
                        using var db = _dbFactory.CreateDbContext();
                        var serverMember = db.Members.Where(x => x.MemberId!.Equals(memberId.ToString())).FirstOrDefault();
                        

                        if (serverMember is not null)
                        {
                            //remove all member roles to get all updated roles
                            serverMember!.Roles = null;
                            db.Members.Update(serverMember);
                            await db.SaveChangesAsync();

                            //save member roles to db
                            var roleIds = new List<string>();
                            var count = 0;
                            foreach (var roleId in member.RoleIds)
                            {
                                roleIds.Add(roleId.ToString());
                                count++;
                            }
                            serverMember.Roles = roleIds;
                            db.Members.Update(serverMember);
                            await db.SaveChangesAsync();

                            //remove member roles from member
                            foreach (var r in member.RoleIds)
                            {
                                await member.RemoveRoleAsync(r);
                            }
                            //give member the mute role
                            await member.AddRoleAsync(36859576);
                        }
                    }
                    catch (Exception e)
                    {
                        await invokator.ReplyAsync($"Error: {e.Message}");
                    }

                }
            }

            
        }
        #endregion

        #region UNMUTE
        [Command(Aliases = ["unmute" ])]
        [Description("unmute a member")]
        public async Task UnMute(CommandEvent invokator, Member? member = null)
        {
            var serverId = invokator.ServerId;
            var authorId = invokator.Message.CreatedBy;
            try
            {
                
                var author = await invokator.ParentClient.GetMemberAsync((HashId)serverId!, authorId);
                var perms = await invokator.ParentClient.GetMemberPermissionsAsync((HashId)serverId, authorId);

                if (!perms.Contains(Permission.ManageChannels))
                {
                    var embed = new Embed();
                    embed.SetTitle("❌ MISSING PERMISSION ❌");
                    embed.SetDescription($"<@{authorId}> you do not have the correct permissions to execute this command.");
                    embed.SetThumbnail(new EmbedMedia("https://t.ly/6p91J"));
                    embed.SetFooter("Pinkerton watching everything ");
                    embed.SetTimestamp(DateTime.Now);
                    await invokator.ReplyAsync(embed);
                }
                else
                {
                    if (member is null)
                    {
                        await invokator.ReplyAsync("I was expecting a mentionable member to unmute, none was provided, command ignored!");
                    }
                    else
                    {
                        using var db = _dbFactory.CreateDbContext();
                        var serverMember = db.Members.Where(x => x.MemberId!.Equals(member.Id.ToString())).FirstOrDefault();
                        if (serverMember is null)
                            await invokator.ReplyAsync("member does not exist in the database, could not re-apply their roles");
                        else
                        {
                            await member.RemoveRoleAsync(36859576);
                            foreach (var r in serverMember.Roles!)
                            {
                               //var newR = uint.Parse(r);
                               await member.AddRoleAsync(uint.Parse(r));
                            }
                            await invokator.ReplyAsync("all roles re-applied succesfully");
                        }
                    }
                }

            }
            catch(Exception e)
            {
                var embed = new Embed();
                embed.SetTitle("❌ ERROR ❌");
                embed.SetDescription($"<@{authorId}> something went wrong.\r\n```{e.Message}```");
                embed.SetThumbnail(new EmbedMedia("https://rb.gy/wuds7l"));
                embed.SetFooter("Pinkerton watching everything ");
                embed.SetTimestamp(DateTime.Now);
                await invokator.ReplyAsync(embed);
            }
                
        }
        #endregion

        #region ERRORCODE LOOKUP
        [Command(Aliases = [ "codelookup", "findcode", "fc", ])]
        [Description("find an errorcode")]
        public async Task LookupCode(CommandEvent invokator, string? errorCode = "")
        {
            if (errorCode is null || errorCode.Equals(""))
            {
                await invokator.ReplyAsync($"❗ please provide an error code!", null, null, true, false);
            }
            else
            {
                var embed = new Embed();
                try
                {
                    using var db = _dbFactory.CreateDbContext();
                    var code = Guid.TryParse(errorCode, out Guid newCode);
                    if (code)
                    {
                        var ec = db.Errors.Where(x => x.ErrorCode.Equals(newCode))
                            .FirstOrDefault();
                        if (ec is not null)
                        {
                            embed.SetTitle("✔ ERROR CODE FOUND ✔");
                            embed.SetDescription($"```{ec.ErrorCode}: {ec.ErrorMessage}```");
                            embed.SetFooter("Pinkerton watching everything");
                            embed.SetTimestamp(DateTime.Now);
                            await invokator.ReplyAsync(embed);
                        }
                        else
                        {
                            await invokator.ReplyAsync("I could not find the error code you provided.");
                        }
                    }
                    else
                        await invokator.ReplyAsync("trouble parsing error code, command canceled!");
                         
                }
                catch(Exception e)
                {
                    embed.SetTitle("❗ ERROR ❗");
                    embed.SetDescription($"{e.Message}");
                    embed.SetFooter("Pinkerton watching everything");
                    embed.SetTimestamp(DateTime.Now);
                    await invokator.ReplyAsync(embed);
                }
            }
        }
        #endregion

        #region MESSAGE HISTORY
        [Command(Aliases = [ "getmsghistory", "gmh" ])]
        [Description("get a list of messages for a specified member")]
        public async Task GetMsgHistory(CommandEvent invokator, Member? member = null, int page = 1)
        {
            var pageCount = 0;
            if (member is null)
            {
                await invokator.ReplyAsync("I was expecting a mentioned member but didn't find one, please mention the member you would like to get a message history for!");
            }
            else
            {
                var memberId = member.Id;
                var serverId = invokator.ServerId;
                var cmdAuthorId = invokator.Message.CreatedBy;
                try
                {
                    var cmdAuthor = await invokator.ParentClient.GetMemberAsync((HashId)serverId!, cmdAuthorId);
                    var perms = await invokator.ParentClient.GetMemberPermissionsAsync((HashId)serverId!, cmdAuthorId);
                    if (!perms.Contains(Permission.ManageMessages))
                    {
                        var embed = new Embed();
                        embed.SetTitle("❌ PERMISSION DENIED ❌");
                        embed.SetDescription($"<@{cmdAuthorId}> you do not have the correct permissions to execute this command!");
                        embed.SetThumbnail(new EmbedMedia("https://t.ly/6p91J"));
                        embed.SetFooter("Pinkerton watching everything ");
                        embed.SetTimestamp(DateTime.Now);
                        await invokator.ReplyAsync(embed);
                    }
                    else
                    {
                        using var db = _dbFactory.CreateDbContext();
                        var messages = db.Messages.Where(x => x.MemberId!.Equals(memberId.ToString()) && x.ServerId!.Equals(serverId.ToString()))
                            .ToList();
                        if (messages.Count > 0)
                        {

                            Paginator<GuildMessage> paginator = new Paginator<GuildMessage>();
                            List<List<GuildMessage>> paginatedList = paginator.Paginate(messages!, 10);
                            pageCount = paginatedList.Count;
                            var listBuilder = new StringBuilder();
                            var count = 1;
                            foreach (var item in paginatedList[page - 1])
                            {
                                if (item.ChannelId is not null)
                                {
                                    var channel = await invokator.ParentClient.GetChannelAsync((Guid)item.ChannelId!);
                                    listBuilder.Append($"- {item.Content}: {channel.Name}\r\n");
                                    count++;
                                }
                                else
                                {
                                   // var channel = await invokator.ParentClient.GetChannelAsync((Guid)item.ChannelId);
                                    listBuilder.Append($"- {item.Content} : channel not found\r\n");
                                    count++;
                                }
                                
                            }
                            var embed = new Embed();
                            embed.SetTitle($"Message History for <@{member.Id}> (`{member.Id}`)");
                            embed.SetDescription(listBuilder.ToString());
                            embed.SetFooter($"viewing page {page} of {paginatedList.Count} ");
                            embed.SetTimestamp(DateTime.Now);
                            await invokator.ReplyAsync(embed);
                        }
                        else
                            await invokator.ReplyAsync($"no messages found for member {member.Name}");
                    }
                }
                catch(Exception e)
                {
                    if (page > pageCount)
                    {
                        var sName = await invokator.ParentClient.GetServerAsync((HashId)serverId!);
                        var error = new SystemError()
                        {
                            ErrorCode = SystemErrors.GetError("Does Not Exist", Guid.NewGuid()),
                            ErrorMessage = "Page does not exist, you requested a page larger than the collection.",
                            ServerId = invokator.ServerId.ToString(),
                            ServerName = sName.Name
                        };
                        var embed = new Embed();
                        embed.SetTitle("❌ ERROR ❌");
                        embed.SetDescription($"{error.ErrorMessage} please refer below code to a moderator for further review ```{error.ErrorCode}```");
                        embed.SetFooter("Pinkerton watching everything ");
                        embed.SetTimestamp(DateTime.UtcNow);
                        embed.SetColor(Color.DarkRed);
                        await invokator.ReplyAsync(embed);
                    }
                    else
                    {
                        var sName = await invokator.ParentClient.GetServerAsync((HashId)serverId!);
                        var error = new SystemError()
                        {
                            ErrorCode = SystemErrors.GetError(e.Message, Guid.NewGuid()),
                            ErrorMessage = e.Message,
                            ServerId = invokator.ServerId.ToString(),
                            ServerName = sName.Name
                        };
                        var embed = new Embed();
                        embed.SetTitle("❌ ERROR ❌");
                        embed.SetDescription($"{error.ErrorMessage} : please refer below code to a moderator for further review ```{error.ErrorCode}```");
                        embed.SetFooter("Pinkerton watching everything ");
                        embed.SetTimestamp(DateTime.UtcNow);
                        embed.SetColor(Color.DarkRed);
                        await invokator.ReplyAsync(embed);
                    }
                    
                }
            }
        }
        #endregion

        #region MESSAGE LOOKUP
        [Command(Aliases = [ "fm", "findmessage" ])]
        [Description("find a specific member message")]
        public async Task FindMessage(CommandEvent invokator, Member? member = null, string? msg = null)
        {
            if (member is null || msg is null)
            {

            }
            else
            {
                try
                {

                }
                catch(Exception e)
                {

                }
            }
        }
        #endregion

        #region MEMBER MATAINENCE
        [Command(Aliases = [ "mat" ])]
        [Description("run matainence on the server")]
        public async Task RunMatainence(CommandEvent invokator, string args = "")
        {
            var embed = new Embed();
            if (args is null || args.Equals(""))
            {
                embed.SetTitle("❌ MISSING ARGUMENT ❌");
                embed.SetDescription("I was expecting an argument to run the command, no argument was provided.\r\nplease provide a command argument." +
                    "\r\nexample command ```p?mat <argument>```\r\n" +
                    "available arguments ```member - runs member matainence on the server```");
                embed.SetFooter("Pinkerton watching everything ");
                embed.SetTimestamp(DateTime.Now);
                await invokator.ReplyAsync(embed);
            }
            else
            {
                switch (args)
                {
                    case "members":
                        {
                            try
                            {
                                var serverId = invokator.ServerId;
                                var members = await invokator.ParentClient.GetMembersAsync((HashId)serverId!);
                                using var db = _dbFactory.CreateDbContext();
                                var serverMembers = db.Members.Where(x => x.ServerId!.Equals(serverId.ToString()))
                                    .Select(x => x.Name)
                                    .ToList();
                                if (members is not null)
                                {
                                    await invokator.ReplyAsync("member matainence started...");
                                    var timer = new Stopwatch();
                                    timer.Start();
                                    foreach (var mem in members)
                                    {
                                        if (!mem.IsBot)
                                        {
                                            var roleIds = await mem.ParentClient.GetMemberRolesAsync((HashId)serverId!, mem.Id);
                                            var roleNames = new List<string>();

                                            foreach (var r in roleIds)
                                            {
                                                var roleName = await mem.ParentClient.GetRoleAsync((HashId)serverId!, r);
                                                roleNames.Add(roleName.Id.ToString());
                                            }

                                            if (!serverMembers.Contains(mem.Name))
                                            {
                                                var sMem = new ServerMember()
                                                {
                                                    Name = mem.Name,
                                                    ServerId = serverId.ToString(),
                                                    MemberId = mem.Id.ToString(),
                                                    Warnings = 0,
                                                    KickedAt = null,
                                                    BannedAt = null,
                                                    IsBanned = false,
                                                    Roles = roleNames,
                                                    Infractions = null
                                                };
                                                db.Members.Add(sMem);
                                                await db.SaveChangesAsync();
                                            }
                                            else
                                            {
                                                var sMem = new ServerMember()
                                                {
                                                    Name = mem.Name,
                                                    ServerId = serverId.ToString(),
                                                    MemberId = mem.Id.ToString(),
                                                    Warnings = 0,
                                                    KickedAt = null,
                                                    BannedAt = null,
                                                    IsBanned = false,
                                                    Roles = roleNames,
                                                    Infractions = null
                                                };
                                                db.Members.Update(sMem);
                                                await db.SaveChangesAsync();
                                            }
                                        }
                                    }
                                    timer.Stop();
                                    var completedIn = timer.Elapsed.Humanize();
                                    await invokator.ReplyAsync($"member matainence is complete, all members have been added to the database, run time [{completedIn}]");
                                }
                                else
                                {
                                    await invokator.ReplyAsync("unable to run member matainence, members not found");
                                }
                            }
                            catch(Exception e)
                            {
                                await invokator.ReplyAsync($"Error: {e.Message}\r\nno error code at this time.");
                            }
                        }
                        break;
                    default:
                        await invokator.ReplyAsync("unknown argument, command aborted!");
                        break;
                }
                
            }
        }
        #endregion

        #region UPDATE MEMBER ROLES
        [Command(Aliases = [ "updatememberroles", "umr" ])]
        [Description("update all member roles")]
        public async Task UpdateMemberRoles(CommandEvent invokator, Member? member = null)
        {
            var roleList = new List<string>();
            try
            {
                if (member is null)
                {
                    await invokator.ReplyAsync("I was expecting a mentioned member but none was provided\r\nplease provide a mentioned member.");
                }
                else
                {
                    using var db = _dbFactory.CreateDbContext();
                    var roleIds = member.RoleIds;
                    var serverMember = db.Members.Where(x => x.MemberId!.Equals(member.Id.ToString()) && x.ServerId!.Equals(member.ServerId.ToString())).FirstOrDefault();

                    if (serverMember is not null)
                    {
                        var timer = new Stopwatch();
                        timer.Start();
                        foreach (var id in roleIds)
                        {
                            //var role = await invokator.ParentClient.GetRoleAsync((HashId)invokator.ServerId!, id);
                            roleList.Add(id.ToString());
                        }
                        serverMember.Roles = roleList;
                        db.Update(serverMember);
                        await db.SaveChangesAsync();
                        timer.Stop();
                        var duration = timer.Elapsed.Humanize();
                        await invokator.ReplyAsync($"updating member roles complete. run time [{duration}]");
                    }
                    else
                    {
                        await invokator.ReplyAsync($"I could not find {member.Name} in the database, roles not updated.", null, null, true, false);
                    }
                    
                }
            }
            catch(Exception e)
            {
                await invokator.ReplyAsync($"Error: {e.Message}, no error code at this time.");
            }
        }
        #endregion
    }
}
