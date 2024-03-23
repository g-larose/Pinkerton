using Pinkerton.BaseModules;
using Pinkerton.Interfaces;
using Pinkerton.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pinkerton.Services
{
    public class WelcomeProviderService: IWelcomeProvider, IDisposable
    {
        public void Dispose()
        {
            DisposableBase disposableBase = new();
            disposableBase.Dispose();
        }

        public async Task<WelcomeMessage> GetRandomWelcomeMessageAsync()
        {
            var jFile = await File.ReadAllTextAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "json", "greetings.json"));
            var json = JsonSerializer.Deserialize<WelcomeRoot>(jFile);
            var rnd = new Random();
            var index = rnd.Next(1, json!.Messages!.Count);

            var message = json.Messages[index].Message;
            var emoji = json.Messages[index].Emoji;

            var welcomeMessage = new WelcomeMessage();
            welcomeMessage.Emoji = emoji;
            welcomeMessage.Message = message;

            return welcomeMessage;

        }

    }
}
