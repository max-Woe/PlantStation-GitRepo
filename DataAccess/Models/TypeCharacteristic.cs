using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    /// <summary>
    /// Represents the TypeCharacteristics table in the datalake.
    /// </summary>
    public class TypeCharacteristic
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

        //TODO: evtl special features einführen, falls sie direkten Einfluss auf Meldugnen o.Ä. haben.
    }
}
