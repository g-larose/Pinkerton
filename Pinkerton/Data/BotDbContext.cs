using Guilded.Content;
using Guilded.Servers;
using Microsoft.EntityFrameworkCore;
using Pinkerton.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinkerton.Data
{
    public class BotDbContext : DbContext
    {
        public DbSet<ServerConfig> Servers { get; set; }
        public DbSet<ServerMember> Members { get; set; }
        public DbSet<SystemError> Errors { get; set; }
        public DbSet<GuildMessage> Messages { get; set; }

        public BotDbContext(DbContextOptions options) : base(options) 
        { 
        }
        public BotDbContext() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }
    }
}
