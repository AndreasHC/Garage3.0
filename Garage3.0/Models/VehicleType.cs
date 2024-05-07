﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Garage3.Data
{
    public class VehicleType
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string Name { get; set; }
        [DisplayName("Number of Wheels")]
        public int NumberOfWheels { get; set; }

        // Relationships
        public List<Vehicle>? Vehicles { get; set; }
    }
}
