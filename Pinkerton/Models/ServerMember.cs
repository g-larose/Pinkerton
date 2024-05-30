using Guilded.Servers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinkerton.Models
{
    public class ServerMember
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? ServerId { get; set; }
        public string? MemberId { get; set; }
        public int Warnings { get; set; }
        public DateTime? KickedAt { get; set; }
        public DateTime? BannedAt { get; set; }
        public bool IsBanned { get; set; }
        public IList<string>? Roles { get; set; }
        public ICollection<Infraction>? Infractions { get; set; }

    }
}
