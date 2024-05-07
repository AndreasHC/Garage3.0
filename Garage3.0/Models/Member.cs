using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Garage3.Data
{
    public class Member
    {
        public int Id { get; set; }
        
        [MaxLength(50)]
        [DisplayName("First Name")]
        public string FirstName { get; set; }
        
        [MaxLength(50)]
        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [MaxLength(13)]
        [RegularExpression(@"\d{8}-?\d{4}", ErrorMessage = "Not a valid Personal Id Number. Use format YYYYMMDD-NNNN.")]
        [DisplayName("Personal Id Number")]
        public string PersonalIdentificationNumber { get; set; }

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
