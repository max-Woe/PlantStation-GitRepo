import datetime
from datetime import datetime, timezone, timedelta

from typing import List, Optional
from PySide6.QtCore import QObject, Signal
from pandas import DataFrame
from datetime import datetime
from DataAcces.Repositories.MeasurementRepo import MeasurementRepo


class MeasurementsWidgetViewModel(QObject):

    valuesChanged = Signal(DataFrame)

    def __init__(self, measurement_repo: MeasurementRepo, station_id: int, sensor_id: int):#, station_repo: StationRepo):
        super().__init__()

        self._measurement_repo = measurement_repo

        self._station_id = station_id
        self._sensor_id = sensor_id
        self._unit = ""
        self._type = ""
        self._last_update_time: Optional[datetime] = None

        self.radiobutton_times = [("1 Stunde", 1),
                                   ("12 Stunden", 12),
                                   ("1 Tag", 24),
                                   ("30 Tage", 24*30),
                                   ("1 Jahr", 24*365)]
        self.selected_radiobutton_index = 1
        self.selected_timespan = self.radiobutton_times[self.selected_radiobutton_index][1]

        self.measurement_df = None

        self. initial_load_measurements()

    @property
    def station_id(self) -> int:
        return self._station_id

    @property
    def sensor_id(self) -> int:
        return self._sensor_id

    @property
    def unit(self) -> str:
        return self._unit

    @property
    def type(self) -> str:
        return self._type

    @property
    def last_update_time(self) -> datetime:
        return self._last_update_time

    def update_selected_timespan(self):
        self.selected_timespan =self.radiobutton_times[self.selected_radiobutton_index][1]

    # The initial loading process of the measurement data as DataFrame.
    def initial_load_measurements(self):
        # Initial loading process of the measurements_df
        self.update_measurements()

        # if the initial loading process of the measurement data has been successful...
        if self.measurement_df is not None:
            # set the unit and type of the measurements to complete the infos for the InfoWidget labels.
            self.set_measurement_infos()

    def update_measurements(self):
        since = datetime.now(timezone.utc)-timedelta(hours=self.selected_timespan)
        self.measurement_df = self._measurement_repo.get_by_sensor_id_since(self.sensor_id, since)

        if self.measurement_df is not None:
            self._last_update_time = datetime.now(timezone.utc)

    def set_measurement_infos(self):
        if self.measurement_df is not None and not self.measurement_df.empty:
            self._unit = self.measurement_df["Unit"][0]
            self._type = self.measurement_df["Type"][0]
        print()