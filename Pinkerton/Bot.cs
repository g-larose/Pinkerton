using Guilded;
using Guilded.Base;
using Guilded.Base.Embeds;
using Guilded.Commands;
using Guilded.Servers;
using Humanizer;
using Humanizer.Localisation;
using Microsoft.EntityFrameworkCore;
using Pinkerton.Commands;
using Pinkerton.Factories;
using Pinkerton.Models;
using Pinkerton.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pinkerton
{
    public class Bot
    {
        private static readonly string? json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "config.json"));
        private static readonly string? token = JsonSerializer.Deserialize<ConfigJson>(json!)!.Token!;
        private static readonly string? prefix = JsonSerializer.Deserialize<ConfigJson>(json!)!.Prefix!;
        private static readonly string? timePattern = "hh : mm tt";
        private TimeSpan duration { get; set; }
        private List<Member> Joins = new();

        private PinkertonDbContextFactory _dbFactory = new();

        public async Task RunAsync()
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            await using var client = new GuildedBotClient(token!)
                .AddCommands(new ModCommands(), prefix!)
                .AddCommands(new MemberCommands(), prefix!);

            using var messageHandler = new MessageHandlerService(_dbFactory, client);

            //var db = _dbFactory.CreateDbContext();
            //db.Database.Migrate();

            #region PREPARED
            client.Prepared
                .Subscribe(me =>
                {
                    
                    var embed = new Embed();
                    embed.SetTitle("Pinkerton has connected!");
                    embed.SetColor(Color.DarkRed);
                    var time = DateTime.Now.ToString(timePattern);
                    var date = DateTime.Now.ToShortDateString();
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine($"[{date}][{time}][INFO]  [{client.Name}] connecting...");
                    //await server.ParentClient.CreateMessageAsync((Guid)channelId!, embed);
                });
            #endregion

            #region MESSAGE CREATED
            client.MessageCreated
                .Subscribe(async msg =>
                {
                   
                    var msgAuthorId = msg.CreatedBy;
                    var serverId = msg.ServerId;
                    Server? server;
                    var logChannelId = Guid.Empty;
                    try
                    {
                        
                        var author = await msg.ParentClient.GetMemberAsync((HashId)serverId!, msgAuthorId);
                        server = await msg.ParentClient.GetServerAsync((HashId)serverId!);
                        if (author.IsBot) return;
                        
                        try
                        {
                            using var db = _dbFactory.CreateDbContext();
                            var gMsg = new GuildMessage()
                            {
                                Identifier = Guid.NewGuid(),
                                Content = msg.Content,
                                CreatedAt = msg.CreatedAt,
                                ChannelId = msg.ChannelId,
                                MemberId = msgAuthorId.ToString(),
                                ServerId = serverId.ToString()
                            };

                            var msgCount = db.Messages.ToList().Count;
                            if (msg.Content.Equals("kingasync"))
                            {
                                await msg.ParentClient.AddXpAsync((HashId)msg.ServerId!, author.Id, 1000);
                                await msg.ReplyAsync($"{author.Name} received 1000 xp");
                            }
                            if (msgCount >= 1000)
                            {
                                var messageToRemove = db.Messages.First();
                                db.Remove(messageToRemove);
                                await db.SaveChangesAsync();
                                db.Add(gMsg);
                                await db.SaveChangesAsync();
                            }
                            else
                            {
                                db.Add(gMsg);
                                await db.SaveChangesAsync();
                            }

                            await messageHandler.HandleMessage(msg.Message);
                            var isSafeWord = messageHandler.BannedWordDetector(msg.Message);
                            
                            if (isSafeWord)
                            {
                                var embed = new Embed();
                                await msg.DeleteAsync();
                                var authorId = msg.CreatedBy;
                                using var modService = new ModCommandProviderService(_dbFactory, client);
                                var infResult = modService.AddInfraction(serverId.ToString()!, authorId.ToString(), "use of filtered word");
                                if (infResult.IsOk)
                                {
                                    embed.SetDescription($"<@{authorId}> you have received an infraction for using a filtered word\r\n\r\n" +
                                        $"don't agree with this infration - refer `{infResult.Value.Identifier}` to a moderator for further review");
                                    await msg.ReplyAsync(true, false, embed);
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            var error = new SystemError()
                            {
                                ErrorCode = SystemErrors.GetError(ex.Message, Guid.NewGuid()),
                                ErrorMessage = ex.Message,
                                ServerId = serverId.ToString(),
                                ServerName = server.Name,
                            };
                            var time = DateTime.UtcNow.ToString(timePattern);
                            var date = DateTime.UtcNow.ToShortDateString();
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine($"[{date}][{time}][ERROR]  [{client.Name}] {msgAuthorId}: {error.ErrorMessage}: {error.ErrorCode}");
                            using var db = _dbFactory.CreateDbContext();
                            db.Add(error);
                            await db.SaveChangesAsync();
                            var embed = new Embed();
                            embed.SetTitle("❌ ERROR ❌");
                            embed.SetDescription($"an error occured , please refer below code to a moderator for further review ```{error.ErrorCode}```");
                            embed.SetFooter("Pinkerton watching everything ");
                            embed.SetColor(Color.DarkRed);
                            logChannelId = (Guid)db.Servers.Where(x => x.ServerId!.Equals(msg.ServerId.ToString())).Select(x => x.LogChannel).FirstOrDefault()!;
                            await msg.ParentClient.CreateMessageAsync(logChannelId, embed);
                        }
                       
                    }
                    catch(Exception e)
                    {
                        var log = new SystemError()
                        {
                            ErrorCode = Guid.NewGuid(),
                            ErrorMessage = e.Message
                        };
                        var time = DateTime.Now.ToString(timePattern);
                        var date = DateTime.Now.ToShortDateString();
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine($"[{date}][{time}][INFO]  [{client.Name}] {msgAuthorId}: {log.ErrorMessage}: {log.ErrorCode}");
                    }
                    
                });
            #endregion

            #region MESSAGE DELETED
            client.MessageDeleted
                .Subscribe(async msg =>
                {
                    var dbFactory = new PinkertonDbContextFactory();
                    using var db = dbFactory.CreateDbContext();
                    var embed = new Embed();
                    try
                    {
                        if (msg.CreatedBy.Equals(msg.ParentClient.Id)) return;
                        var time = DateTime.UtcNow.ToString(timePattern);
                        var date = DateTime.UtcNow.ToLongDateString();
                        var server = await msg.ParentClient.GetServerAsync((HashId)msg.ServerId!);
                        var logchannelId = db.Servers.Where(x => x.ServerId!.Equals(msg.ServerId.ToString())).FirstOrDefault();
                        if (logchannelId is null)
                        {
                            var logchannel = await msg.ParentClient.GetChannelAsync((Guid)server.DefaultChannelId!);
                            embed.SetDescription($"I could not find a log channel.\r\nplease create a log channel if one isn't already created.\r\n\r\n" +
                                                         "to set the log channel id - run command `p?slc <channel id>`");
                            embed.SetColor(Color.DarkSlateGray);
                            embed.SetFooter("Pinkerton ");
                            embed.SetTimestamp(DateTime.Now);

                            await msg.Message.ReplyAsync(embed);
                        }
                        else
                        {
                            var logchannel = await msg.ParentClient.GetChannelAsync((Guid)logchannelId.LogChannel!);
                            var author = await msg.ParentClient.GetMemberAsync((HashId)msg.ServerId!, msg.CreatedBy);
                            var msgChannel = await msg.ParentClient.GetChannelAsync((Guid)msg.ChannelId);
                            var createdAt = msg.CreatedAt;
                            var msgId = msg.Id;
                            var channelId = await msg.ParentClient.GetChannelAsync(msg.ChannelId);
                            var channelLink = $"[{channelId.Name}](https://www.guilded.gg/teams/{msg.ServerId}/channels/{channelId.Id}/chat)";
                            var msgDate = $"{date} at {time}";
                            
                            embed.SetDescription($"Message from <@{msg.CreatedBy}>(`{msg.CreatedBy}`) was deleted in {channelLink}" +
                                $"\r\n\r\nContent" +
                                $"```{msg.Content}```\r\n" +
                                $"Additional Info:\r\n" +
                                $"When: `{msgDate}`\r\n" +
                                $"Message ID: `{msgId}`\r\n" +
                                $"Channel ID: `{channelId.Id}`\r\n");
                            embed.SetFooter("Pinkerton ");
                            embed.SetTimestamp(DateTime.Now);
                            await msg.ParentClient.CreateMessageAsync((Guid)logchannel.Id, embed);
                        } 
                        
                    }
                    catch(Exception e)
                    {
                        var log = new SystemError()
                        {
                            ErrorCode = Guid.NewGuid(),
                            ErrorMessage = e.Message
                        };
                        if (log.ErrorMessage.Contains("Missing permissions"))
                            await msg.Message.ReplyAsync("I do not have the correct permissions to send messages in the default channel, ensure I have the [CanReadChats] [CanCreateChats] permissions");
                        var time = DateTime.UtcNow.ToString(timePattern);
                        var date = DateTime.UtcNow.ToShortDateString();
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine($"[{date}][{time}][INFO]  [{client.Name}] {log.ErrorMessage}: {log.ErrorCode}");
                    }
                });
            #endregion

            #region CHANNEL CREATED
            client.ChannelCreated
                .Subscribe(async channel =>
                {
                    Server? server = null;
                    try
                    {
                        var db = _dbFactory.CreateDbContext();
                        var localServer = db.Servers.Where(x => x.ServerId.Equals(channel.Channel.ServerId.ToString())).FirstOrDefault();
                        server = await channel.ParentClient.GetServerAsync((HashId)channel.ServerId!);
                        var channelId = channel.Channel.Id;
                        var chnl = await channel.Channel.ParentClient.GetChannelAsync((Guid)channelId);
                        var creatorId = channel.CreatedBy;
                        var createdDate = channel.CreatedAt.Subtract(DateTime.UtcNow).Humanize();
                        var embed = new Embed();
                        embed.SetTitle($"Channel {chnl.Name} Created");
                        embed.SetDescription($"<@{chnl.Id}> created {createdDate} by <@{creatorId}>\r\nAdditional Info ```Creator: <@{creatorId}>\r\nCreated Date: {createdDate}```");
                        embed.SetFooter("Pinkerton watching everything ");
                        embed.SetTimestamp(DateTime.UtcNow);

                        if (localServer is not null)
                        {
                            if (localServer.DefaultChannelId is not null || localServer.DefaultChannelId != Guid.Empty)
                            {
                                 await channel.Channel.ParentClient.CreateMessageAsync((Guid)localServer.DefaultChannelId!, embed);
                            }
                            else
                            {
                                var time = DateTime.Now.ToString(timePattern);
                                var date = DateTime.Now.ToShortDateString();
                                Console.WriteLine($"[{date}][{time}] {server.Name} doesn't have a default channel set");
                            }
                                
                        } 
                        else
                            await channel.Channel.ParentClient.CreateMessageAsync((Guid)server.DefaultChannelId!, embed);
                    }
                    catch(Exception e)
                    {
                        server = await channel.ParentClient.GetServerAsync((HashId)channel.ServerId!);
                        var embed = new Embed();
                        embed.SetTitle($"❌ Error ❌");
                        embed.SetDescription($"a channel was created, I am having trouble fetching the channel info!");
                        embed.SetFooter("Pinkerton watching everything ");
                        embed.SetTimestamp(DateTime.UtcNow);
                        await channel.Channel.ParentClient.CreateMessageAsync((Guid)server.DefaultChannelId!, embed);
                    }
                });
            #endregion

            #region MEMBER JOINED
            client.MemberJoined
                .Subscribe(async member =>
                {
                    Joins.Add(member.Member);

                    if (Joins.Count >= 5)
                    {
                        duration = DateTime.UtcNow.Second.Seconds().Subtract(Joins.First().JoinedAt.Second.Seconds());
                        if (duration <= TimeSpan.FromSeconds(5))
                        {
                            Server? gServer = null;
                            try
                            {
                                gServer = await member.ParentClient.GetServerAsync((HashId)member.Member.ServerId!);
                                await client.CreateMessageAsync((Guid)gServer.DefaultChannelId!, "raid detected.... all parties of the raid have been kicked from the server.");
                                foreach (var mem in Joins)
                                {
                                    try
                                    {
                                        await mem.ParentClient.RemoveMemberAsync((HashId)mem.ServerId!, mem.Id);
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine($"error in [{gServer.Name}] : {e.Message}");
                                        continue;
                                    }

                                }
                                Joins.Clear();
                            }
                            catch(Exception e)
                            {
                                Console.WriteLine($"error in [{gServer.Name}] : {e.Message}");
                            }
                            
                        }
                        else
                            Joins.Clear();
                    }

                    try
                    {
                        using var db = _dbFactory.CreateDbContext();
                        using var welcomeService = new WelcomeProviderService();
                        var serverMem = db.Members.Where(x => x.MemberId!.Equals(member.Member.Id.ToString()) && x.ServerId!.Equals(member.Member.ServerId.ToString())).FirstOrDefault();
                        if (serverMem is null)
                        {
                            var defaultChannelId = Guid.Empty;
                            var roles = new List<string>();

                            foreach (var r in member.Member.RoleIds)
                            {
                                var role = await member.Member.ParentClient.GetRoleAsync((HashId)member.Member.ServerId!, r);
                                roles.Add(role.Id.ToString());
                                
                            }
                            var localServer = db.Servers.Where(x => x.ServerId!.Equals(member.Member.ServerId.ToString())).FirstOrDefault();
                            var gServer = await member.Member.ParentClient.GetServerAsync((HashId)member.Member.ServerId!);

                            var newMem = new ServerMember()
                            {
                                Name = member.Member.Name,
                                ServerId = member.Member.ServerId.ToString(),
                                MemberId = member.Member.Id.ToString(),
                                IsBanned = false,
                                BannedAt = null,
                                KickedAt = null,
                                Warnings = 0,
                                Infractions = null,
                                Roles = roles,
                            };

                            db.Members.Add(newMem);
                            await db.SaveChangesAsync();

                            var welcomeMsg = await welcomeService.GetRandomWelcomeMessageAsync();
                            var newWelcomeMsg = welcomeMsg.Message!.Replace("[member]", $"<@{member.Member.Id}>").Replace("[server]", localServer!.ServerName);
                            defaultChannelId = (Guid)gServer.DefaultChannelId!;
                            var embed = new Embed();
                            embed.SetDescription($"{newWelcomeMsg}");
                            var embeds = new Embed[] { embed };
                            await member.Member.ParentClient.CreateMessageAsync(defaultChannelId, embeds);

                            if (localServer is not null)
                            {
                                if (localServer.LogChannel is not null || localServer.LogChannel != Guid.Empty)
                                {
                                    var logChannelId = (Guid)localServer.LogChannel!;
                                    await member.ParentClient.CreateMessageAsync(logChannelId!, $"{member.Member.Name} has joind {localServer.ServerName}");
                                }
                                else
                                    await member.ParentClient.CreateMessageAsync((Guid)gServer.DefaultChannelId!, $"{member.Member.Name} has joind {localServer.ServerName}");
                                
                            }
                        }

                    }
                    catch(Exception e)
                    {
                        var time = DateTime.Now.ToString(timePattern);
                        var date = DateTime.Now.ToShortDateString();
                        Console.WriteLine($"[{date}] [{time}] {client.Name} threw error {e.Message} in member joined event\r\n{member.Member.Name} in {member.Member.ServerId}");
                    }
                    
                });
            #endregion

            #region MEMBER UPDATED
            client.MemberUpdated                   
                .Subscribe(async mem =>
                {
                    try
                    {
                        var serverId = mem.ServerId;
                        var memberId = mem.Id;
                        var channelId = Guid.Parse("33b672e9-faff-4dc1-a692-986a69fa5204");
                        var server = await mem.ParentClient.GetServerAsync((HashId)serverId!);
                        var member = await mem.ParentClient.GetMemberAsync((HashId)serverId!, memberId);
                        var nickName = member.Nickname ?? "";
                        await mem.ParentClient.CreateMessageAsync(channelId, $"member {member.Name} has updated there user's nickname to [{nickName}]");
                    }
                    catch (Exception e)
                    {
                        var log = new SystemError()
                        {
                            ErrorCode = Guid.NewGuid(),
                            ErrorMessage = e.Message
                        };
                        var time = DateTime.Now.ToString(timePattern);
                        var date = DateTime.Now.ToShortDateString();
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine($"[{date}][{time}][INFO]  [{client.Name}] {log.ErrorMessage}: {log.ErrorCode}");
                    }


                });
            #endregion

            #region SERVER ADDED
            client.ServerAdded
                .Subscribe(async server =>
                {
                    var serverId = server.ServerId;
                    var dbFactory = new PinkertonDbContextFactory();
                    using var db = dbFactory.CreateDbContext();

                    try
                    {
                        var newServer = new ServerConfig()
                        {
                            ServerId = server.ServerId.ToString(),
                            CreatedAt = server.Server.CreatedAt,
                            DefaultChannelId = server.Server.DefaultChannelId ?? Guid.Empty,
                            LogChannel = server.Server.DefaultChannelId ?? Guid.Empty,
                            Type = Enums.ServerType.GUILD,
                            FilteredWords = ["fuck", "FUCK", "Fuck", "PUSSY", "Pussy", "pussy", "NIGGER", "Nigger", "nigger", "nigga", "NIGGA", "Nigga"],
                            Messages = [],
                            OwnerId = server.Server.OwnerId.ToString(),
                            ServerName = server.Server.Name,
                        };

                        var members = await server.ParentClient.GetMembersAsync((HashId)serverId!);
                        var serverMembers = new List<ServerMember>();

                       
                            foreach (var mem in members)
                            {
                                try
                                {
                                    if (mem.IsBot)
                                        continue;
                                    var roles = await mem.ParentClient.GetMemberRolesAsync(serverId, mem.Id);
                                    var memRoles = new string[roles.Count];
                                    var count = 0;
                                    foreach (var role in roles)
                                    {
                                        var mRole = await mem.ParentClient.GetRoleAsync((HashId)serverId!, role);
                                        memRoles[count] = $"{mRole.Name},";
                                    }

                                    var serverMem = new ServerMember()
                                    {
                                        Name = mem.Name,
                                        ServerId = serverId.ToString(),
                                        MemberId = mem.Id.ToString(),
                                        BannedAt = null,
                                        KickedAt = null,
                                        IsBanned = false,
                                        Infractions = null,
                                        Warnings = 0,
                                        Roles = mem.RoleIds.Select(x => x.ToString()).ToList()
                                    };
                                    serverMembers.Add(serverMem);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);

                                }
                    }
                        
                        
                        db.Members.AddRange(serverMembers);
                        db.Servers.Add(newServer);
                        await db.SaveChangesAsync();
                        var time = DateTime.Now.ToString(timePattern);
                        var date = DateTime.Now.ToShortDateString();
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine($"[{date}][{time}][INFO]  [{client.Name}] {newServer.ServerName} with ID: {newServer.ServerId} added.");
                    }
                    catch(Exception e)
                    {
                        var s = await server.ParentClient.GetServerAsync((HashId)serverId);
                        var error = new SystemError()
                        {
                            ErrorCode = SystemErrors.GetError(e.Message, Guid.NewGuid()),
                            ErrorMessage = e.Message,
                            ServerId = serverId.ToString(),
                            ServerName = s.Name
                        };
                        //db.Errors.Add(error);
                        //await db.SaveChangesAsync();
                        var time = DateTime.Now.ToString(timePattern);
                        var date = DateTime.Now.ToShortDateString();
                        Console.WriteLine($"[{date}][{time}][INFO]  [{client.Name}] {e.Message}");
                       // await server.ParentClient.CreateMessageAsync((Guid)s.DefaultChannelId!, "something went wrong, members not added to database. you can run cmd `p?mat <members>` to collect server member data");
                    }
                    
                });
            #endregion

            #region ROLE CREATED
            client.RoleCreated
                .Subscribe(async role =>
                {
                    using var db = _dbFactory.CreateDbContext();
                    try
                    {
                        var embed = new Embed();
                        var logChannel = db.Servers.Where(x => x.ServerId!.Equals(role.ServerId.ToString()))
                                                   .Select(x => x.LogChannel)
                                                   .FirstOrDefault();
                        if (logChannel is not null)
                        {
                            embed.SetTitle($"Role ```{role.Name}``` Created");
                            embed.SetDescription($"Additional Info:\r\n" +
                                $"```Role: {role.Name}```\r\n" +
                                $"```Role ID {role.Id}```\r\n" +
                                $"Self Assignable: {role.IsSelfAssignable}");
                            embed.SetFooter("Pinkerton watching everything ");
                            embed.SetTimestamp(DateTime.Now);
                            await role.ParentClient.CreateMessageAsync((Guid)logChannel, embed);
                        }
                        else
                        {
                            var serverId = role.ServerId;
                            var server = await role.ParentClient.GetServerAsync((HashId)serverId);
                            var defaultChannel = server.DefaultChannelId;
                            embed.SetTitle($"Role Created ```{role.Name}```");
                            embed.SetDescription($"Additional Info:\r\n" +
                                $"```Role: {role.Name}```\r\n" +
                                $"```Role ID {role.Id}```\r\n" +
                                $"Self Assignable: {role.IsSelfAssignable}");
                            embed.SetFooter("Pinkerton watching everything ");
                            embed.SetTimestamp(DateTime.Now);
                            await role.ParentClient.CreateMessageAsync((Guid)defaultChannel!, embed);
                        }
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine($"ERROR: {e.Message}");
                    }
                    
                });
            #endregion

            #region ROLE UPDATED
            client.MemberRolesUpdated
                .Subscribe(async memberRole =>
                {
                    using var db = _dbFactory.CreateDbContext();
                    
                    var embed = new Embed();
                    try
                    {
                        var logChannel = db.Servers.Where(x => x.ServerId!.Equals(memberRole.ServerId.ToString()))
                        .Select(x => x.LogChannel).FirstOrDefault();

                        var memberId = memberRole.MemberRoleIds[0].Id;
                        var member = await memberRole.ParentClient.GetMemberAsync((HashId)memberRole.ServerId!, memberId);
                        var cachedRoles = db.Members.Where(x => x.MemberId!.Equals(member.Id.ToString())
                                                    && x.ServerId!.Equals(member.ServerId.ToString()))
                                              .Select(x => x.Roles)!
                                              .FirstOrDefault()!
                                              .ToList();

                        var serverRoles = memberRole.MemberRoleIds[0].RoleIds;
                       
                        
                        if (serverRoles.Count > cachedRoles.Count)
                        {
                            //role added

                            IEnumerable<uint> addedRoles = serverRoles.Where(x => !cachedRoles.Contains(x.ToString())).Cast<uint>();
                            var mem = db.Members.Where(x => x.ServerId!.Equals(member.ServerId.ToString()) && x.MemberId!.Equals(member.Id.ToString())).FirstOrDefault();
                            mem!.Roles!.Add(addedRoles.First().ToString());
                            db.Update(mem);
                            await db.SaveChangesAsync();
                            if (logChannel is not null || logChannel != Guid.Empty)
                            {
                                embed.SetTitle($"<@{member.Id}> had role <@{addedRoles.First()}> given");
                                embed.SetColor(Color.LightGreen);
                                await memberRole.ParentClient.CreateMessageAsync((Guid)logChannel!, embed);

                            }
                        }
                        if (cachedRoles.Count > serverRoles.Count)
                        {
                            //role removed
                            IEnumerable<uint> removedRoles = cachedRoles.Where(x => !serverRoles.Contains(uint.Parse(x))).Cast<uint>();

                            var mem = db.Members.Where(x => x.ServerId!.Equals(member.ServerId.ToString()) && x.MemberId!.Equals(member.Id.ToString())).FirstOrDefault();
                            mem!.Roles!.Remove(removedRoles.First().ToString());
                            db.Update(mem);
                            await db.SaveChangesAsync();
                            if (logChannel is not null || logChannel != Guid.Empty)
                            {
                                embed.SetTitle($"<@{member.Id}> had role <@{removedRoles.First()}> removed");
                                embed.SetColor(Color.LightPink);
                                await memberRole.ParentClient.CreateMessageAsync((Guid)logChannel!, embed);

                            }
                        }
                        
                        if (cachedRoles.Count == serverRoles.Count)
                            await memberRole.ParentClient.CreateMessageAsync((Guid)logChannel!, "no changes");
                            
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine($"Error: {e.Message} in [MemberRoleUpdated] event");
                    }

                });
            #endregion

            #region ROLE REMOVED
            client.RoleDeleted
                .Subscribe(async role =>
                {
                    using var db = _dbFactory.CreateDbContext();
                    try
                    {
                        var embed = new Embed();
                        var logChannel = db.Servers.Where(x => x.ServerId!.Equals(role.ServerId.ToString()))
                                                   .Select(x => x.LogChannel)
                                                   .FirstOrDefault();
                        if (logChannel is not null)
                        {
                            await role.ParentClient.CreateMessageAsync((Guid)logChannel, $"role {role.Name} has been removed");
                        }
                        else
                        {
                            var serverId = role.ServerId;
                            var server = await role.ParentClient.GetServerAsync((HashId)serverId);
                            var defaultChannel = server.DefaultChannelId;
                            await role.ParentClient.CreateMessageAsync((Guid)defaultChannel!, $"role {role.Name} has been removed.");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"ERROR: {e.Message}");
                    }
                });
            #endregion

            await client.ConnectAsync();
            await client.SetStatusAsync("always watching...", 90002579);
            var botTimer = new BotTimerService();
            var time = DateTime.Now.ToString(timePattern);
            var date = DateTime.Now.ToShortDateString();
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"[{date}][{time}][INFO]  [{client.Name}] connected...");
            Console.WriteLine($"[{date}][{time}][INFO]  [{client.Name}] registering command modules...");
            await Task.Delay(200);
            Console.WriteLine($"[{date}][{time}][INFO]  [{client.Name}] listening for events...");
            await Task.Delay(-1);
        }
    }
}
