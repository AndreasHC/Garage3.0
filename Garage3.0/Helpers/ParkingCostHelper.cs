using Garage3.Data;

namespace Garage3.Helpers
{
    public static class ParkingCostHelper
    {
        private const double BaseCost = 50;
        private const double HourlyRate = 20;

        public static (double TotalCost, double Savings) CalculateParkingCostAndSavings(Member member, VehicleType vehicleType, DateTime parkingTime)
        {
            double costMultiplier = 1;
            double timeMultiplier = 1;
            double savings = 0;

            if (!vehicleType.SizeIsInverted)
            {
                if (vehicleType.Size == 2)
                {
                    costMultiplier = 1.3;
                    timeMultiplier = 1.4;
                }
                else if (vehicleType.Size >= 3)
                {
                    costMultiplier = 1.6;
                    timeMultiplier = 1.5;
                }
            }

            var hoursParked = (DateTime.Now - parkingTime).TotalHours;
            var discount = MembershipHelper.CalculateDiscount(member, vehicleType.Size);
            var baseCostWithDiscount = (BaseCost * costMultiplier) * (1 - discount);
            var hourlyCostWithDiscount = (HourlyRate * timeMultiplier) * (1 - discount);
            var totalCostWithoutDiscount = (BaseCost * costMultiplier) + (HourlyRate * timeMultiplier * hoursParked);
            var totalCost = baseCostWithDiscount + (hourlyCostWithDiscount * hoursParked);
            savings = totalCostWithoutDiscount - totalCost;

            if (member.EndDate < DateTime.Now)
            {
                var proHours = (member.EndDate - parkingTime).TotalHours;
                var nonProHours = hoursParked - proHours;
                var proCost = (BaseCost * costMultiplier * proHours) + (HourlyRate * timeMultiplier * proHours);
                var nonProCost = (BaseCost * nonProHours) + (HourlyRate * nonProHours);
                totalCost = proCost + nonProCost;
                savings = (BaseCost * costMultiplier * hoursParked) + (HourlyRate * timeMultiplier * hoursParked) - totalCost;
            }

            return (totalCost, savings);
        }
    }
}
