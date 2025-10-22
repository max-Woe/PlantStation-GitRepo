using System.ComponentModel.DataAnnotations;

using PlantStationHelperService;

namespace DataAccess.Models
{
    /// <summary>
    /// Represents an individual Plant and includes plant type and characterisics.
    /// </summary>
    public class Plant
    {
        /// <summary>
        /// Gets or sets the unique identifier of a plant in the database.
        /// </summary>
        [Key]
        public int Id { get; set; }
        
        /// <summary>
        /// Gets or sets the name (chosen by user) of the plant in the database.
        /// </summary>
        /// <value>Simplifies the identification for the user, by improved readability.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the plant type of the plant in the database.
        /// </summary>
        public PlantType? PlantType { get; set; }

        /// <summary>
        /// Gets or sets a list of characteristics in the database.
        /// </summary>
        /// <value>
        /// The characteristics are special for an individual plant. 
        /// For example: "Had a special illness".
        /// </value>
        public List<PlantCharacteristic>? PlantCharacteristics  { get; set; }

        /// <summary>
        /// Updates Name and PlantType of the entity.
        /// </summary>
        /// <param name="plant"></param>
        public void UpdatePlant(Plant plant)
        {
            Name = plant.Name;
            PlantType = plant.PlantType;
        }

        public override string ToString()
        {
            return $"Plant: {Id}, Name: {Name}, PlantType: {PlantType?.Name}, Characteristics: {ListExtensions.ToDelimitedString(PlantCharacteristics)}";
        }
    }
}
//internal enum PlantCharacteristic
//{
//    HighWaterNeeds,
//    AtypicalLeafColor,
//    SpontaneousAerialRoots,
//    FrostTolerance,
//    RareFlowering,
//    AsymmetricGrowth,
//    SlowCaudexResponse,
//    DeepShadeTolerance,
//    AbnormalLeafSize,
//    PestResistance
//}