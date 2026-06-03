
using System.ComponentModel.DataAnnotations;
using DataAccess.Interfaces;

namespace DataAccess.Models;

public class AnalogMeasurement:IJsonSerializable
{
    private DateTime _recordedAt = DateTime.UtcNow;
    
    [Key]
    public int Id { get; set; }
    
    public int Value { get; set; }
    
    public string Unit { get; set; }

    public int SensorId{ get; set; }
    
    public int StationId{ get; set; }
    
    public DateTime RecordedAt { get => _recordedAt; set => _recordedAt = value.ToUniversalTime(); }

    public void Update(AnalogMeasurement analogMeasurement)
    {
        Id = analogMeasurement.Id;
        Value = analogMeasurement.Value;
        Unit = analogMeasurement.Unit;
        SensorId = analogMeasurement.SensorId;
        StationId = analogMeasurement.StationId;
        RecordedAt = analogMeasurement.RecordedAt;
    }
    
    public string ToStrign()
    {
        string analogMeasurementString = $"Id = {Id}, " +
                                         $"Value= {Value}, " +
                                         $"Unit= ({Unit}, ), " +
                                         $"SensorId = {SensorId}, " +
                                         $"StationId = {StationId}, " +
                                         $"RecordedAt = {RecordedAt}"; 
        
        return analogMeasurementString;
    }
}