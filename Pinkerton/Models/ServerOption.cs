using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinkerton.Models
{
    public class ServerOption
    {
        public int Id { get; set; }
        public string? ServerId { get; set; }
        public ServerConfig? Server { get; set; }
        public bool? AntiSpam { get; set; }
        public bool? AntiLink { get; set; }
        public bool? AntiRaid { get; set; }
        public int CoolDown { get; set; }


    }
}
