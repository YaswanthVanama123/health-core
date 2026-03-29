using System.ComponentModel.DataAnnotations.Schema;

namespace GeniView.Data.Web
{
    [ComplexType]
    public class Address
    {
        public string? StreetLineOne { get; set; }

        public string? StreetLineTwo { get; set; }

        public string? City { get; set; }

        public string? Region { get; set; }

        public string? Country { get; set; }

        public string? PostalCode { get; set; }

        public string? Phone { get; set; }

        public string? Fax { get; set; }

        public string? Email { get; set; }

        public string? Website { get; set; }
    }
}
