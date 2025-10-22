using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    /// <summary>
    /// Represents a PlantCharacteristic from the datalake.
    /// </summary>
    public class PlantCharacteristic
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        public override string ToString()
        {
            return $"Id = {Id}, Name = {Name}";
        }
    }
}
