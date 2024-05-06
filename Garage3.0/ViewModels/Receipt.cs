using Garage3.Data;
using System.Security.Permissions;

namespace Garage3.ViewModels
{
    public class Receipt
    {
        public string RegistrationNumber { get; set; }
        public DateTime ParkingTime { get; set; }
        public DateTime CheckoutTime { get; set; }
        public TimeSpan ParkingDuration => CheckoutTime - ParkingTime;
        public int Price => ((int)ParkingDuration.TotalSeconds) / 1000;
        public string ParkerName { get; set; }
        public Receipt(Vehicle vehicle, string parkerName)
        {
            RegistrationNumber = vehicle.RegistrationNumber;
            ParkingTime = vehicle.ParkingTime;
            CheckoutTime = DateTime.Now;
            ParkerName = parkerName;
        }
    }
}
