using Guilded;
using Guilded.Base;
using Guilded.Base.Embeds;
using Guilded.Client;
using Guilded.Content;
using Pinkerton.BaseModules;
using Pinkerton.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinkerton.Services
{
    public class MessageHandlerService : IDisposable
    {
        public async Task HandleMessage(Message message)
        {
            try
            {
                var channelId = message.ChannelId;
                var authorId = message.CreatedBy;
                var serverId = message.ServerId;
                var server = await message.ParentClient.GetServerAsync((HashId)serverId!);
                var author = await message.ParentClient.GetMemberAsync((HashId)serverId!, authorId);
                var embed = new Embed();

                if (message.Content!.Length.Equals(2) && message.Content.ToLower().StartsWith("hi"))
                {

                    using var welcomeService = new WelcomeProviderService();
                    var greeting = await welcomeService.GetRandomWelcomeMessageAsync();
                    var newMsg = greeting!.Message!.Replace("[member]", $"<@{author.Name}>").Replace("[server]", server.Name);
                    await message.ReplyAsync($"{newMsg}", true, false, null!);
                }
                if (message.Content.Contains("help", StringComparison.CurrentCultureIgnoreCase))
                {
                    embed.SetTitle($"<@{authorId}> asked for assistance.");
                    embed.SetDescription("I see you are in need of some help,\r\nwhat can I help you with today?\r\n\r\nreply to this message to get help.");
                    embed.SetFooter("Pinkerton always watching ");
                    embed.SetTimestamp(DateTime.Now);
                    await message.ReplyAsync(embed);
                }
            }
            catch(Exception e)
            {
                var log = new SystemError()
                {
                    ErrorCode = Guid.NewGuid(),
                    ErrorMessage = e.Message
                };
               await message.ReplyAsync($"{log.ErrorMessage}: please refer [`{log.ErrorCode}`] to a Bot Dev");
            }
            
        }

        public void Dispose()
        {
            DisposableBase disposableBase = new();
            disposableBase.Dispose();
        }
    }
}
