using System.ComponentModel;

namespace Garage3.ViewModels
{
    public class StatisticsViewModel
    {
        [DisplayName("Vehicle Amount By Type")] 
        public Dictionary<string, int> VehicleCountByType { get; set; }

        [DisplayName("Total Number of Wheels")]
        public int TotalWheelsCount { get; set; }

        [DisplayName("Total Amount of Wheels")]
        public decimal TotalRevenue { get; set; }
    }
}
