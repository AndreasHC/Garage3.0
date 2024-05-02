namespace Garage3.Data
{
    public partial class GarageContext
    {
        public class Vehicle
        {
            public int Id { get; set; }
            public string RegistrationNumber { get; set; }
            public int VehicleTypeId { get; set; }
            public VehicleType VehicleType { get; set; }
            public DateTime ParkingTime { get; set; }

            // Relationships
            public Member Owner { get; set; }

        }
    }
}
