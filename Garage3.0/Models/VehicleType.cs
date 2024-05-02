namespace Garage3.Data
{
    public partial class GarageContext
    {
        public class VehicleType
        {
            public int Id { get; set; }
            public string Name { get; set; }

            // Relationships
            public List<Vehicle> Vehicles { get; set; }
        }
    }
}
