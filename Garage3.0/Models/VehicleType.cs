﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Garage3.Data
{
    public class VehicleType
    {
        public int Id { get; set; }

        [MaxLength(50)]
        public string Name { get; set; } = null!;

        [DisplayName("Number of Wheels")]
        [Range(0, int.MaxValue)]
        public int NumberOfWheels { get; set; }
        public int Size {  get; set; }
        [DisplayName("Size is Vehicles per Spot")]
        public bool SizeIsInverted { get; set; }

        // Relationships
        public List<Vehicle>? Vehicles { get; set; }
    }
}
