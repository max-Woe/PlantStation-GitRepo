using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using DataAccess.Interfaces;

namespace DataAccess.Models;

/// <summary>
/// Represents a single measurement of a sensor.
/// </summary>
public class Measurement:IJsonSerializable
{
    private DateTime _recordedAt = DateTime.UtcNow;
    private DateTime _createdAt = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    public double Value { get; set; }

    /// <summary>
    /// Gets or sets the unit.
    /// </summary>
    public string Unit { get; set; }
    
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public string Type { get; set; }   

    /// <summary>
    /// Gets or sets the sensor id.
    /// </summary>
    public int SensorId { get; set; }  //TODO: Sind sensorId und sensorReferenceId redundant?

    /// <summary>
    /// Gets or sets the Sensor.
    /// </summary>
    public Sensor? Sensor { get; set; }

    /// <summary>
    /// Gets or sets the reference id between sensor and measurement.
    /// </summary>
    public int SensorIdReference { get; set; }  //TODO: Sind sensorId und sensorReferenceId redundant?

    /// <summary>
    /// Gets or sets the time when the measurement was recorded from the sensor.
    /// </summary>
    public DateTime RecordedAt
    {
        get => _recordedAt;
        set => _recordedAt = value.ToUniversalTime();
    }

    /// <summary>
    /// Gets or sets the time when the measurement was captured in the database.
    /// </summary>
    public DateTime CreatedAt
    {
        get => _createdAt;
        set => _createdAt = value.ToUniversalTime();
    }

    /// <summary>
    /// Updates the modifiable properties of this measurement
    /// based on the values from another measurement instance.
    /// Does not modify the creation timestamp.    
    /// </summary>
    /// <param name="measurement">
    /// A <see cref="Measurement"/> instance containing the new values.
    /// </param>
    public void Update(Measurement measurement)
    {
        Value = measurement.Value;
        SensorId = measurement.SensorId;
        SensorIdReference = measurement.SensorIdReference;
        RecordedAt = measurement.RecordedAt;
    }

    public override string ToString()
    {
        string measurementAsString = $"Value = {Value}, " +
                                     $"Unit = {Unit}, " +
                                     $"SensorId = {SensorId}, " +
                                     $"SensorIdReference = {SensorIdReference}";
        return measurementAsString;
    }
}