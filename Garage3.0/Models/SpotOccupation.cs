using Garage3.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Garage3.Models
{
    [PrimaryKey(nameof(VehicleId), nameof(SpotId))]

    public class SpotOccupation
    {
        public int VehicleId { get; set; }
        public int SpotId { get; set; }
        public Vehicle? Vehicle { get; set; }
    }
}
