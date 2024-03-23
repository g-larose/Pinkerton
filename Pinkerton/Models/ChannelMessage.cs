using Guilded.Servers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinkerton.Models
{
    public class ChannelMessage
    {
        public int Id { get; set; }
        public Guid ChannelId { get; set; }
        public string? ServerId { get; set; }
        public Member? Author { get; set; }
        public string? CreatedAt { get; set; }
        public string? Message { get; set; }

    }
}
