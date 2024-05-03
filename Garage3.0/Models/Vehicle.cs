using System.ComponentModel.DataAnnotations;

namespace Garage3.Data
{
    public class Vehicle
    {
        public int Id { get; set; }
        [MaxLength(10)]
        public string RegistrationNumber { get; set; }
        [MaxLength(50)]
        public string Color { get; set; }
        [MaxLength(50)]
        public string Brand { get; set; }
        public DateTime ParkingTime { get; set; }

        // Relationships
        public int VehicleTypeId { get; set; }
        public Member? Owner { get; set; }
        public int OwnerId { get; set; }
        public VehicleType? VehicleType { get; set; }
    }
}
