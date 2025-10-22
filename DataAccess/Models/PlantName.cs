using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    /// <summary>
    /// Represents the german name of a plant
    /// </summary>
    public class PlantName
    {
        ///<summary>
        ///Gets or sets the unique identifier.
        ///</summary>
        [Key]
        public int Id { get; set; }

        ///<summary>
        ///Gets or sets the scientific plant name.
        /// </summary>
        public string? NameSci {  get; set; }

        ///<summary>
        ///Gets or sets the english plant name.
        /// </summary>
        public string? NameEng { get; set; }

        ///<summary>
        ///Gets or sets the spanish plant name.
        /// </summary>
        public string? NameSp { get; set; }

        ///<summary>
        ///Gets or sets the german plant name.
        /// </summary>
        public string? NameGer { get; set; }

        ///<summary>
        ///Gets or sets the french plant name.
        ///</summary>
        public string? NameFr { get; set; }

        public override string ToString()
        {
            return $"Id = {Id}, NameSci = {NameSci}, NameEng = {NameEng}, NameSp = {NameSp}, NameGer = {NameGer}, NameFr = {NameFr}";
        }
    }
}
