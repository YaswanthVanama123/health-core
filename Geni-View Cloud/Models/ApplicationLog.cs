using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace GeniView.Cloud.Models
{
    public class ApplicationLog
    {
        [Key]
        public long Id { get; set; }
        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime Logged { get; set; }
        [Required]
        [MaxLength(50)]
        public string Level { get; set; }
        [Required]
        [MaxLength(250)]
        public string Message { get; set; }
        public string UserName { get; set; }
        public string ServerName { get; set; }
        public string Port { get; set; }
        public string Url { get; set; }
        public bool Https { get; set; }
        [MaxLength(100)]
        public string ServerAddress { get; set; }
        [MaxLength(100)]
        public string Logger { get; set; }
        public string Callsite { get; set; }
        public string Exception { get; set; }
        public string InnerException { get; set; }
        public string ExceptionData { get; set; }

    }


    public class ApplicationLogsFilter
    {
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Count { get; set; }
        public ApplicationLogLevel LogLevel { get; set; }
        public List<ApplicationLog> ApplicationLogList { get; set; }
    }

    public enum ApplicationLogLevel
    {
        ALL = 0,
        ERROR = 1,
        WARN = 2,
        INFO = 3,
      //  Debug = 4
    }
}