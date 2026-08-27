import pandas as pd

from ViewModels.MeasurementViewModelBase import MeasurementViewModelBase
from DataAcces.Repositories.MeasurementRepo import MeasurementRepo
from HelperServices.MeasurementValidationService import MeasurementValidationService, ValidationStatus
from datetime import datetime, timezone
from typing import Optional
from Context.StationDataContext import StationDataContext

class SingleMeasurementViewModel(MeasurementViewModelBase):

    def __init__(self,
                 station_data_context: StationDataContext,
                 measurement_validation_service: MeasurementValidationService,
                 station_id: int,
                 sensor_id: int,
                 measurement_type: str):
        self._sensor_id = sensor_id
        self.measurement_type = measurement_type

        self.measurement_df = None
        super().__init__(station_data_context, measurement_validation_service, station_id)

        self._unit = ""

        self.validation_status = self.validation_status
        self._last_update_time: Optional[datetime] = None


    # SINGLE
    @property
    def sensor_id(self) -> int:
        return self._sensor_id


    # SINGLE
    @property
    def unit(self) -> str:
        return self._unit


    # SINGLE
    @property
    def type(self) -> str:
        return self.measurement_type


    def initial_load_measurements(self):
        # Initial loading process of the measurements_df
        self.update_measurements()

        # if the initial loading process of the measurement data has been successful...
        if self.measurement_df is not None:
            # set the unit and type of the measurements to complete the infos for the InfoWidget labels.
            self.set_measurement_infos()

    def update_measurements(self):
        since = self.since
        self._station_data_context.update_all_measurement_data(since)

        updated_df = self._station_data_context.get_measurements_by_type(self.measurement_type)
        self.validation_status = self._validation_service.validate_dataframe(updated_df)

        if self.validation_status == ValidationStatus.ALL_VALID or self.validation_status == ValidationStatus.PARTIAL_VALID:
            self.measurement_df = self._validation_service.clear_by_limits(updated_df)
            if self.measurement_df is not None:
                self._last_update_time = datetime.now(timezone.utc)


    def set_measurement_infos(self):
        if self.measurement_df is not None and not self.measurement_df.empty:
            self._unit = self.measurement_df["Unit"][0]
            self.measurement_type = self.measurement_df["Type"][0]
        print()