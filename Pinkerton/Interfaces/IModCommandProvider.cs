using Guilded.Base;
using Guilded.Content;
using Pinkerton.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinkerton.Interfaces
{
    public interface IModCommandProvider
    {
        Task<Result<bool, SystemError>> AddWarningAsync(string serverId, string memId, string reason);
        Result<List<Infraction>, SystemError> GetMemberInfractions(string serverId, string memberId);
        Result<bool, SystemError> RemoveWarning(string serverId, string memId);
        Result<Infraction, SystemError> AddInfraction(string serverId, string memId, string reason);
        Task<Result<List<Message>, SystemError>> CollectMemberMessagesAsync(string serverId, string memId);
        Task<Result<bool, SystemError>> KickMemberAsync(string serverId, string memId, string reason);
        Task<Result<bool, SystemError>> BanMemberAsync(string serverId, string memId, string reason);
        Task<Result<bool, SystemError>> SetRoleAsync(string serverId, string memId, uint roleId);
        Task<Result<bool, SystemError>> RemoveRoleAsync(string serverId, string memId, uint roleId);
        Task<Result<bool, SystemError>> SetFilteredWordsAsync(string serverId, string[] words);
        Task<Result<List<string>, SystemError>> GetServerFilteredWords(string serverId);
        Task<Result<bool, SystemError>> SetServerLogChannelIdAsync(string serverID, Guid logChannelId);
    }
}
