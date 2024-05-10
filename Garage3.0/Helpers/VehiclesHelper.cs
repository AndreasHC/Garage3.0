namespace Garage3.Helpers
{
    public class VehiclesHelper
    {
        public static int GetParkingCost(TimeSpan parkingDuration)
        {
            return Math.Max(((int)parkingDuration.TotalSeconds) / 1000, 10);
        }
    }
}