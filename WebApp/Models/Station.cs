using System.ComponentModel.DataAnnotations;

namespace WebApp
{
    public class Station
    {
        [Key]
        public int Id { get; set; }
        public string? MacAddress { get; set; }
        public string? Location { get; set; }
        public int SensorsCount { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}

