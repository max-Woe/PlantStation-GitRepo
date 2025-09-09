using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models;

/// <summary>
/// Represents a station.
/// </summary>
public class Station
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the mac adress of the board, which collets the measurements.
    /// </summary>
    public string? MacAddress { get; set; }

    /// <summary>
    /// Gets or sets the location.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Gets or sets the count of sensors.
    /// </summary>
    public int SensorsCount { get; set; }

    /// <summary>
    /// Gets or sets the time when the station was first created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Updates the station including mac address, location and count of sensors.
    /// </summary>
    /// <param name="station"></param>
    public void Update(Station station)
    {
        MacAddress = station.MacAddress;
        Location = station.Location;
        SensorsCount = station.SensorsCount;
    }
}