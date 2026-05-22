from PySide6.QtCore import QObject, Signal

from DataAcces.Repositories.SensorRepo import SensorRepo
from DataAcces.Repositories.StationRepo import StationRepo


class MainWindowViewModel(QObject):
    statusChanged = Signal(str)

    def __init__(self, station_repo : StationRepo, sensor_repo: SensorRepo):
        super().__init__()

        self._station_repo = station_repo
        self._sensor_repo = sensor_repo

        self._list_of_stations = None
        self._station_sensor_pairs = []
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
        self._list_of_stations.sort()

    def load_stations_and_sensors_as_list_of_tuples(self):

        stations = self._station_repo.get_all()

        for index, station in stations.iterrows():
            sensors= self._sensor_repo.get_sensors_by_station_id(station['Id'])
            self._station_sensor_pairs.append((station, sensors))

        self._station_sensor_pairs.sort(key = lambda pair: pair[0]['Id'])
