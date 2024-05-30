using Guilded.Commands;

namespace Pinkerton.Commands
{
    public class DevCommands: CommandModule
    {
        [Command(Aliases = [ "announce" ])]
        [Description("creates a server wide announcement")]
        public async Task Announce(CommandEvent invokator, string[] announcement)
        {
            if (announcement is null)
            {

            }
            else
            {
                try
                {
                    var cmdAuthorId = invokator.Message.CreatedBy;
                    var serverId = invokator.ServerId;

                    if (!cmdAuthorId.ToString().Equals("mq1ezklm"))
                    {
                        // only the dev team can send server announcements.

                    }
                }
                catch(Exception e)
                {

                }
            }
        }
    }
}
