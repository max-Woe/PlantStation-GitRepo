using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models;

/// <summary>
/// Represents a station.
/// </summary>
public class Station
{
    private DateTime _createdAt = DateTime.UtcNow;
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
    public DateTime CreatedAt
    {
        get => _createdAt; 
        set => _createdAt = value.ToUniversalTime();
    }

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

    public override string ToString()
    {
        return $"Id: {Id}, MacAddress: {MacAddress}, Location: {Location}, SensorsCount: {SensorsCount}, CreatedAt: {CreatedAt}";
    }
}