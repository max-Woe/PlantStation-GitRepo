from HelperServices import GrowAnalyticsService
from ViewModels.MeasurementViewModelBase import MeasurementViewModelBase
from HelperServices.MeasurementValidationService import MeasurementValidationService, ValidationStatus
from datetime import datetime, timezone
from DataAcces.Context.StationDataContext import StationDataContext
import pandas as pd

class VpdMeasurementViewModel(MeasurementViewModelBase):
    def __init__(self,station_data_context: StationDataContext,
                 measurement_validation_service: MeasurementValidationService,
                 station_id: int):
        self._station_data_context = station_data_context
        self._unit = "kPa"
        self._type = "vpd"

        self.temperature_df = None
        self.humidity_df = None
        self.vpd_df = None
        super().__init__(station_data_context, measurement_validation_service, station_id)

        self.validation_status = self.validation_status
        self.validation_status_temp = None
        self.validation_status_hum = None

    def initial_load_measurements(self):
        # Initial loading process of the measurements_df
        self.update_measurements()

        # if the initial loading process of the measurement data has been successful...
        if self.temperature_df is not None and self.humidity_df is not None:
            # set the unit and type of the measurements to complete the infos for the InfoWidget labels.
            self.set_measurement_infos()

    # SINGLE /VPD
    def update_measurements(self):
        since = self.since
        until = self.until
        self._station_data_context.update_all_measurement_data(since, until)

        updated_temperature_df, updated_humidity_df = self._station_data_context.get_measurements_df_by_type_for_vpd()
        self.validation_status_temp=self._validation_service.validate_dataframe(updated_temperature_df)
        self.validation_status_hum=self._validation_service.validate_dataframe(updated_humidity_df)
        if (self.validation_status_temp == ValidationStatus.ALL_VALID and
                self.validation_status_hum == ValidationStatus.ALL_VALID):
            self.validation_status = ValidationStatus.ALL_VALID

        if self.validation_status == ValidationStatus.ALL_VALID or self.validation_status == ValidationStatus.PARTIAL_VALID:
            updated_temperature_df = self._validation_service.clear_by_limits(updated_temperature_df).drop_duplicates(subset='RecordedAt')
            updated_humidity_df = self._validation_service.clear_by_limits(updated_humidity_df).drop_duplicates(subset='RecordedAt')
            updated_temperature_df = updated_temperature_df[updated_temperature_df['RecordedAt'].isin(updated_humidity_df['RecordedAt'])].reset_index(drop=True)
            updated_humidity_df = updated_humidity_df[updated_humidity_df['RecordedAt'].isin(updated_temperature_df['RecordedAt'])].reset_index(drop=True)
            self.temperature_df = updated_temperature_df.copy()
            self.humidity_df = updated_humidity_df.copy()
            self.vpd_df = GrowAnalyticsService.calculate_vapor_pressure_difference(updated_temperature_df,updated_humidity_df)

        if self.temperature_df is not None and self.humidity_df is not None:
            self._last_update_time = datetime.now(timezone.utc)


    # SINGLE /VPD
    def set_measurement_infos(self):
        if (self.temperature_df is not None and not self.temperature_df.empty and
                self.humidity_df is not None and not self.humidity_df.empty):
            self._unit = "kPa"
            self._type = "VPD"
        print()