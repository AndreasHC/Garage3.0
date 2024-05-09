using Garage3.Data;

namespace Garage3.Helpers
{
    public static class MembershipHelper
    {
        public static double CalculateDiscount(Member member, DateTime parkingTime)
        {
            if (member.EndDate < parkingTime) return 0;

            int numberOfSpots = ParkingCostHelper.CountOccupiedSpots(member);

            return 0.1 * Math.Min(4, numberOfSpots);
        }
    }
}
