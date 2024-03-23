using Guilded.Base;
using Guilded.Base.Embeds;
using Guilded.Commands;
using Guilded.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinkerton.Commands
{
    public class ModCommands: CommandModule
    {
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
    }
}
