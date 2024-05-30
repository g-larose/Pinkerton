using Guilded.Content;
using Pinkerton.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinkerton.Models
{
    public class ServerConfig
    {
        public int Id { get; set; }
        public string? ServerId { get; set; }
        public string? ServerName { get; set; }
        public string? OwnerId { get; set; }
        public bool? IsFilterEnabled { get; set; } = false;
        public ServerType? Type { get; set; }
        public DateTime? CreatedAt { get; set; }
        public Guid? DefaultChannelId { get; set; }
        public Guid? LogChannel { get; set; }
        public List<GuildMessage>? Messages { get; set; }
        public string[]? FilteredWords { get; set; }


    }
}
