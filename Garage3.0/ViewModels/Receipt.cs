using Garage3.Data;
using Garage3.Helpers;
using System.ComponentModel;
using System.Security.Permissions;

namespace Garage3.ViewModels
{
    public class Receipt
    {
        public Member Member { get; set; }
        public VehicleType VehicleType { get; }
        [DisplayName("Registration Number")]
        public string RegistrationNumber { get; set; }
        [DisplayName("Parked Since")]
        public DateTime ParkingTime { get; set; }
        [DisplayName("Checked Out")]
        public DateTime CheckoutTime { get; set; }
        [DisplayName("Parking Duration")]
        public TimeSpan ParkingDuration => CheckoutTime - ParkingTime;
        public int Price => VehiclesHelper.GetParkingCost(Member, VehicleType, ParkingTime);
        [DisplayName("Parker Name")]
        public string ParkerName { get; set; }
        public Receipt(Member member, Vehicle vehicle, string parkerName)
        {
            Member = member;
            VehicleType = vehicle.VehicleType!;
            RegistrationNumber = vehicle.RegistrationNumber;
            ParkingTime = vehicle.ParkingTime;
            CheckoutTime = DateTime.Now;
            ParkerName = parkerName;
        }
    }
}
