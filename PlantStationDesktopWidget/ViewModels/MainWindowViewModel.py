from PySide6.QtCore import QObject, Signal, Property

from DataAcces.Repositories import SensorRepo
from DataAcces.Repositories.StationRepo import StationRepo


class MainWindowViewModel(QObject):
    statusChanged = Signal(str)

    def __init__(self, station_repo : StationRepo, sensor_repo: SensorRepo):
        super().__init__()

        self._station_repo = station_repo
        self._sensor_repo = sensor_repo

        self._list_of_stations = None
        self._list_of_station_and_sensor_ids_as_tuples = []
        self._status = "Initializing"

        self.load_stations_as_list()
        self.load_stations_and_sensors_as_list_of_tuples()
        print()

    def getStatus(self):
        return self._status

    def setStatus(self, value):
        if value != self._status:
            self._status = value
            self.statusChanged.emit(value)

    def load_stations_as_list(self):
        self._list_of_stations = self._station_repo.get_all_station_ids()

    def load_stations_and_sensors_as_list_of_tuples(self):
        station_ids = self._station_repo.get_all_station_ids()
        for station_id in station_ids:
            sensor_ids = self._sensor_repo.get_sensor_ids_by_station_id(station_id)
            self._list_of_station_and_sensor_ids_as_tuples.append((station_id, sensor_ids))
            # for sensor_id in sensor_ids:
            #     self._list_of_station_and_sensor_ids_as_tuples.append((station_id, sensor_id))

    # status = Property(str, getStatus, setStatus, notify=statusChanged)
