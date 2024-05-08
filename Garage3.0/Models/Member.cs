using Garage3.Helpers;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Garage3.Data
{
    public enum MembershipType
    {
        Regular_Member = 1,
        Pro_Member = 2
    }

    public class Member
    {
        public int Id { get; set; }

        [MaxLength(50)]
        [DisplayName("First Name")]
        public string FirstName { get; set; } = null!;

        [MaxLength(50)]
        [DisplayName("Last Name")]
        public string LastName { get; set; } = null!;

        [MaxLength(13)]
        [RegularExpression(@"\d{8}-?\d{4}", ErrorMessage = "Not a valid Personal Id Number. Use format YYYYMMDD-NNNN.")]
        [DisplayName("Personal Id Number")]
        public string PersonalIdentificationNumber { get; set; } = null!;

        // Membership Info
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Readonly property for membership
        [DisplayName("Membership Type")]
        public string MembershipType => MemberHelper.DisplayMemberType(EndDate);

        [DisplayName("Membership Days Left")]
        public double MembershipDaysLeft => (EndDate - DateTime.Today).TotalDays;

        // Create a new read-only property for the full name
        [DisplayName("Name")]
        public string FullName => $"{FirstName} {LastName}";

        // Relationships
        public List<Vehicle>? Vehicles { get; set; }
    }
}
