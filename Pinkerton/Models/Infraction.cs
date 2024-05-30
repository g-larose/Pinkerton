using Guilded.Servers;
using Pinkerton.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinkerton.Models
{
    public class Infraction
    {
        public int Id { get; set; }
        public Guid? Identifier { get; set; }
        public string? Reason { get; set; }
        public string? ServerId { get; set; }
        public InfractionType? Type { get; set; }
        public DateTime? CreatedAt { get; set; }
        public ServerMember? ServerMember { get; set; }
        public int? ServerMemberId { get; set; }

    }
}
