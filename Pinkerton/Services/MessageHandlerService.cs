using Guilded;
using Guilded.Base;
using Guilded.Client;
using Guilded.Content;
using Pinkerton.BaseModules;
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
            if (message.Content!.Length.Equals(2) && message.Content.ToLower().StartsWith("hi"))
            {
                var channelId = message.ChannelId;
                var authorId = message.CreatedBy;
                var serverId = message.ServerId;
                var server = await message.ParentClient.GetServerAsync((HashId)serverId!);
                var author = await message.ParentClient.GetMemberAsync((HashId)serverId!, authorId);
                using var welcomeService = new WelcomeProviderService();
                var greeting = await welcomeService.GetRandomWelcomeMessageAsync();
                var newMsg = greeting!.Message!.Replace("[member]", $"<@{author.Name}>").Replace("[server]", server.Name);
                await message.ReplyAsync($"{newMsg}", true, false, null!);
            }
        }

        public void Dispose()
        {
            DisposableBase disposableBase = new();
            disposableBase.Dispose();
        }
    }
}
