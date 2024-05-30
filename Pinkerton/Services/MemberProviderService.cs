using Guilded.Base;
using Guilded.Client;
using Pinkerton.BaseModules;
using Pinkerton.Interfaces;
using Pinkerton.Models;

namespace Pinkerton.Services
{
    public class MemberProviderService : IMemberProvider, IDisposable
    {

        #region GET MEMBER ROLES
        public async Task<Result<List<long>, SystemError>> GetMemberRolesAsync(AbstractGuildedClient client, HashId serverId, HashId memberId)
        {
            var roleList = new List<long>();
            var server = await client.GetServerAsync(serverId);
            var roles = await client.GetMemberRolesAsync(serverId, memberId);
            foreach (var role in roles)
            {
                var r = await client.GetRoleAsync(serverId, role);
                roleList.Add(r.Id);
            }
            if (roleList.Count > 0)
            {
                return Result<List<long>, SystemError>.Ok(roleList)!;
            }
            else
            {
                return Result<List<long>, SystemError>.Err(new SystemError()
                {
                    ErrorCode = SystemErrors.GetError("Empty Array", Guid.NewGuid()), 
                    ErrorMessage = "the result returned an empty list.",
                    ServerId = serverId.ToString(),
                    ServerName = server.Name
                })!;
            }

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
