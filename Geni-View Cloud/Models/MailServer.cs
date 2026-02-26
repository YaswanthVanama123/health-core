using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;

namespace GeniView.Cloud.Models
{
    public class MailServer 
    {
        public long ID { get; set; }
        [Required]
        public string Host { get; set; }
        [Required]
        public int Port { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string User { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [DataType(DataType.EmailAddress)]
        public string ReplyTo { get; set; }
        public SmtpDeliveryMethod DeliveryMethod { get; set; }
        public bool EnableSsl { get; set; }

    }
}