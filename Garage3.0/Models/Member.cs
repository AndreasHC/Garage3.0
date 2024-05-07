using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Garage3.Data
{
    public enum MembershipType
    {
        RegularMember,
        ProMember
    }

    public class Membership
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public MembershipType Type { get; set; }    // Regular, Pro

        // Navigation property to member
        public Member Member { get; set; }

        // Foreign key to Member
        public int MemberId { get; set; }

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

        // Create a new read-only property for the full name
        [DisplayName("Name")]
        public string FullName => $"{FirstName} {LastName}";

        // Read-only property for now.
        [DisplayName("Membership Type")]
        public string MembershipType => "Ordinary";

        // Relationships
        public List<Vehicle>? Vehicles { get; set; }

       
    }
}
