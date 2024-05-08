using Garage3.Data;

namespace Garage3.Helpers
{
    public class VehiclesHelper
    {
/*
Det har även kommit in nya krav på prissättningen. Det är först en grundavgift som tas ut när 
fordonet parkeras. Samt olika taxor beroende på vilket fordon som parkerats. 
Sen har vi även medlemskap nivån som påverkar prissättningen. 
Fordon som tar upp en plats  
grundavgift + timpris * tid 
Fordon som tar upp två platser  
Fordon som tar upp tre eller fler  
grundavgift * 1.3 + timpris * 1.4 * tid 
grundavgift * 1.6 + timpris * 1.5 * tid 
Vid Pro medlemskap minskas grundavgiften med 10% för varje plats medlemmen tar upp i garaget 
när ett fordon checkas ut. Maximalt kan 40% rabatt uppnås på grundpriset. 
För större fordon rabatteras även den löpande kostnaden med 10% med Pro medlemskapet. 
Om Pro medlemskapet upphör under en parkeringsperiod beräknas priset fram till dess att det 
upphör enligt Pros prisplan resterande enligt ordinarie taxa.
*/
        public static int GetParkingCost(Member member, VehicleType vehicleType, DateTime startDate)
        {
            double coefficient = 1.0;
            double baseModifier = 1.0;
            double hourlyModifer = 1.0;
            double reduction = 1.0;
            double parkingDuration = (int)(DateTime.Today - startDate).TotalHours;

            if (!vehicleType.SizeIsInverted)
            {
                if (vehicleType.Size == 2)
                {
                    baseModifier = 1.3;
                    hourlyModifer = 1.4;
                }
                else if (vehicleType.Size >= 3)
                {
                    baseModifier = 1.6;
                    hourlyModifer = 1.5;
                }
            }

            // Check if member was Pro member when the vehicle was parked
            if (startDate < member.EndDate)
            {
                if (member.Vehicles is null) throw new InvalidOperationException($"Expected to find vehicles parked by member `{member.Id}`, but none were found.");

                // Calculate full Pro member reduction
                reduction = (double)Math.Min(member.Vehicles.Count(), 4) / 10.0;

                // Check if Pro membership has ended
                if (member.EndDate < DateTime.Today)
                {
                    double overlapingMembershipDuration = (int)(member.EndDate - startDate).TotalHours;

                    reduction *= 1 - (parkingDuration / overlapingMembershipDuration);
                }
            }

            double cost = 10 * baseModifier + 10 * hourlyModifer * parkingDuration;

            return (int)(cost / reduction);
        }
    }
}