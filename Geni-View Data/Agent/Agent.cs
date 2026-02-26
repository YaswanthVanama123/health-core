using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;

namespace GeniView.Data.Agent
{
    [Index(nameof(AgentID), IsUnique = true)]
    public class Agent
    {
        public long ID { get; set; }

        [StringLength(40)]
        public string AgentID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string OperatingSystem { get; set; }

        public string AgentVersion { get; set; }

        public string AgentAddress { get; set; }

        public DateTime Timestamp { get; set; }

        static public Agent Default()
        {
            byte[] bytes = new byte[16];
            var rand = RandomNumberGenerator.Create();
            rand.GetBytes(bytes);

            var guid = new Guid(bytes);

            Agent agent = new Agent()
            {
                Name = "Default",
                AgentAddress="",
                AgentID = guid.ToString().ToUpper(),
                AgentVersion ="",
                Description = "",
                OperatingSystem = "",
                Timestamp = DateTime.Now,
            };


            return agent;
        }
    }
}
