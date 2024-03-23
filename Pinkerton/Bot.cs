using Guilded;
using Guilded.Base;
using Guilded.Base.Embeds;
using Guilded.Commands;
using Pinkerton.Commands;
using Pinkerton.Models;
using Pinkerton.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
        private static readonly string? timePattern = "hh:mm:ss tt";

        public async Task RunAsync()
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            await using var client = new GuildedBotClient(token!)
                .AddCommands(new ModCommands(), prefix!)
                .AddCommands(new MemberCommands(), prefix!);

            using var messageHandler = new MessageHandlerService();

            #region PREPARED
            client.Prepared
                .Subscribe(async me =>
                {
                    var server = await me.ParentClient.GetServerAsync(new HashId("R0KmBAXj"));
                    var channelId = Guid.Parse("109d26d2-2d4d-46c0-9a7c-89deb2f23a9c");
                    var embed = new Embed();
                    embed.SetTitle("Pinkerton has connected!");
                    embed.SetColor(Color.DarkRed);
                    //await server.ParentClient.CreateMessageAsync((Guid)channelId!, embed);
                });
            #endregion

            #region MESSAGE CREATED
            client.MessageCreated
                .Subscribe(async msg =>
                {
                    await messageHandler.HandleMessage(msg.Message);
                });
            #endregion

            #region MESSAGE DELETED
            client.MessageDeleted
                .Subscribe(async msg =>
                {
                    var time = DateTime.Now.ToString(timePattern);
                    var date = DateTime.Now.ToLongDateString();
                    var server = await msg.ParentClient.GetServerAsync(new HashId("R0KmBAXj"));
                    var channelId = Guid.Parse("109d26d2-2d4d-46c0-9a7c-89deb2f23a9c");
                    var channel = await msg.ParentClient.GetChannelAsync((Guid)msg.ChannelId);
                    
                    var embed = new Embed();
                    embed.SetDescription($"[{time}] [{date}] <@{msg.CreatedBy}> deleted message [{msg.Content}] from {channel.Name}");
                    embed.SetFooter("Pinkerton ");
                    embed.SetTimestamp(DateTime.Now);
                    //await msg.ParentClient.CreateMessageAsync(channel.Id, embed);
                });
            #endregion

            #region MEMBER UPDATED
            client.MemberUpdated
                .Subscribe(async member =>
                {

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
