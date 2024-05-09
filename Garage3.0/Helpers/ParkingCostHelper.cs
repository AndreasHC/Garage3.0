using Garage3.Data;
using Microsoft.EntityFrameworkCore;

namespace Garage3.Helpers
{
    public static class ParkingCostHelper
    {
        private const double BaseCost = 50;
        private const double HourlyRate = 20;

        public static (double TotalCost, double Savings) CalculateParkingCostAndSavings(
            Member member, 
            VehicleType vehicleType, 
            DateTime parkingTime)
        {
            // If vehicleType is null that means we aren't interested in this information.
            if (vehicleType is null) return (0.0, 0.0);

            double costMultiplier = 1;
            double timeMultiplier = 1;

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
            var discount = MembershipHelper.CalculateDiscount(member, parkingTime);
            var hourlyCostWithoutDiscount = HourlyRate * timeMultiplier;
            var hourlyCostWithDiscount = hourlyCostWithoutDiscount * (1 - discount);

            var proHours = member.EndDate > parkingTime ? Math.Min((member.EndDate - parkingTime).TotalHours, hoursParked) : 0;
            var nonProHours = hoursParked - proHours;

            var baseCostProportionForPro = BaseCost * costMultiplier * (proHours / hoursParked);
            var baseCostProportionForNonPro = BaseCost * costMultiplier * (nonProHours / hoursParked);

            var proCost = baseCostProportionForPro + (hourlyCostWithDiscount * proHours);
            var nonProCost = baseCostProportionForNonPro + (hourlyCostWithoutDiscount * nonProHours);

            var totalCostWithDiscount = proCost + nonProCost;

            var totalCostWithoutDiscount = BaseCost * costMultiplier + (hourlyCostWithoutDiscount * hoursParked);
            var savings = totalCostWithoutDiscount - totalCostWithDiscount;
            savings = savings < 0 ? 0 : savings; // Säkerställ att sparade pengar inte är negativa

            totalCostWithDiscount = Math.Round(totalCostWithDiscount, 2);
            savings = Math.Round(savings, 2);

            return (totalCostWithDiscount, savings);
        }

        public static int CountOccupiedSpots(Member member)
        {
            if (member.Vehicles is null) return 0;

            var smallSizes = member.Vehicles?.Select(v => v.VehicleType).Where(v => v.SizeIsInverted).GroupBy(v => v.Size).Select(group => group.Key);

            int fullyOccupiedSpots = 0;
            foreach (int size in smallSizes)
            {
                // count (minimal) number of spots required to hold all small vehicles of this size (number of spots used may be larger)
                var count = Math.Ceiling((double)(member.Vehicles?.Select(v => v.VehicleType).Where(v => v.SizeIsInverted & v.Size == size).Count() / size));
                fullyOccupiedSpots += (int)count;
            }

            // count number of spots used to hold non-small vehicles
            fullyOccupiedSpots += member.Vehicles.Select(v => v.VehicleType).Where(v => !v.SizeIsInverted).Sum(v => v.Size);
            return fullyOccupiedSpots;
        }
    }
}
