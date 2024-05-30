using Guilded;
using Guilded.Base;
using Guilded.Base.Embeds;
using Guilded.Client;
using Guilded.Content;
using Guilded.Permissions;
using Humanizer;
using Pinkerton.BaseModules;
using Pinkerton.Factories;
using Pinkerton.Models;
using RestSharp.Serializers.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pinkerton.Services
{

    public class MessageHandlerService : IDisposable
    {
        public string? Message { get; set; }
        private Message? lastMessage { get; set; }
        private readonly AbstractGuildedClient? _client;

        private readonly PinkertonDbContextFactory _dbFactory;
        private List<Message> spam = new();

        private string? timePattern = "hh:mm:ss tt";

        public MessageHandlerService(PinkertonDbContextFactory dbFactory, AbstractGuildedClient client)
        {
            _dbFactory = dbFactory;
            _client = client;
        }


        #region BANNED WORDS DETECTOR
        public bool BannedWordDetector(Message msg)
        {
            var serverId = msg.ServerId;
            //check for banned word in message
            var isSafeWord = IsSafeWordEnabled(serverId.ToString()!, msg);
            if (isSafeWord)
            {
                return true;
            }
            return false;
        }
        #endregion

        #region HANDLE MESSAGE
        public async Task HandleMessage(Message message)
        {
            var interval = new TimeSpan();
            try
            {
                this.Message = message?.Content;
                var authorId = message!.CreatedBy;
                var serverId = message.ServerId;
                var server = await message.ParentClient.GetServerAsync((HashId)serverId!);
                var channelId = message.ChannelId;
                var embed = new Embed();
                var author = await message.ParentClient.GetMemberAsync((HashId)serverId!, authorId);

                if (author.IsBot) return;

                var permissions = await message.ParentClient.GetMemberPermissionsAsync((HashId)serverId!, authorId);
                var isSpam = false;

                //spam.Add(message);
                //if (spam.Count >= 5)
                //{
                //    foreach (var item in spam)
                //    {
                //        if (item.CreatedBy.Equals(message.CreatedBy) && item.ServerId.Equals(message.ServerId))
                //        {
                //            isSpam = true;
                //        }
                //        else
                //        {
                //            isSpam = false;
                //        }
                           
                //    }

                //    if (isSpam)
                //    {
                //        try
                //        {
                //            interval = spam[4].CreatedAt.Second.Seconds().Subtract(spam[0].CreatedAt.Second.Seconds());
                //            //var interval = spam[4].CreatedAt.Subtract(spam[0].CreatedAt);
                //            if (interval <= TimeSpan.FromSeconds(5))
                //            {
                //                using var modService = new ModCommandProviderService(_dbFactory, _client!);
                //                serverId = spam[4].ServerId;
                //                authorId = spam[4].CreatedBy;
                //                author = await spam[4].ParentClient.GetMemberAsync((HashId)serverId!, authorId);
                //                await message.ReplyAsync($"spam detected, `{author.Name}` please stop spamming the chat!");
                //                var infractionResult = modService.AddInfraction(serverId.ToString()!, authorId.ToString(), "spamming chat");

                //                if (infractionResult.IsOk)
                //                {
                //                    embed.SetDescription($"<@{authorId}> you have received an infraction for spamming the chat\r\n\r\n" +
                //                        $"don't agree with this infration - refer `{infractionResult.Value.Identifier}` to a moderator for further review");
                //                    await message.ReplyAsync(true, false, embed);
                //                }
                //                foreach (var msg in spam)
                //                {
                //                    await msg.DeleteAsync();
                //                    await Task.Delay(200);
                //                }
                //                spam.Clear();
                //                isSpam = false;
                //            }
                //            else
                //            {
                //                spam.Clear();
                //                isSpam = false;
                //            }

                //        }
                //        catch (Exception e)
                //        {
                //            using var db = _dbFactory.CreateDbContext();
                //            var error = new SystemError()
                //            {
                //                ErrorCode = Guid.NewGuid(),
                //                ErrorMessage = e.Message,
                //                ServerId = serverId.ToString(),
                //                ServerName = server.Name
                //            };
                //            db.Add(error);
                //            await db.SaveChangesAsync();
                //            spam.Clear();
                //            isSpam = false;
                //        }
                //        spam.Clear();
                //        isSpam = false;
                //    }
                //    spam.Clear();
                       
                //}


                //filter the links from the message
                var filtered = await FilterMessageAsync(message);
                if (!filtered.IsOk)
                {
                    await message.DeleteAsync();
                    await message.ReplyAsync($"{author.Name} {filtered.Error} and has been removed.");
                }
            }
            catch (Exception e)
            {
                var time = DateTime.Now.ToString(timePattern);
                var date = DateTime.Now.ToShortDateString();
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"[{date}][{time}][ERROR]  [{message.ParentClient.Name}] {e.Message} [MessageHandler event]\r\nspam list count {spam.Count}");
                Console.WriteLine($"Interval: {interval}");
            }

        }

        #endregion

        #region FILTER MESSAGE
        private async Task<Result<bool, string>> FilterMessageAsync(Message msg)
        {
            //7zZsro9PvWHQG64UX8nQGt61zZikoCAg
            var pattern = @"(?:https?|ftp):\/\/(?:[\w-]+\.)+[\w-]+(?:\/[\w@?^=%&/~+#-]*)?|guilded\.gg|discord\.gg";
            var regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var postReqValues = new Dictionary<string, string>()
            {
                {"stricktness", "0" },
                {"fast", "true" }
            };

            HttpClient client = new HttpClient();
            var content = new FormUrlEncodedContent(postReqValues);

            foreach (Match match in regex.Matches(msg.Content))
            {
                string url = match.Value.Replace(":", "%3A").Replace("/", "%2F");
                HttpResponseMessage response = await client.PostAsync($"https://www.ipqualityscore.com/api/json/url/7zZsro9PvWHQG64UX8nQGt61zZikoCAg/{url}", content);
                var responseString = await response.Content.ReadAsStringAsync();
                UrlScannerResponse parsedResponse = JsonSerializer.Deserialize<UrlScannerResponse>(responseString)!;

                if (parsedResponse.adult.Equals(true) || parsedResponse.@unsafe.Equals(true))
                {
                    return Result<bool, string>.Err("link found to be unsafe and/or adult content")!;
                }
            }

            return Result<bool, string>.Ok(true)!;
        }
        #endregion

        #region IS SAFE WORD
        private bool IsSafeWordEnabled(string serverId, Message msg)
        {
            using var db = _dbFactory.CreateDbContext();
            var isEnabled = db.Servers.Where(x => x.ServerId.Equals(serverId))
                .FirstOrDefault();
            if (isEnabled!.IsFilterEnabled == true)
            {
                var words = db.Servers.Where(x => x.ServerId!.Equals(serverId))
                                      .Select(x => x.FilteredWords)
                                      .FirstOrDefault();
                if (words is not null)
                {
                    var values = words.SelectMany(x => x.Split(",", StringSplitOptions.RemoveEmptyEntries));
                    foreach (var w in values)
                    {
                        if (msg.Content!.Contains(w))
                            return true;
                    }
                }
            }
            
            return false;
        }
        #endregion

        #region DISPOSE
        public void Dispose()
        {
            DisposableBase disposableBase = new();
            disposableBase.Dispose();
        }
        #endregion
    }
}
