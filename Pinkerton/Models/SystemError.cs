﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinkerton.Models
{
    public class SystemError
    {
        public int Id { get; set; }
        public Guid ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ServerName { get; set; }
        public string? ServerId { get; set; }

    }
}
