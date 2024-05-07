using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Garage3.Data
{
    public class Vehicle
    {
        public int Id { get; set; }
        [MaxLength(10)]
        [DisplayName("Registration Number")]
        public string RegistrationNumber { get; set; } = null!;
        [MaxLength(50)]
        public string Color { get; set; } = null!;
        [MaxLength(50)]
        public string Brand { get; set; } = null!;
        [MaxLength(50)]
        public string Model { get; set; } = null!;
        [DisplayName("Parked Since")]
        public DateTime ParkingTime { get; set; }

        // Relationships
        public int VehicleTypeId { get; set; }
        public Member? Owner { get; set; }
        public int OwnerId { get; set; }
        [DisplayName("Vehicle Type")]
        public VehicleType? VehicleType { get; set; }
        
    }
}
