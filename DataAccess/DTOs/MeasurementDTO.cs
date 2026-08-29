namespace DataAccess.DTOs;

public class MeasurementDto
{
    public long UnixTime { get; set; }
    
    public double Value { get; set; }
    
    public string? Unit { get; set; } = string.Empty;
    
    public string? Type { get; set; } = string.Empty;
    
    public int Pin { get; set; }
    
    public string? MacAddress { get; set; } = string.Empty;
}