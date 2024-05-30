using Guilded.Base;
using Guilded.Client;
using Guilded.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Pinkerton.BaseModules;
using Pinkerton.Enums;
using Pinkerton.Factories;
using Pinkerton.Interfaces;
using Pinkerton.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace Pinkerton.Services
{
    public class ModCommandProviderService : IModCommandProvider, IDisposable
    {
        private readonly PinkertonDbContextFactory _dbFactory;
        private readonly AbstractGuildedClient _client;

        public ModCommandProviderService(PinkertonDbContextFactory dbFactory, AbstractGuildedClient client)
        {
            _dbFactory = dbFactory;
            _client = client;
        }

        #region GET MEMBER INFRACTIONS
        public Result<List<Infraction>, SystemError> GetMemberInfractions(string serverId, string memberId)
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();
                var member = db.Members.Where(x => x.ServerId.Equals(serverId) && x.MemberId.Equals(memberId))
                    .Include(x => x.Infractions)
                    .FirstOrDefault();
                if (member is null)
                {
                    return Result<List<Infraction>, SystemError>.Err(new SystemError()
                    {
                        ErrorCode = SystemErrors.GetError("Not Found", Guid.NewGuid()),
                        ErrorMessage = "Member Not Found",
                        ServerId = serverId

                    })!;
                }
                else
                {
                    var list = new List<Infraction>();
                    foreach (var i in member.Infractions!)
                    {
                        list.Add(i);
                    }
                    if (list.Count < 1)
                        return Result<List<Infraction>, SystemError>.Err(new SystemError()
                        {
                            ErrorCode = SystemErrors.GetError("No Elements in List", Guid.NewGuid()),
                            ErrorMessage = "there were no elements in the Infractions list.",
                            ServerId = serverId
                        })!;
                    else
                    {
                        return Result<List<Infraction>, SystemError>.Ok(list)!;
                    }
                }
            }
            catch(Exception e)
            {
                var error = new SystemError()
                {
                    ErrorCode = SystemErrors.GetError(e.Message, Guid.NewGuid()),
                    ErrorMessage = e.Message,
                    ServerId = serverId

                };
                return Result<List<Infraction>, SystemError>.Err(error)!;
            }
            
        }
        #endregion

        #region ADD WARNING
        public async Task<Result<bool, SystemError>> AddWarningAsync(string serverId, string memId, string reason)
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();
                var member = db.Members.Where(x => x.ServerId.Equals(serverId) && x.MemberId!.Equals(memId))
                    .Include(x => x.Infractions)
                    .FirstOrDefault();
                if (member is null)
                {
                    var error = new SystemError()
                    {
                        ErrorCode = SystemErrors.GetError("Not Found", Guid.NewGuid()),
                        ErrorMessage = "Member Not Found.",
                        ServerId = serverId
                    };
                    return Result<bool, SystemError>.Err(error)!;
                }
                else
                {
                    member.Warnings += 1;
                   
                    var infraction = new Infraction()
                    {
                        Identifier = Guid.NewGuid(),
                        CreatedAt = DateTime.UtcNow,
                        Reason = reason,
                        ServerId = serverId,
                        ServerMember = member,
                        ServerMemberId = member.Id,
                        Type = InfractionType.HARRASSMENT
                    };
                    member.Infractions!.Add(infraction);
                    db.Members.Update(member);
                    db.SaveChanges();

                    if (member.Warnings >= 4)
                        await BanMemberAsync(serverId, memId, reason);
                    return true;
                }
            }
            catch(Exception e)
            {
                return Result<bool, SystemError>.Err(new SystemError()
                {
                    ErrorCode = SystemErrors.GetError(e.Message, Guid.NewGuid()),
                    ErrorMessage = e.Message,
                    ServerId = serverId
                })!;
            }
        }
        #endregion

        #region ADD INFRACTION
        public Result<Infraction, SystemError> AddInfraction(string serverId, string memId, string reason)
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();
                var member = db.Members.Where(x => x.ServerId!.Equals(serverId) && x.MemberId!.Equals(memId))
                    .Include(x => x.Infractions)
                    .FirstOrDefault();
                if (member is null)
                {
                    return Result<Infraction, SystemError>.Err(new SystemError()
                    {
                        ErrorCode = SystemErrors.GetError("Not Found", Guid.NewGuid()),
                        ErrorMessage = "the provided member was not found.",
                        ServerId = serverId
                    })!;
                }
                else
                {
                    var infraction = new Infraction()
                    {
                        Identifier = Guid.NewGuid(),
                        CreatedAt = DateTime.UtcNow,
                        Reason = reason,
                        ServerId = serverId, 
                        ServerMemberId = member.Id,
                        ServerMember = member,
                        Type = InfractionType.DEFAULT
                    };
                    member.Infractions!.Add(infraction);
                    db.Members.Update(member);
                    db.SaveChanges();
                    return Result<Infraction, SystemError>.Ok(infraction)!;
                }
            }
            catch(Exception e)
            {
                return Result<Infraction, SystemError>.Err(new SystemError()
                {
                    ErrorCode = SystemErrors.GetError(e.Message, Guid.NewGuid()),
                    ErrorMessage = e.Message,
                    ServerId = serverId
                })!;
            }
        }
        #endregion

        #region BAN MEMBER
        public async Task<Result<bool, SystemError>> BanMemberAsync(string serverId, string memId, string reason)
        {
            var sId = HashId.TryParse(serverId, out HashId newId);
            var mId = HashId.TryParse(memId, out HashId newMemId);
            if (sId || mId)
            {
                await _client.AddMemberBanAsync(newId, newMemId, reason);
                return Result<bool, SystemError>.Ok(true)!;
            }
            else
            {
                var error = new SystemError()
                {
                    ErrorCode = SystemErrors.GetError("Incorrect Format", Guid.NewGuid()),
                    ErrorMessage = "the provided Id was not in the correct format.",
                };
                return Result<bool, SystemError>.Err(error)!;
            }
            
        }
        #endregion

        #region COLLECT MEMBER MESSAGES
        public Task<Result<List<Message>, SystemError>> CollectMemberMessagesAsync(string serverId, string memId)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region KICK MEMBER
        public Task<Result<bool, SystemError>> KickMemberAsync(string serverId, string memId, string reason)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region REMOVE ROLE
        public Task<Result<bool, SystemError>> RemoveRoleAsync(string serverId, string memId, uint roleId)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region REMOVE WARNING
        public Result<bool, SystemError> RemoveWarning(string serverId, string memId)
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();
                var member = db.Members.Where(x => x.ServerId!.Equals(serverId) && x.MemberId!.Equals(memId))
                    .FirstOrDefault();

                if (member is not null)
                {
                    member.Warnings -= 1;
                    db.Members.Update(member);
                    db.SaveChanges();
                    return Result<bool, SystemError>.Ok(true)!;
                }
                else
                {
                    var error = new SystemError()
                    {
                        ErrorCode = SystemErrors.GetError("Not Found", Guid.NewGuid()),
                        ErrorMessage = "the mentioned member was not in the database.",
                        ServerId = serverId
                    };
                    return Result<bool, SystemError>.Err(error)!;
                }
            }
            catch(Exception e)
            {
                var error = new SystemError()
                {
                    ErrorCode = SystemErrors.GetError(e.Message, Guid.NewGuid()),
                    ErrorMessage = e.Message,
                    ServerId = serverId
                };
                return Result<bool, SystemError>.Err(error)!;
            }
        }
        #endregion

        #region SET FILTERED WORDS
        public async Task<Result<bool, SystemError>> SetFilteredWordsAsync(string serverId, string[] words)
        {
            using var db = _dbFactory.CreateDbContext();
            try
            {
                var server = db.Servers.Where(x => x.ServerId!.Equals(serverId)).FirstOrDefault();
                var count = 0;
                var newWords = new List<string>();
                if (server.FilteredWords is not null)
                {
                    var details = server.FilteredWords.SelectMany(x => x.Split(",", StringSplitOptions.RemoveEmptyEntries));
                    foreach (var w in details)
                    {
                        newWords.Add(w);
                    }
                    foreach (var item in words)
                    {
                        newWords.Add(item);
                    }

                    server.FilteredWords = newWords.ToArray();
                    db.Servers.Update(server!);
                    await db.SaveChangesAsync();
                    return true;

                }
               
            }
            catch(Exception e)
            {
                var error = new SystemError()
                {
                    ErrorCode = SystemErrors.GetError(e.Message, Guid.NewGuid()),
                    ErrorMessage = e.Message,
                    ServerId = serverId
                };
                db.Errors.Add(error);
                await db.SaveChangesAsync();
                return Result<bool, SystemError>.Err(error)!;

            }
            return false;
        }
        #endregion

        #region SET ROLE
        public Task<Result<bool, SystemError>> SetRoleAsync(string serverId, string memId, uint roleId)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region SET LOG CHANNEL
        public async Task<Result<bool, SystemError>> SetServerLogChannelIdAsync(string serverID, Guid logChannelId)
        {
            using var db = _dbFactory.CreateDbContext();
            var server = db.Servers.Where(x => x.ServerId!.Equals(serverID)).FirstOrDefault();

            try
            {

                if (server is not null)
                {
                    server.LogChannel = logChannelId;
                    db.Servers.Update(server);
                    await db.SaveChangesAsync();
                    return Result<bool, SystemError>.Ok(true)!;
                }
                else
                    return Result<bool, SystemError>.Ok(false)!;
            }
            catch(Exception e)
            {
                var error = new SystemError()
                {
                    ErrorCode = SystemErrors.GetError(e.Message, Guid.NewGuid()),
                    ErrorMessage = e.Message,
                    ServerId = serverID
                };
                db.Errors.Add(error);
                await db.SaveChangesAsync();
                return Result<bool, SystemError>.Err(error)!;
            }
            
        }
        #endregion

        #region GET FILTERED WORDS

        public async Task<Result<List<string>, SystemError>> GetServerFilteredWords(string serverId)
        {
            using var db = _dbFactory.CreateDbContext();
            try
            {
               
                var words = db.Servers.Where(x => x.ServerId!.Equals(serverId.ToString()))
                    .Select(x => x.FilteredWords)
                    .FirstOrDefault()!
                    .ToList();
                if (words is not null)
                    return Result<List<string>, SystemError>.Ok(words)!;
                
                return Result<List<string>, SystemError>.Err(new SystemError()
                {
                    ErrorCode = SystemErrors.GetError("Not Found", Guid.NewGuid()),
                    ErrorMessage = "Could not find filtered word list.",
                    ServerId = serverId
                })!;
                
            }
            catch(Exception e)
            {
                var error = new SystemError()
                {
                    ErrorCode = SystemErrors.GetError(e.Message, Guid.NewGuid()),
                    ErrorMessage = e.Message,
                    ServerId = serverId
                };
                db.Errors.Add(error);
                await db.SaveChangesAsync();
                return Result<List<string>, SystemError>.Err(error)!;
                
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
