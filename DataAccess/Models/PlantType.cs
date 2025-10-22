using System.ComponentModel.DataAnnotations;

using PlantStationHelperService;

namespace DataAccess.Models
{
    /// <summary>
    /// Represents a plant type with descriptive information for a specific group of plants.
    /// </summary>
    public class PlantType
    {
        /// <summary>
        /// Gets or sets the unique identifier of the plant type in the database. 
        /// Must be 0 for new entries.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the Name of the plant type.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the optical characteristics of a plant.
        /// For example: "Huge green leafes with big holes".
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets a list of special characteristics for that type of plants. 
        /// For example: "Likes bright sunlight.
        /// </summary>
        public List<TypeCharacteristic> TypeCharacteristics { get; set; }

        public void Update(PlantType plantType)
        { 
            Name = plantType.Name;
            Description = plantType.Description;
            TypeCharacteristics = plantType.TypeCharacteristics;
        }

        public override string ToString()
        {
            return $"Id = {Id}, Name = {Name}, Description = {Description}, TypeCharacteristicts = {ListExtensions.ToDelimitedString(TypeCharacteristics)}";
        }
    }
}

//internal enum TypeCharacteristic
//{
//    GnarledTwistedTrunk,
//    BroadDenseCanopy,
//    OvalEvergreenLeaves,
//    CompactSlowGrowth,
//    BrightIndirectLight,
//    AvoidDrafts,
//    Prefers18To22Celsius,
//    Minimum15Celsius,
//    ModeratelyMoistSoil,
//    AvoidWaterlogging,
//    HighHumidity,
//    BenefitsFromMisting,
//    RegularPruning,
//    FertilizeEveryTwoWeeksInSeason,
//    NotFrostHardy,
//    PestResistant
//}


