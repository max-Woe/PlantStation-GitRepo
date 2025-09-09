using Xunit;
using Moq;
using DataAccess.Repositories;
using DataAccess.Models;
using DataAccess;
using DataAccess.Interfaces;
using LoggingService;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

public class MeasurementRepoTests
{
    private readonly Mock<ApiContext> _contextMock;
    private readonly Mock<ILoggingService> _loggerMock;
    private readonly Mock<ISensorRepo> _sensorRepoMock;
    private readonly Mock<IStationRepo> _stationRepoMock;
    private readonly MeasurementRepo _repo;

    public MeasurementRepoTests()
    {
        _contextMock = new Mock<ApiContext>(new DbContextOptions<ApiContext>());
        _loggerMock = new Mock<ILoggingService>();
        _sensorRepoMock = new Mock<ISensorRepo>();
        _stationRepoMock = new Mock<IStationRepo>();
        _repo = new MeasurementRepo(_contextMock.Object, _loggerMock.Object, _sensorRepoMock.Object, _stationRepoMock.Object);
    }

    [Fact]
    public async Task Create_ReturnsNull_WhenMeasurementIsNull()
    {
        var result = await _repo.Create(null as Measurement);
        Assert.Null(result);
    }

    [Fact]
    public async Task Create_AddsMeasurement_WhenValid()
    {
        var measurement = new Measurement { Id = 1, SensorId = 1 };
        _sensorRepoMock.Setup(r => r.GetById(It.IsAny<int>())).ReturnsAsync(new Sensor { Id = 1 });
        _contextMock.Setup(c => c.Measurements.AddAsync(measurement, default)).ReturnsAsync((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Measurement>)null);

        var result = await _repo.Create(measurement);

        Assert.Equal(measurement, result);
        _loggerMock.Verify(l => l.Log("Measurement", "created", measurement), Times.Once);
    }

    [Fact]
    public async Task GetById_ReturnsNull_WhenIdIsInvalid()
    {
        var result = await _repo.GetById(0);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetById_ReturnsMeasurement_WhenFound()
    {
        // Arrange
        var expectedMeasurement = new Measurement { Id = 1, Value = 100 };

        // Die Datenquelle für den Mock erstellen
        var measurements = new List<Measurement> { expectedMeasurement }.AsQueryable();

        // Das DbSet mocken, um FindAsync() zu simulieren
        var dbSetMock = new Mock<DbSet<Measurement>>();
        dbSetMock.As<IQueryable<Measurement>>().Setup(m => m.Provider).Returns(measurements.Provider);
        dbSetMock.As<IQueryable<Measurement>>().Setup(m => m.Expression).Returns(measurements.Expression);
        dbSetMock.As<IQueryable<Measurement>>().Setup(m => m.ElementType).Returns(measurements.ElementType);
        dbSetMock.As<IQueryable<Measurement>>().Setup(m => m.GetEnumerator()).Returns(measurements.GetEnumerator());
        dbSetMock.Setup(m => m.FindAsync(1)).ReturnsAsync(expectedMeasurement);

        // Den Context-Mock so konfigurieren, dass er das gemockte DbSet zurückgibt
        _contextMock.Setup(c => c.Measurements).Returns(dbSetMock.Object);

        // Act
        var result = await _repo.GetById(1);

    }

    [Fact]
    public async Task GetAllAsList_ReturnsMeasurements()
    {
        var measurements = new List<Measurement> { new Measurement { Id = 1 } };
        _contextMock.Setup(c => c.Measurements.ToListAsync(default)).ReturnsAsync(measurements);

        var result = await _repo.GetAllAsList();

        Assert.Equal(measurements, result);
    }

    [Fact]
    public async Task Update_UpdatesMeasurement_WhenFound()
    {
        var measurement = new Measurement { Id = 1 };
        var measurementFromDb = new Measurement { Id = 1 };
        _contextMock.Setup(c => c.Measurements.FindAsync(1)).ReturnsAsync(measurementFromDb);

        var result = await _repo.Update(measurement);

        Assert.Equal(measurementFromDb, result);
        _loggerMock.Verify(l => l.Log("Measurement", "updated", measurementFromDb), Times.Once);
    }

    [Fact]
    public async Task DeleteById_RemovesMeasurement_WhenFound()
    {
        var measurement = new Measurement { Id = 1 };
        _contextMock.Setup(c => c.Measurements.FindAsync(1)).ReturnsAsync(measurement);

        var result = await _repo.DeleteById(1);

        Assert.Equal(measurement, result);
    }

    // Weitere Tests für die restlichen Methoden analog aufbauen...
}
