from abc import abstractmethod
from datetime import timezone, timedelta
from typing import Optional
from PySide6.QtCore import QObject, Signal
from pandas import DataFrame
from datetime import datetime

from Context.StationDataContext import StationDataContext
from DataAcces.Repositories.MeasurementRepo import MeasurementRepo
from HelperServices.MeasurementValidationService import MeasurementValidationService
from HelperServices.MeasurementValidationService import ValidationStatus


class MeasurementViewModelBase(QObject):

    valuesChanged = Signal(DataFrame)

    def __init__(self, station_data_context: StationDataContext, measurement_validation_service: MeasurementValidationService, station_id: int):
        super().__init__()

        self._station_data_context = station_data_context
        self._validation_service = measurement_validation_service

        self._station_id = station_id

        self._last_update_time: Optional[datetime] = None

        self.radiobutton_times = [("1 Stunde", 1),
                                   ("12 Stunden", 12),
                                   ("1 Tag", 24),
                                   ("30 Tage", 24*30),
                                   ("1 Jahr", 24*365)]
        self.selected_radiobutton_index = 1

        self.since = datetime.now(timezone.utc) - timedelta(hours=self.radiobutton_times[self.selected_radiobutton_index][1])

        self.validation_status = ValidationStatus.EMPTY

        self. initial_load_measurements()



    @property
    def station_id(self) -> int:
        return self._station_id

    @property
    def last_update_time(self) -> datetime:
        if self._last_update_time is not None:
            return self._last_update_time
        else:
            return datetime(1,1,1)

    # The initial loading process of the measurement data as DataFrame.
    def initial_load_measurements(self):
        pass

    def update_selected_timespan(self):
        self.since =self.radiobutton_times[self.selected_radiobutton_index][1]

    @abstractmethod
    def update_measurements(self):
        pass

    @abstractmethod
    def set_measurement_infos(self):
        pass