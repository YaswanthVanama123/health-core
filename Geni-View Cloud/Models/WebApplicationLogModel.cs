using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace GeniView.Cloud.Models
{
    public class WebApplicationLogModel
    {
        public long Id { get; set; }
        public DateTime Timestamp { get; set; }
        [StringLength(50)]
        public string Level { get; set; }
        public string Message { get; set; }
        [StringLength(120)]
        public string UserName { get; set; }
        public string ServerName { get; set; }
        public string Port { get; set; }
        public string Url { get; set; }
        public bool Https { get; set; }
        public string ServerAddress { get; set; }
        public string Logger { get; set; }
        public string Callsite { get; set; }
        public string Exception { get; set; }
        public string InnerException { get; set; }
        public string ExceptionData { get; set; }

    }
}