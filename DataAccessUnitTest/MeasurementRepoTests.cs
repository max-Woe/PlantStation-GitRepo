
using DataAccess;
using DataAccess.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories;
using LoggingService;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MeasurementRepoTests
{

    public class MeasurementRepoTests
    {
        private readonly Mock<IApiContext> _contextMock;
        private readonly Mock<ILoggingService> _loggerMock;
        private readonly Mock<ISensorRepo> _sensorRepoMock;
        private readonly Mock<IStationRepo> _stationRepoMock;
        private readonly MeasurementRepo _repo;
        private readonly Mock<DbSet<Measurement>> _dbSetMock;
        private readonly Mock<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Measurement>> _entityEntryMock;

        public MeasurementRepoTests()
        {
            _contextMock = new Mock<IApiContext>();
            _loggerMock = new Mock<ILoggingService>();
            _sensorRepoMock = new Mock<ISensorRepo>();
            _stationRepoMock = new Mock<IStationRepo>();
            _repo = new MeasurementRepo(_contextMock.Object, _loggerMock.Object, _sensorRepoMock.Object, _stationRepoMock.Object);
            _dbSetMock = new Mock<DbSet<Measurement>>();
            _entityEntryMock = new Mock<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Measurement>>();
        }


        #region // Hilfsklassen für das Testen von EF Core asynchronen Methoden
        public class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;

            public TestAsyncQueryProvider(IQueryProvider inner)
            {
                _inner = inner;
            }

            public IQueryable CreateQuery(Expression expression)
            {
                // Die Methode sollte ein IQueryable<T> zurückgeben
                // Dies erfordert, dass die Methode den Ausdruck in eine
                // abfragbare Form übersetzt
                return _inner.CreateQuery(expression);
            }

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            {
                // Explizite Konvertierung, um das Problem zu beheben
                return (IQueryable<TElement>)new TestAsyncEnumerable<TElement>(_inner.CreateQuery<TElement>(expression));
            }

            public object Execute(Expression expression)
            {
                return _inner.Execute(expression);
            }

            public TResult Execute<TResult>(Expression expression)
            {
                return _inner.Execute<TResult>(expression);
            }

            public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
            {
                var result = _inner.Execute<TResult>(expression);
                return new TestAsyncEnumerable<TResult>(new List<TResult> { result });
            }

            TResult IAsyncQueryProvider.ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
            {
                var result = Execute<TResult>(expression);
                return result;
            }
        }
        public class TestAsyncEnumerable<T> : IAsyncEnumerable<T>
        {
            private readonly IEnumerable<T> _enumerable;
            public TestAsyncEnumerable(IEnumerable<T> enumerable) => _enumerable = enumerable;
            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) => new TestAsyncEnumerator<T>(_enumerable.GetEnumerator());
        }
        public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _enumerator;
            public TestAsyncEnumerator(IEnumerator<T> enumerator) => _enumerator = enumerator;
            public T Current => _enumerator.Current;
            public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(_enumerator.MoveNext());
            public ValueTask DisposeAsync() => new ValueTask();
        }
        #endregion

        #region // Repo Test
        public class Create : MeasurementRepoTests
        {
            [Fact]
            public async Task Create_ReturnsNull_WhenMeasurementIsNull()
            {
                var result = await _repo.Create(null as Measurement);

                _loggerMock.Verify(l => l.LogSuccess("Nullcheck", "Create", null), Times.Never);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Nullcheck", "Create", null), Times.Once);
                Assert.Null(result);
            }

            [Fact]
            public async Task Create_AddsMeasurement_WhenValid()
            {
                //SETUP
                var measurement = new Measurement { Id = 1, SensorId = 1 };
                var measurementsList = new List<Measurement>();

                _dbSetMock.Setup(m => m.AddAsync(It.IsAny<Measurement>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Measurement>)null)
                         .Callback<Measurement, CancellationToken>((m, ct) => measurementsList.Add(m));

                _contextMock.Setup(c => c.Measurements).Returns(_dbSetMock.Object);
                _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

                //ACT
                var result = await _repo.Create(measurement);

                //ASSERT
                _loggerMock.Verify(l => l.LogSuccess("AddAsync", "Create", measurement), Times.Once);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "AddAsync", "Create", measurement), Times.Never);

                _loggerMock.Verify(l => l.LogSuccess("SaveChangesAsync", "Create", measurement), Times.Once);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "SaveChangesAsync", "Create", measurement), Times.Never);
                Assert.Equal(measurement, result);
            }

            [Fact]
            public async Task Create_CreatesNewSensor_WhenSensorIsNotFound()
            {
                // Arrange
                var measurement = new Measurement { Id = 1, SensorId = 1 };
                _sensorRepoMock.Setup(r => r.GetById(1)).ReturnsAsync((Sensor)null);
                _contextMock.Setup(c => c.Measurements.AddAsync(measurement, default)).ReturnsAsync((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Measurement>)null);
                _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

                // Act
                var result = await _repo.Create(measurement);

                // Assert
                _loggerMock.Verify(l => l.LogSuccess("SaveChangesAsync", "Create", measurement), Times.Once);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "SaveChangesAsync", "Create", measurement), Times.Never);
                Assert.Equal(measurement,result);
            }

            [Fact]
            public async Task Create_ReturnsNull_OnException()
            {
                // Arrange
                var measurement = new Measurement { Id = 1, SensorId = 1 };
                //_sensorRepoMock.Setup(r => r.GetById(It.IsAny<int>())).ReturnsAsync(new Sensor { Id = 1 });
                _contextMock.Setup(c => c.Measurements.AddAsync(It.IsAny<Measurement>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Database error"));

                // Act
                var result = await _repo.Create(measurement);

                // Assert
                _loggerMock.Verify(l => l.LogSuccess("AddAsync", "Create", measurement), Times.Never);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "AddAsync", "Create", measurement), Times.Once);
                Assert.Null(result);
            }

            [Fact]
            public async Task Create_ReturnsNull_OnExceptionInSaveChanges()
            {
                // Arrange
                var measurement = new Measurement { Id = 1, SensorId = 1 };
                //_sensorRepoMock.Setup(r => r.GetById(It.IsAny<int>())).ReturnsAsync(new Sensor { Id = 1 });
                _contextMock.Setup(c => c.Measurements.AddAsync(measurement, default)).ReturnsAsync((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Measurement>)null);
                _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Database error"));

                // Act
                var result = await _repo.Create(measurement);

                // Assert
                _loggerMock.Verify(l => l.LogSuccess("SaveChangesAsync", "Create", measurement), Times.Never);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "SaveChangesAsync", "Create", measurement), Times.Once);
                Assert.Null(result);

            }
        }
        public class CreateByList : MeasurementRepoTests
        {
            [Fact]
            public async Task Create_ReturnsEmptyList_WhenMeasurementListIsNull()
            {
                var result = await _repo.CreateByList(null as List<Measurement>);
                Assert.Empty(result);
            }

            [Fact]
            public async Task Create_AddsMeasurementList_WhenValid()
            {
                var measurements = new List<Measurement>();
                var measurement = new Measurement { Id = 1, SensorId = 1 };

                measurements.Add(measurement);

                _contextMock.Setup(c => c.Measurements.AddAsync(measurement, default)).ReturnsAsync((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Measurement>)null);
                _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
                
                var result = await _repo.CreateByList(measurements);
                
                _loggerMock.Verify(l => l.LogSuccess("SaveChangesAsync", "Create", measurements), Times.Once);
                _loggerMock.Verify(l => l.LogSuccess("AddAsync", "Create", It.IsAny<Measurement>()), Times.Once);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "SaveChangesAsync", "Create", It.IsAny<Measurement>()), Times.Never);

                Assert.Equal(measurements, result);
            }

            // Neuer Test: Create mit ungültigem SensorId
            [Fact]
            public async Task Create_CreatesNewSensor_WhenSensorIsNotFound()
            {
                // Arrange
                var measurements = new List<Measurement>();
                var measurement = new Measurement { Id = 1, SensorId = 999 };
                measurements.Add(measurement);

                _sensorRepoMock.Setup(r => r.GetById(999)).ReturnsAsync((Sensor)null);
                _contextMock.Setup(c => c.Measurements.AddAsync(measurement, default)).ReturnsAsync((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Measurement>)null);
                _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

                // Act
                var result = await _repo.CreateByList(measurements);

                // Assert
                _loggerMock.Verify(l => l.LogSuccess("AddAsync", "Create", measurement), Times.Once);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "AddAsync", "Create", measurement), Times.Never);

                _loggerMock.Verify(l => l.LogSuccess("SaveChangesAsync", "Create", measurements), Times.Once);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "SaveChangesAsync", "Create", measurement), Times.Never);
                Assert.Equal(measurements, result);
            }

            // Neuer Test: Create gibt Null zurück bei Ausnahme
            [Fact]
            public async Task Create_ReturnsEmptyList_OnExceptionInAddAsync()
            {
                // Arrange
                var measurements = new List<Measurement>();
                var measurement = new Measurement { Id = 1, SensorId = 1 };
                measurements.Add(measurement);

                _sensorRepoMock.Setup(r => r.GetById(It.IsAny<int>())).ReturnsAsync(new Sensor { Id = 1 });
                _contextMock.Setup(c => c.Measurements.AddAsync(It.IsAny<Measurement>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Database error"));

                // Act
                var expected = new List<Measurement>();
                var actual = await _repo.CreateByList(measurements);

                // Assert
                _loggerMock.Verify(l => l.LogSuccess("AddAsync", "Create", measurement), Times.Never);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "AddAsync", "Create", measurement), Times.Once);

                Assert.Equal(expected, actual);
            }
            [Fact]
            public async Task Create_ReturnsEmptyList_OnExceptionInSaveChangesAsync()
            {
                // Arrange
                var measurements = new List<Measurement>();
                var measurement = new Measurement { Id = 1, SensorId = 1 };
                measurements.Add(measurement);

                _sensorRepoMock.Setup(r => r.GetById(It.IsAny<int>())).ReturnsAsync(new Sensor { Id = 1 });
                _contextMock.Setup(c => c.Measurements.AddAsync(measurement, default)).ReturnsAsync((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Measurement>)null);
                _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Database error"));

                // Act
                var expected = new List<Measurement>();
                var actual = await _repo.CreateByList(measurements);

                // Assert
                _loggerMock.Verify(l => l.LogSuccess("AddAsync", "Create", measurement), Times.Once);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "AddAsync", "Create", measurement), Times.Never);

                _loggerMock.Verify(l => l.LogSuccess("SaveChangesAsync", "Create", measurements), Times.Never);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "SaveChangesAsync", "Create", measurements), Times.Once);
                Assert.Equal(expected, actual);
            }
        }

        public class Get : MeasurementRepoTests
        {
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
                var measurements = new List<Measurement> { expectedMeasurement }.AsQueryable();

                var dbSetMock = new Mock<DbSet<Measurement>>();
                dbSetMock.As<IQueryable<Measurement>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Measurement>(measurements.Provider));
                dbSetMock.As<IQueryable<Measurement>>().Setup(m => m.Expression).Returns(measurements.Expression);
                dbSetMock.As<IQueryable<Measurement>>().Setup(m => m.ElementType).Returns(measurements.ElementType);
                dbSetMock.As<IQueryable<Measurement>>().Setup(m => m.GetEnumerator()).Returns(measurements.GetEnumerator());
                dbSetMock.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync(expectedMeasurement);

                _contextMock.Setup(c => c.Measurements).Returns(dbSetMock.Object);

                // Act
                var result = await _repo.GetById(1);

                // Assert
                Assert.Equal(expectedMeasurement, result);
            }

            [Fact]
            public async Task GetAllAsList_ReturnsMeasurements()
            {
                // Arrange
                var measurements = new List<Measurement> { new Measurement { Id = 1, Value = 100 } };
                var mockDbSet = new Mock<DbSet<Measurement>>();

                mockDbSet.As<IAsyncEnumerable<Measurement>>().Setup(m => m.GetAsyncEnumerator(default)).Returns(new TestAsyncEnumerator<Measurement>(measurements.GetEnumerator()));
                mockDbSet.As<IQueryable<Measurement>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Measurement>(measurements.AsQueryable().Provider));
                mockDbSet.As<IQueryable<Measurement>>().Setup(m => m.Expression).Returns(measurements.AsQueryable().Expression);
                mockDbSet.As<IQueryable<Measurement>>().Setup(m => m.ElementType).Returns(measurements.AsQueryable().ElementType);
                mockDbSet.As<IQueryable<Measurement>>().Setup(m => m.GetEnumerator()).Returns(measurements.GetEnumerator());

                _contextMock.Setup(c => c.Measurements).Returns(mockDbSet.Object);

                var repo = new MeasurementRepo(_contextMock.Object, _loggerMock.Object, _sensorRepoMock.Object, _stationRepoMock.Object);

                // Act
                var result = await repo.GetAllAsList();

                // Assert
                Assert.Equal(measurements, result);
            }

            // Neuer Test: GetAllAsList gibt leere Liste bei keinen Daten zurück
            [Fact]
            public async Task GetAllAsList_ReturnsEmptyList_WhenNoMeasurements()
            {
                // Arrange
                var measurements = new List<Measurement>();
                var mockDbSet = new Mock<DbSet<Measurement>>();

                mockDbSet.As<IAsyncEnumerable<Measurement>>().Setup(m => m.GetAsyncEnumerator(default)).Returns(new TestAsyncEnumerator<Measurement>(measurements.GetEnumerator()));
                mockDbSet.As<IQueryable<Measurement>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Measurement>(measurements.AsQueryable().Provider));
                mockDbSet.As<IQueryable<Measurement>>().Setup(m => m.Expression).Returns(measurements.AsQueryable().Expression);
                mockDbSet.As<IQueryable<Measurement>>().Setup(m => m.ElementType).Returns(measurements.AsQueryable().ElementType);
                mockDbSet.As<IQueryable<Measurement>>().Setup(m => m.GetEnumerator()).Returns(measurements.GetEnumerator());

                _contextMock.Setup(c => c.Measurements).Returns(mockDbSet.Object);

                // Act
                var result = await _repo.GetAllAsList();

                // Assert
                Assert.Empty(result);
            }
        }
        public class GetAllAsList : MeasurementRepoTests
        {
            [Fact]
            public async Task GetAllAsList_ReturnsNull_WhenMeasurementIsNull()
            {
                var result = await _repo.Create(null as Measurement);
                Assert.Null(result);
            }

            [Fact]
            public async Task GetAllAsList_AddsMeasurement_WhenValid()
            {
                var measurement = new Measurement { Id = 1, SensorId = 1 };
                _sensorRepoMock.Setup(r => r.GetById(It.IsAny<int>())).ReturnsAsync(new Sensor { Id = 1 });
                _contextMock.Setup(c => c.Measurements.AddAsync(measurement, default)).ReturnsAsync((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Measurement>)null);

                var result = await _repo.GetById(measurement.Id);

                _loggerMock.Verify(l => l.LogSuccess("FindAsync", "GetById", null), Times.Once);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), (Measurement?)null), Times.Never);
                Assert.Equal(measurement, result);
            }

            // Neuer Test: Create mit ungültigem SensorId
            [Fact]
            public async Task GetAllAsList_ReturnsNull_WhenSensorIsNotFound()
            {
                // Arrange
                var measurement = new Measurement { Id = 1, SensorId = 999 };

                _sensorRepoMock.Setup(r => r.GetById(999)).ReturnsAsync((Sensor)null);

                // Act
                var result = await _repo.Create(measurement);

                // Assert
                _loggerMock.Verify(l => l.LogSuccess(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), measurement), Times.Once);
                Assert.Null(result);
            }

            // Neuer Test: Create gibt Null zurück bei Ausnahme
            [Fact]
            public async Task GetAllAsList_ReturnsNull_OnException()
            {
                // Arrange
                var measurement = new Measurement { Id = 1, SensorId = 1 };
                _sensorRepoMock.Setup(r => r.GetById(It.IsAny<int>())).ReturnsAsync(new Sensor { Id = 1 });
                _contextMock.Setup(c => c.Measurements.AddAsync(It.IsAny<Measurement>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Database error"));

                // Act
                var result = await _repo.Create(measurement);

                // Assert
                Assert.Null(result);
                _loggerMock.Verify(l => l.LogSuccess(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), measurement), Times.Once);

            }
        }
        public class GetById : MeasurementRepoTests
        {
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
                _loggerMock.Verify(l => l.LogSuccess(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), (Measurement?)null), Times.Never);
            }

            // Neuer Test: Create mit ungültigem SensorId
            [Fact]
            public async Task Create_ReturnsNull_WhenSensorIsNotFound()
            {
                // Arrange
                var measurement = new Measurement { Id = 1, SensorId = 999 };
                _sensorRepoMock.Setup(r => r.GetById(999)).ReturnsAsync((Sensor)null);

                // Act
                var result = await _repo.Create(measurement);

                // Assert
                Assert.Null(result);
                _loggerMock.Verify(l => l.LogSuccess(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), measurement), Times.Once);
            }

            // Neuer Test: Create gibt Null zurück bei Ausnahme
            [Fact]
            public async Task Create_ReturnsNull_OnException()
            {
                // Arrange
                var measurement = new Measurement { Id = 1, SensorId = 1 };
                _sensorRepoMock.Setup(r => r.GetById(It.IsAny<int>())).ReturnsAsync(new Sensor { Id = 1 });
                _contextMock.Setup(c => c.Measurements.AddAsync(It.IsAny<Measurement>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Database error"));

                // Act
                var result = await _repo.Create(measurement);

                // Assert
                Assert.Null(result);
                _loggerMock.Verify(l => l.LogSuccess(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), measurement), Times.Once);

            }
        }
        public class GetAllBySensorIdAsList : MeasurementRepoTests
        {
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
                _loggerMock.Verify(l => l.LogSuccess(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), (Measurement?)null), Times.Never);
            }

            // Neuer Test: Create mit ungültigem SensorId
            [Fact]
            public async Task Create_ReturnsNull_WhenSensorIsNotFound()
            {
                // Arrange
                var measurement = new Measurement { Id = 1, SensorId = 999 };
                _sensorRepoMock.Setup(r => r.GetById(999)).ReturnsAsync((Sensor)null);

                // Act
                var result = await _repo.Create(measurement);

                // Assert
                Assert.Null(result);
                _loggerMock.Verify(l => l.LogSuccess(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), measurement), Times.Once);
            }

            // Neuer Test: Create gibt Null zurück bei Ausnahme
            [Fact]
            public async Task Create_ReturnsNull_OnException()
            {
                // Arrange
                var measurement = new Measurement { Id = 1, SensorId = 1 };
                _sensorRepoMock.Setup(r => r.GetById(It.IsAny<int>())).ReturnsAsync(new Sensor { Id = 1 });
                _contextMock.Setup(c => c.Measurements.AddAsync(It.IsAny<Measurement>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Database error"));

                // Act
                var result = await _repo.Create(measurement);

                // Assert
                Assert.Null(result);
                _loggerMock.Verify(l => l.LogSuccess(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), measurement), Times.Once);

            }
        }
        public class GetByListOfIds : MeasurementRepoTests
        {
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
                _loggerMock.Verify(l => l.LogSuccess(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), (Measurement?)null), Times.Never);
            }

            // Neuer Test: Create mit ungültigem SensorId
            [Fact]
            public async Task Create_ReturnsNull_WhenSensorIsNotFound()
            {
                // Arrange
                var measurement = new Measurement { Id = 1, SensorId = 999 };
                _sensorRepoMock.Setup(r => r.GetById(999)).ReturnsAsync((Sensor)null);

                // Act
                var result = await _repo.Create(measurement);

                // Assert
                Assert.Null(result);
                _loggerMock.Verify(l => l.LogSuccess(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), measurement), Times.Once);
            }

            // Neuer Test: Create gibt Null zurück bei Ausnahme
            [Fact]
            public async Task Create_ReturnsNull_OnException()
            {
                // Arrange
                var measurement = new Measurement { Id = 1, SensorId = 1 };
                _sensorRepoMock.Setup(r => r.GetById(It.IsAny<int>())).ReturnsAsync(new Sensor { Id = 1 });
                _contextMock.Setup(c => c.Measurements.AddAsync(It.IsAny<Measurement>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Database error"));

                // Act
                var result = await _repo.Create(measurement);

                // Assert
                Assert.Null(result);
                _loggerMock.Verify(l => l.LogSuccess(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), measurement), Times.Once);

            }
        }
        public class GetAll : MeasurementRepoTests
        {
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
                _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

                var result = await _repo.Create(measurement);

                _loggerMock.Verify(l => l.LogSuccess("AddAsync", "Create", measurement), Times.Once);
                _loggerMock.Verify(l => l.LogSuccess("SaveChangesAsync", "Create", measurement), Times.Once);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "AddAsync", "Create", measurement), Times.Never);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "SaveChangesAsync", "Create", measurement), Times.Never);
                Assert.Equal(measurement, result);
            }

            // Neuer Test: Create mit ungültigem SensorId
            [Fact]
            public async Task Create_ReturnsNull_WhenSensorIsNotFound()
            {
                // Arrange
                var measurement = new Measurement { Id = 1, SensorId = 999 };
                _sensorRepoMock.Setup(r => r.GetById(999)).ReturnsAsync((Sensor)null);

                // Act
                var result = await _repo.Create(measurement);

                // Assert
                Assert.Null(result);
                _loggerMock.Verify(l => l.LogSuccess(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), measurement), Times.Once);
            }

            // Neuer Test: Create gibt Null zurück bei Ausnahme
            [Fact]
            public async Task Create_ReturnsNull_OnException()
            {
                // Arrange
                var measurement = new Measurement { Id = 1, SensorId = 1 };
                _sensorRepoMock.Setup(r => r.GetById(It.IsAny<int>())).ReturnsAsync(new Sensor { Id = 1 });
                _contextMock.Setup(c => c.Measurements.AddAsync(It.IsAny<Measurement>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Database error"));

                // Act
                var result = await _repo.Create(measurement);

                // Assert
                Assert.Null(result);
                _loggerMock.Verify(l => l.LogSuccess(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), measurement), Times.Once);

            }
        }

        public class Update : MeasurementRepoTests
        {
            [Fact]
            public async Task Update_UpdatesMeasurement_WhenFound()
            {
                var measurement = new Measurement { Id = 1, Value = 150 };
                var measurementFromDb = new Measurement { Id = 1, Value = 100 };
                _contextMock.Setup(c => c.Measurements.FindAsync(It.IsAny<object[]>())).ReturnsAsync(measurementFromDb);

                var result = await _repo.Update(measurement);

                Assert.Equal(measurementFromDb, result);
                _loggerMock.Verify(l => l.LogSuccess(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), measurement), Times.Never);
            }

            // Neuer Test: Update gibt Null zurück, wenn Messung nicht gefunden wird
            [Fact]
            public async Task Update_ReturnsNull_WhenNotFound()
            {
                // Arrange
                var measurement = new Measurement { Id = 999 };
                _contextMock.Setup(c => c.Measurements.FindAsync(It.IsAny<object[]>())).ReturnsAsync((Measurement)null);

                // Act
                var result = await _repo.Update(measurement);

                // Assert
                Assert.Null(result);
                _loggerMock.Verify(l => l.LogSuccess(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()), Times.Never);
                _loggerMock.Verify(l => l.LogInformationText($"Measurement with the id {measurement.Id} not found in database."), Times.Once);
            }
            // Neuer Test: Update gibt Null zurück bei Ausnahme
            [Fact]
            public async Task Update_ReturnsNull_OnException()
            {
                // Arrange
                var measurement = new Measurement { Id = 1, Value = 150 };
                var measurementFromDb = new Measurement { Id = 1, Value = 100 };
                _contextMock.Setup(c => c.Measurements.FindAsync(It.IsAny<object[]>())).ReturnsAsync(measurementFromDb);
                _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Database error"));

                // Act
                var result = await _repo.Update(measurement);

                // Assert
                Assert.Null(result);
                _loggerMock.Verify(l => l.LogSuccess("FindAsync", "Update", measurement), Times.Never);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Update", "Update", measurement), Times.Never);
                _loggerMock.Verify(l => l.LogSuccess("FindAsync", "Update", measurement), Times.Never);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Update", "Update", measurement), Times.Never);
                _loggerMock.Verify(l => l.LogSuccess("SaveChanges", "Update", measurement), Times.Never);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "SaveChanges", "Update", measurement), Times.Once);
                Assert.Null(result);
            }

        }

        public class UpdateByList : MeasurementRepoTests
        {
            [Fact]
            public async Task Update_UpdatesMeasurement_WhenFound()
            {
                var measurement = new Measurement { Id = 1, Value = 150 };
                var measurementFromDb = new Measurement { Id = 1, Value = 100 };
                _contextMock.Setup(c => c.Measurements.FindAsync(It.IsAny<object[]>())).ReturnsAsync(measurementFromDb);

                var result = await _repo.Update(measurement);

                Assert.Equal(measurementFromDb, result);
                _loggerMock.Verify(l => l.LogSuccess(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), measurement), Times.Never);
            }

            // Neuer Test: Update gibt Null zurück, wenn Messung nicht gefunden wird
            [Fact]
            public async Task Update_ReturnsNull_WhenNotFound()
            {
                // Arrange
                var measurement = new Measurement { Id = 999 };
                _contextMock.Setup(c => c.Measurements.FindAsync(It.IsAny<object[]>())).ReturnsAsync((Measurement)null);

                // Act
                var result = await _repo.Update(measurement);

                // Assert
                Assert.Null(result);
                _loggerMock.Verify(l => l.LogSuccess(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()), Times.Never);
                _loggerMock.Verify(l => l.LogInformationText($"Measurement with the id {measurement.Id} not found in database."), Times.Once);
            }
            // Neuer Test: Update gibt Null zurück bei Ausnahme
            [Fact]
            public async Task Update_ReturnsNull_OnException()
            {
                // Arrange
                var measurement = new Measurement { Id = 1, Value = 150 };
                var measurementFromDb = new Measurement { Id = 1, Value = 100 };
                _contextMock.Setup(c => c.Measurements.FindAsync(It.IsAny<object[]>())).ReturnsAsync(measurementFromDb);
                _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Database error"));

                // Act
                var result = await _repo.Update(measurement);

                // Assert
                Assert.Null(result);
                _loggerMock.Verify(l => l.LogSuccess("FindAsync", "Update", measurement), Times.Never);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Update", "Update", measurement), Times.Never);
                _loggerMock.Verify(l => l.LogSuccess("FindAsync", "Update", measurement), Times.Never);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Update", "Update", measurement), Times.Never);
                _loggerMock.Verify(l => l.LogSuccess("SaveChanges", "Update", measurement), Times.Never);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "SaveChanges", "Update", measurement), Times.Once);
                Assert.Null(result);
            }

        }

        public class Delete : MeasurementRepoTests
        {
            [Fact]
            public async Task DeleteById_RemovesMeasurement_WhenFound()
            {
                // Arrange
                var measurementId = 1;
                var measurement = new Measurement { Id = measurementId };
                var dbSetMock = new Mock<DbSet<Measurement>>();

                dbSetMock.Setup(x => x.FindAsync(It.IsAny<object[]>()))
                         .ReturnsAsync((object[] ids) => ids.Contains(measurementId) ? measurement : null);
                dbSetMock.Setup(x => x.Remove(measurement)).Verifiable();

                _contextMock.Setup(x => x.Measurements).Returns(dbSetMock.Object);
                _contextMock.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

                // Act
                await _repo.Delete(measurementId);

                // Assert
                dbSetMock.Verify(x => x.Remove(measurement), Times.Once);
                _contextMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
            }

            // Neuer Test: DeleteById löscht nichts, wenn Messung nicht gefunden wird
            [Fact]
            public async Task DeleteById_DoesNotRemove_WhenNotFound()
            {
                // Arrange
                var measurementId = 999;
                var dbSetMock = new Mock<DbSet<Measurement>>();
                dbSetMock.Setup(x => x.FindAsync(It.IsAny<object[]>())).ReturnsAsync((Measurement)null);

                _contextMock.Setup(x => x.Measurements).Returns(dbSetMock.Object);
                _contextMock.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(0);

                // Act
                await _repo.Delete(measurementId);

                // Assert
                dbSetMock.Verify(x => x.Remove(It.IsAny<Measurement>()), Times.Never);
                _contextMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Never); // Hier von 'Once' auf 'Never' ändern
            }

            // Neuer Test: DeleteById fängt Ausnahme ab und loggt
            [Fact]
            public async Task DeleteById_LogsError_OnException()
            {
                // Arrange
                var measurementId = 1;
                var measurement = new Measurement { Id = measurementId };
                var dbSetMock = new Mock<DbSet<Measurement>>();
                dbSetMock.Setup(x => x.FindAsync(It.IsAny<object[]>())).ReturnsAsync(measurement);
                dbSetMock.Setup(x => x.Remove(measurement)).Throws(new Exception("Remove error"));

                _contextMock.Setup(x => x.Measurements).Returns(dbSetMock.Object);
                _contextMock.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(0);

                // Act
                await _repo.Delete(measurementId);

                // Assert
                _loggerMock.Verify(l => l.LogSuccess(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
                //_loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Measurement>()), Times.Once);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);

            }
        }
        public class DeleteAll : MeasurementRepoTests
        {
            [Fact]
            public async Task DeleteById_RemovesMeasurement_WhenFound()
            {
                // Arrange
                var measurementId = 1;
                var measurement = new Measurement { Id = measurementId };
                var dbSetMock = new Mock<DbSet<Measurement>>();

                dbSetMock.Setup(x => x.FindAsync(It.IsAny<object[]>()))
                         .ReturnsAsync((object[] ids) => ids.Contains(measurementId) ? measurement : null);
                dbSetMock.Setup(x => x.Remove(measurement)).Verifiable();

                _contextMock.Setup(x => x.Measurements).Returns(dbSetMock.Object);
                _contextMock.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

                // Act
                await _repo.Delete(measurementId);

                // Assert
                dbSetMock.Verify(x => x.Remove(measurement), Times.Once);
                _contextMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
            }

            // Neuer Test: DeleteById löscht nichts, wenn Messung nicht gefunden wird
            [Fact]
            public async Task DeleteById_DoesNotRemove_WhenNotFound()
            {
                // Arrange
                var measurementId = 999;
                var dbSetMock = new Mock<DbSet<Measurement>>();
                dbSetMock.Setup(x => x.FindAsync(It.IsAny<object[]>())).ReturnsAsync((Measurement)null);

                _contextMock.Setup(x => x.Measurements).Returns(dbSetMock.Object);
                _contextMock.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(0);

                // Act
                await _repo.Delete(measurementId);

                // Assert
                dbSetMock.Verify(x => x.Remove(It.IsAny<Measurement>()), Times.Never);
                _contextMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Never); // Hier von 'Once' auf 'Never' ändern
            }

            // Neuer Test: DeleteById fängt Ausnahme ab und loggt
            [Fact]
            public async Task DeleteById_LogsError_OnException()
            {
                // Arrange
                var measurementId = 1;
                var measurement = new Measurement { Id = measurementId };
                var dbSetMock = new Mock<DbSet<Measurement>>();
                dbSetMock.Setup(x => x.FindAsync(It.IsAny<object[]>())).ReturnsAsync(measurement);
                dbSetMock.Setup(x => x.Remove(measurement)).Throws(new Exception("Remove error"));

                _contextMock.Setup(x => x.Measurements).Returns(dbSetMock.Object);
                _contextMock.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(0);

                // Act
                await _repo.Delete(measurementId);

                // Assert
                _loggerMock.Verify(l => l.LogSuccess(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
                //_loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Measurement>()), Times.Once);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);

            }
        }
        public class DeleteMeasurmentsBySensorId : MeasurementRepoTests
        {
            [Fact]
            public async Task DeleteById_RemovesMeasurement_WhenFound()
            {
                // Arrange
                var measurementId = 1;
                var measurement = new Measurement { Id = measurementId };
                var dbSetMock = new Mock<DbSet<Measurement>>();

                dbSetMock.Setup(x => x.FindAsync(It.IsAny<object[]>()))
                         .ReturnsAsync((object[] ids) => ids.Contains(measurementId) ? measurement : null);
                dbSetMock.Setup(x => x.Remove(measurement)).Verifiable();

                _contextMock.Setup(x => x.Measurements).Returns(dbSetMock.Object);
                _contextMock.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

                // Act
                await _repo.Delete(measurementId);

                // Assert
                dbSetMock.Verify(x => x.Remove(measurement), Times.Once);
                _contextMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
            }

            // Neuer Test: DeleteById löscht nichts, wenn Messung nicht gefunden wird
            [Fact]
            public async Task DeleteById_DoesNotRemove_WhenNotFound()
            {
                // Arrange
                var measurementId = 999;
                var dbSetMock = new Mock<DbSet<Measurement>>();
                dbSetMock.Setup(x => x.FindAsync(It.IsAny<object[]>())).ReturnsAsync((Measurement)null);

                _contextMock.Setup(x => x.Measurements).Returns(dbSetMock.Object);
                _contextMock.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(0);

                // Act
                await _repo.Delete(measurementId);

                // Assert
                dbSetMock.Verify(x => x.Remove(It.IsAny<Measurement>()), Times.Never);
                _contextMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Never); // Hier von 'Once' auf 'Never' ändern
            }

            // Neuer Test: DeleteById fängt Ausnahme ab und loggt
            [Fact]
            public async Task DeleteById_LogsError_OnException()
            {
                // Arrange
                var measurementId = 1;
                var measurement = new Measurement { Id = measurementId };
                var dbSetMock = new Mock<DbSet<Measurement>>();
                dbSetMock.Setup(x => x.FindAsync(It.IsAny<object[]>())).ReturnsAsync(measurement);
                dbSetMock.Setup(x => x.Remove(measurement)).Throws(new Exception("Remove error"));

                _contextMock.Setup(x => x.Measurements).Returns(dbSetMock.Object);
                _contextMock.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(0);

                // Act
                await _repo.Delete(measurementId);

                // Assert
                _loggerMock.Verify(l => l.LogSuccess(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
                //_loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Measurement>()), Times.Once);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);

            }
        }
        public class DeleteByListOfIds : MeasurementRepoTests
        {
            [Fact]
            public async Task DeleteById_RemovesMeasurement_WhenFound()
            {
                // Arrange
                var measurementId = 1;
                var measurement = new Measurement { Id = measurementId };
                var dbSetMock = new Mock<DbSet<Measurement>>();

                dbSetMock.Setup(x => x.FindAsync(It.IsAny<object[]>()))
                         .ReturnsAsync((object[] ids) => ids.Contains(measurementId) ? measurement : null);
                dbSetMock.Setup(x => x.Remove(measurement)).Verifiable();

                _contextMock.Setup(x => x.Measurements).Returns(dbSetMock.Object);
                _contextMock.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

                // Act
                await _repo.Delete(measurementId);

                // Assert
                dbSetMock.Verify(x => x.Remove(measurement), Times.Once);
                _contextMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
            }

            // Neuer Test: DeleteById löscht nichts, wenn Messung nicht gefunden wird
            [Fact]
            public async Task DeleteById_DoesNotRemove_WhenNotFound()
            {
                // Arrange
                var measurementId = 999;
                var dbSetMock = new Mock<DbSet<Measurement>>();
                dbSetMock.Setup(x => x.FindAsync(It.IsAny<object[]>())).ReturnsAsync((Measurement)null);

                _contextMock.Setup(x => x.Measurements).Returns(dbSetMock.Object);
                _contextMock.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(0);

                // Act
                await _repo.Delete(measurementId);

                // Assert
                dbSetMock.Verify(x => x.Remove(It.IsAny<Measurement>()), Times.Never);
                _contextMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Never); // Hier von 'Once' auf 'Never' ändern
            }

            // Neuer Test: DeleteById fängt Ausnahme ab und loggt
            [Fact]
            public async Task DeleteById_LogsError_OnException()
            {
                // Arrange
                var measurementId = 1;
                var measurement = new Measurement { Id = measurementId };
                var dbSetMock = new Mock<DbSet<Measurement>>();
                dbSetMock.Setup(x => x.FindAsync(It.IsAny<object[]>())).ReturnsAsync(measurement);
                dbSetMock.Setup(x => x.Remove(measurement)).Throws(new Exception("Remove error"));

                _contextMock.Setup(x => x.Measurements).Returns(dbSetMock.Object);
                _contextMock.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(0);

                // Act
                await _repo.Delete(measurementId);

                // Assert
                _loggerMock.Verify(l => l.LogSuccess(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
                //_loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Measurement>()), Times.Once);
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);

            }
        }
        #endregion
    }
}