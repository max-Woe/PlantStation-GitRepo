using Microsoft.EntityFrameworkCore;
using DataAccess;
using DataAccess.Models;
using Moq;
using LoggingService;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using DataAccess.Interfaces;

namespace DataAccessUnitTest
{
    public class MeasurmentTest
    {
        private static ApiContext GetDbContext()
        {
            // Erstellt für jeden Test eine einzigartige In-Memory-Datenbank
            var options = new DbContextOptionsBuilder<ApiContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dbContext = new ApiContext(options);
            dbContext.Database.EnsureCreated();

            return dbContext;
        }

        [Fact]
        public async Task Create_ShouldAddMeasurementSuccessfully()
        {
            // Arrange
            var dbContext = GetDbContext();
            var loggerMock = new Mock<ILoggingService>(); // Oder dein konkretes ILogger-Interface
            var stationRepoMock = new Mock<IStationRepo>();
            SensorRepo sensorRepo = new SensorRepo(dbContext, loggerMock.Object, stationRepoMock.Object);

            MeasurementRepo measurementRepo = new MeasurementRepo(dbContext, loggerMock.Object, sensorRepo, stationRepoMock.Object);
            var measurement = new Measurement { Id = 1, Value = 10.5 };

            // Act
            var result = await measurementRepo.Create(measurement);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(measurement.Value, result.Value);

            // Überprüfe den Datenbankinhalt
            var addedMeasurement = dbContext.Measurements.Find(1);
            Assert.NotNull(addedMeasurement);
            Assert.Equal(measurement.Value, addedMeasurement.Value);

            // VERIFIZIERUNG DER LOGGING-AUFRUFE
            // Prüft, ob die StartTimer-Methode genau einmal aufgerufen wurde
            loggerMock.Verify(x => x.StartTimer(), Times.Once);
            // Prüft, ob StopTimerAndLog mit den korrekten Werten aufgerufen wurde
            loggerMock.Verify(x => x.Log("Measurement", "created", It.IsAny<Measurement>()), Times.Once);
        }

        [Fact]
        public async Task GetById_ExistingId_ReturnsMeasurement()
        {
            // Arrange
            var dbContext = GetDbContext();
            var loggerMock = new Mock<ILoggingService>();
            var stationRepoMock = new Mock<IStationRepo>();
            SensorRepo sensorRepo = new SensorRepo(dbContext, loggerMock.Object, stationRepoMock.Object);
            MeasurementRepo measurementRepo = new MeasurementRepo(dbContext, loggerMock.Object, sensorRepo, stationRepoMock.Object);
            var measurementId = 1;
            var expectedMeasurement = new Measurement { Id = measurementId, Value = 10.5 , Unit = "°C"};

            // Fügen Sie das erwartete Element der Datenbank hinzu
            await dbContext.Measurements.AddAsync(expectedMeasurement);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await measurementRepo.GetById(measurementId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedMeasurement.Id, result.Id);
            Assert.Equal(expectedMeasurement.Value, result.Value);

            //// VERIFIZIERUNG DER LOGGING-AUFRUFE
            //loggerMock.Verify(x => x.StartTimer(), Times.Once);
            //loggerMock.Verify(x => x.StopTimerAndLog("Measurement", "queried", It.IsAny<Measurement>()), Times.Once);
        }

        [Fact]
        public async Task GetById_NonExistingId_ReturnsNull()
        {
            // Arrange
            var dbContext = GetDbContext();
            var loggerMock = new Mock<ILoggingService>();
            var stationRepoMock = new Mock<IStationRepo>();
            SensorRepo sensorRepo = new SensorRepo(dbContext, loggerMock.Object, stationRepoMock.Object);
            MeasurementRepo measurementRepo = new MeasurementRepo(dbContext, loggerMock.Object, sensorRepo, stationRepoMock.Object);
            var nonExistingId = 99;

            // Act
            var result = await measurementRepo.GetById(nonExistingId);

            // Assert
            Assert.Null(result);

            //// VERIFIZIERUNG DER LOGGING-AUFRUFE
            //// Die Logging-Methode sollte nicht aufgerufen werden, wenn kein Element gefunden wird
            //loggerMock.Verify(x => x.StartTimer(), Times.Never);
            //loggerMock.Verify(x => x.StopTimerAndLog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Measurement>()), Times.Never);
        }
    }
}