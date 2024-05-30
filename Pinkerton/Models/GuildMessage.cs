using Guilded.Servers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinkerton.Models
{
    public class GuildMessage
    {
        public int Id { get; set; }
        public Guid Identifier { get; set; }
        public Guid? ChannelId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? Content { get; set; }
        public string? ServerId { get; set; }
        public string? MemberId { get; set; }

    }
}
