using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Garage3.Data
{
    public class Member
    {
        public int Id { get; set; }
        
        [MaxLength(50)]
        public string FirstName { get; set; }
        
        [MaxLength(50)]
        public string LastName { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [MaxLength(13)]
        [RegularExpression(@"\d{8}-?\d{4}", ErrorMessage = "Not a valid Personal Id Number. Use format YYYYMMDD-NNNN.")]
        [DisplayName("Personal Id Number")]
        public string PersonalIdentificationNumber { get; set; }
        
        // Create a new read-only property for the full name
        public string FullName => $"{FirstName} {LastName}";

        // Read-only property for now.
        public string MembershipType => "Ordinary";

        // Relationships
        public List<Vehicle>? Vehicles { get; set; }
    }
}
