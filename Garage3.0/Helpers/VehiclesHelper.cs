using Garage3.Data;
namespace Garage3.Helpers
{
    public class VehiclesHelper
    {
        public static int GetParkingCost(VehicleType vehicleType, TimeSpan parkingDuration)
        {
            double coefficient = 1.0;
            if (!vehicleType.SizeIsInverted)
            {

            }
            return Math.Max(((int)parkingDuration.TotalSeconds) / 1000, 10);
        }
    }
}