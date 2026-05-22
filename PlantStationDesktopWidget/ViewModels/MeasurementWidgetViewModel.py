from datetime import timezone, timedelta
from typing import Optional
from PySide6.QtCore import QObject, Signal
from pandas import DataFrame
from datetime import datetime
from DataAcces.Repositories.MeasurementRepo import MeasurementRepo
from HelperServices import MeasurementValidationService
from HelperServices.MeasurementValidationService import ValidationStatus


class MeasurementsWidgetViewModel(QObject):

    valuesChanged = Signal(DataFrame)

    def __init__(self, measurement_repo: MeasurementRepo, station_id: int, sensor_id: int):#, station_repo: StationRepo):
        super().__init__()

        self._measurement_repo = measurement_repo

        self.measurements_df_is_valid_flag = False      #True: at least two value are valid; False: all values are invalid.
        self.measurements_df_contains_invalid_values_flag = False #True: any value is invalid; False: all values are valid
        self.measurement_df_is_empty_flag = True        #True: the dataframe is empty; False: the dataframe got entries.

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

        self.selected_timespan = datetime.now(timezone.utc)-timedelta(hours=self.radiobutton_times[self.selected_radiobutton_index][1])

        self.measurement_df = None
        self.validation_status = ValidationStatus.EMPTY

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
        if self._last_update_time is not None:
            return self._last_update_time
        else:
            return datetime(1,1,1)

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
        since = self.selected_timespan
        df = self._measurement_repo.get_by_sensor_id_since(self.sensor_id, since)
        self.validation_status = MeasurementValidationService.validate_dataframe(df)

        if self.validation_status == ValidationStatus.ALL_VALID or self.validation_status == ValidationStatus.PARTIAL_VALID:
            self.measurement_df = MeasurementValidationService.clear(df)
            if self.measurement_df is not None:
                self._last_update_time = datetime.now(timezone.utc)


    def set_measurement_infos(self):
        if self.measurement_df is not None and not self.measurement_df.empty:
            self._unit = self.measurement_df["Unit"][0]
            self._type = self.measurement_df["Type"][0]
        print()