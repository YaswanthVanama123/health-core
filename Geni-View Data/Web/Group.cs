using GeniView.Data.Hardware;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeniView.Data.Web
{
    [Index(nameof(GroupID), IsUnique = true)]
    public partial class Group
    {
        public long ID { get; set; }

        public Guid GroupID { get; set; }

        [Required(ErrorMessage = "Name cannot be empty or null.")]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(160)]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        [Display(Name = "Create Date")]
        public DateTime CreateDate { get; set; }

        [ForeignKey(nameof(Community))]
        public long CommunityID { get; set; }

        [ForeignKey(nameof(ParentGroup))]
        public long? ParentGroupID { get; set; }

        #region Navigation Properties

        public virtual List<Device> Devices { get; set; }

        public virtual List<Battery> Batteries { get; set; }

        public virtual Community Community { get; set; }

        public virtual Group ParentGroup { get; set; }

        #endregion
    }
}
