using Guilded;
using Guilded.Base;
using Pinkerton.Models;
using System;
using System.Collections.Generic;
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
            await using var client = new GuildedBotClient(token!);

            client.Prepared
                .Subscribe(async me =>
                {
                    Console.WriteLine("Pinkerton is connected");
                    var server = await me.ParentClient.GetServerAsync(new HashId("R0KmBAXj"));
                    var channelId = server.DefaultChannelId;
                    await server.ParentClient.CreateMessageAsync((Guid)channelId!, "Pinkerton has connected!");
                });
               


            await client.ConnectAsync();
            await client.SetStatusAsync("Watching Everything", 90002579);
           
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
