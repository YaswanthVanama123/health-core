using GeniView.Data.Hardware;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeniView.Data.Web
{
    [Index(nameof(CommunityID), IsUnique = true)]
    public partial class Community
    {
        public long ID { get; set; }

        public Guid CommunityID { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(50)]
        [Display(Name = "Community Name")]
        public string Name { get; set; }

        [StringLength(160)]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public Address Address { get; set; } = new Address();

        [Display(Name = "Create Date")]
        public DateTime CreateDate { get; set; }

        #region Navigation Properties

        public virtual List<Battery> Batteries { get; set; }

        public virtual List<Device> Devices { get; set; }

        public virtual List<Group> Groups { get; set; }

        #endregion
    }
}
