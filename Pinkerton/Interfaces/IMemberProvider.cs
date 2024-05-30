using Guilded.Base;
using Guilded.Client;
using Pinkerton.Models;

namespace Pinkerton.Interfaces
{
    public interface IMemberProvider
    {
        Task<Result<List<long>, SystemError>> GetMemberRolesAsync(AbstractGuildedClient client, HashId serverId, HashId memberId);
    }
}
