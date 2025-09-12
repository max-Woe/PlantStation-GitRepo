using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models;

/// <summary>
/// Represents an exceptions thrown while running the app.
/// </summary>
public class AppException
{
    /// <summary>
    /// Gets or sets the unique identifier for the thrown exception.
    /// </summary>
    [Key]
    public int id { get; set; }

    /// <summary>
    /// Gets or sets the exception message from the database.
    /// </summary>
    public string? ExceptionMessage { get; set; }

    /// <summary>
    /// gets or sets the time when the exception was captured in the database.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}