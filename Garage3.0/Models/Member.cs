namespace Garage3.Data
{
    public class Member
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string PersonalIdentificationNumber { get; set; }

        // Create a new read-only property for the full name
        public string FullName => $"{FirstName} {LastName}";
        
        // Relationships
        public List<Vehicle> Vehicles { get; set; }
    }
}
