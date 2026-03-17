import datetime
from datetime import datetime, timezone, timedelta

from typing import List, Optional
from PySide6.QtCore import QObject, Signal
from pandas import DataFrame
from datetime import datetime

from DataAcces.DTOs.MeasurementDTO import MeasurementDTO
from DataAcces.Repositories.MeasurementRepo import MeasurementRepo
from DataAcces.Repositories.StationRepo import StationRepo


class MeasurementsWidgetViewModel(QObject):

    valuesChanged = Signal(DataFrame)

    def __init__(self, measurement_repo: MeasurementRepo, sensor_id: int):#, station_repo: StationRepo):
        super().__init__()
        self._measurement_repo = measurement_repo
        self._sensor_id = sensor_id
        self._unit = ""
        self._type = ""
        self._last_update_time: Optional[datetime] = None
        self._last_update_time: Optional[datetime] = None
        # self._station_repo = station_repo
        self.radiobutton_times = [("1 Stunde", 1),
                                   ("12 Stunden", 12),
                                   ("1 Tag", 24),
                                   ("30 Tage", 24*30),
                                   ("1 Jahr", 24*365)]
        # self._stations_list = []
        self.selected_radiobutton_index = 0
        self.selected_timespan = self.radiobutton_times[0][1]
        self._list_of_measurementDTOs = []

        # self._list_of_temperatureDTOs = None
        # self._list_of_humidityDTOs = None
        # self._list_of_soil_moistureDTOs = None

        # self.loadStations()
        # self.loadDTOs()
        self. loadMeasurementDTOsSince()
        print()

    def getListOfMeasurementDTOs(self):
        return self._list_of_measurementDTOs

    def setListOfMeasurementDTOs(self, values: List[List[MeasurementDTO]]):
        if values and values != self._list_of_measurementDTOs:
            self._list_of_measurementDTOs = values

    def getTimesAndValuesAsDataFrameSince(self, since: datetime):
        if since.tzinfo is None:
            since = since.replace(tzinfo=timezone.utc)

        test = since
        # since = since-timedelta(hours=3)
        current_measurement_dtos = [dto for dto in self._list_of_measurementDTOs if dto.recorded_at > since]
        if not current_measurement_dtos:
            return DataFrame({"Date":[], "Value": []})

        return DataFrame({"Date": [dto.recorded_at for dto in current_measurement_dtos],
                          self._list_of_measurementDTOs[0].type: [dto.value for dto in current_measurement_dtos]
                          })

    def getTimeAndValuesAsDataFrame(self):
        if not self._list_of_measurementDTOs:
            return DataFrame({"Date":[], "Value": []})

        return DataFrame({
            "Date": [date.recorded_at for date in self._list_of_measurementDTOs],
            self._list_of_measurementDTOs[0].type: [temp.value for temp in self._list_of_measurementDTOs]
        })

    def loadMeasurementDTOsSince(self):
        self._list_of_measurementDTOs = self._measurement_repo.get_all_by_sensor_id_since(self._sensor_id,datetime.now(timezone.utc)-timedelta(hours=1))
        if self._list_of_measurementDTOs:
            self._unit = self._list_of_measurementDTOs[0].unit
            self._type = self._list_of_measurementDTOs[0].type
        self._last_update_time = datetime.now(timezone.utc)

    def updateMeasurementDTOs(self):
        test = datetime.now(timezone.utc)
        self._list_of_measurementDTOs = self._measurement_repo.get_all_by_sensor_id_since(self._sensor_id,
                                                                                 datetime.now(timezone.utc)-timedelta(hours=self.selected_timespan))

        if self._list_of_measurementDTOs:
            self._unit = self._list_of_measurementDTOs[0].unit
            self._type = self._list_of_measurementDTOs[0].type

        self._last_update_time = datetime.now(timezone.utc)

    # def getTemperatureValues(self):
    #     return self._list_of_temperatureDTOs
    #
    # def getHumidityValues(self):
    #     return self._list_of_humidityDTOs
    #
    # def getSoilMoistureValues(self):
    #     return self._list_of_soil_moistureDTOs

    # def getAllValuesAsDataFrame(self):
    #     min_list_length = len(self._list_of_temperatureDTOs)
    #     if min_list_length > len(self._list_of_humidityDTOs):
    #         min_list_length = len(self._list_of_humidityDTOs)
    #     if min_list_length > len(self._list_of_soil_moistureDTOs):
    #         min_list_length = len(self._list_of_soil_moistureDTOs)
    #
    #     temp = self._list_of_temperatureDTOs[-min_list_length:]
    #     hum = self._list_of_humidityDTOs[-min_list_length:]
    #     soil = self._list_of_soil_moistureDTOs[-min_list_length:]
    #
    #     return DataFrame({
    #                 "Datum": [date.recorded_at for date in temp],
    #                 "Temperature": [temp.value for temp in temp],
    #                 "Humidity": [hum.value for hum in hum],
    #                 "SoilMoisture":[soil.value for soil in soil]
    #     })


    # def setDTOs(self, values: List[List[MeasurementDTO]]):
    #     """
    #     Sets the temperature, humidity and soil moisture measurements.
    #     :param values: A list of MeasurementDTO-lists (temperatureDTOs, humidityDTOs, soil moistureDTOs)
    #     """
    #     for index, DTOs in values:
    #         if DTOs is not None:
    #             if index == 0 and DTOs != self._list_of_temperatureDTOs:
    #                 self._list_of_temperatureDTOs = DTOs
    #             if index == 1 and DTOs != self._list_of_humidityDTOs:
    #                 self._list_of_humidityDTOs = DTOs
    #             if index == 2 and DTOs != self._list_of_humidityDTOs:
    #                 self._list_of_soil_moistureDTOs = DTOs
    #
    #     self.valuesChanged.emit(self._list_of_temperatureDTOs)

    # def loadDTOs(self):
    #     self._list_of_temperatureDTOs = self._measurement_repo.get_by_sensor_id(33)
    #     self._list_of_humidityDTOs = self._measurement_repo.get_by_sensor_id(32)
    #     self._list_of_soil_moistureDTOs = self._measurement_repo.get_by_sensor_id(88)
    #     self.valuesChanged.emit(self._list_of_temperatureDTOs)

    # def loadStations(self):
    #     self._stations_list = [station.id for station in self._station_repo.get_all()]