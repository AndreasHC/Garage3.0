namespace Garage3.Data
{
    public partial class GarageContext
    {
        public class Member
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public DateTime DateOfBirth { get; set; }
            public string PersonalIdentificationNumber { get; set; }
            //public DateTime ProMembershipExpiration { get; set; }

            // Relationships
            public List<Vehicle> Vehicles { get; set; }
        }
    }
}
