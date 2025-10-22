using Microsoft.CodeAnalysis.Editing;
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

        /// <summary>
        /// Returns a string representation of the current object that includes its primary key (<c>Id</c>) and its descriptive name (<c>Name</c>).
        /// </summary>
        /// <returns>A string formatted as "Id = [Value], Name = [Value]".</returns>
        public override string ToString()
        {
            return $"Id = {Id}, Name = {Name}";
        }
    }
}
