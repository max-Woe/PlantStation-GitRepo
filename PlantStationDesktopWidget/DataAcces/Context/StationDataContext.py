from curses import start_color

import pandas as pd

from datetime import datetime, timedelta, timezone
from HelperServices.MeasurementValidationService import MeasurementValidationService

from DataAcces.Repositories.MeasurementRepo import MeasurementRepo
from HelperServices.MeasurementValidationService import MeasurementValidationService
from HelperServices.MeasurementsSeperationService import seperate_measurements_df, get_measurement_types

class StationDataContext:
    def __init__(self, station_id: int, measurements_repo: MeasurementRepo,
                 since = (datetime.now()- timedelta(hours=1)), until: datetime | None = None):
        self._station_id =station_id
        self._measurements_repo = measurements_repo
        self._since = since
        self._until = until

        self._all_measurements_dict = {}
        self._validation_service = MeasurementValidationService()

        self._initial_load()

    @property
    def all_measurements_dict(self):
        return {key: df.copy() for key, df in self._all_measurements_dict.items()}

    @property
    def available_types(self):
        return list(self._all_measurements_dict.keys())

    @property
    def since(self):
        return self._since
    @since.setter
    def since(self, value):
        if value != self._since:
            self._since = value

    @property
    def until(self):
        return self._until
    @until.setter
    def until(self, value):
        if value != self._until:
            self._until = value


    def _initial_load(self):
        """The initial load of the StationDataContext. Calls the measurements as Pandas DataFrame and a Dict."""
        self._fetch_and_prepare_data()

    def update_all_measurement_data(self, since: datetime, until: datetime |None = None):
        """Updates the internal since, all_measurements_df and all_measurements_dict."""
        self._since = since
        self._until = until
        self._fetch_and_prepare_data()

    def _fetch_and_prepare_data(self):
        """Fetches, validates and prepares measurement data for current station.
        Loads measurements starting from the configured timestamp, converts timestamps to local timezone, and updates
        both the internal all_measurements_df and all_measurements_dict grouped by types."""
        start_time = self._since
        if self._until is not None:
            end_time = self._until.astimezone()
        else:
            end_time = datetime.now()
        all_measurements_df = self._measurements_repo.get_by_sensor_id_and_time_range(self._station_id, start_time, end_time)
        # all_measurements_df = self._measurements_repo.get_measurement_data_by_station_id_since(self._station_id, self._since)

        all_measurements_df = self._validation_service.clear_by_limits(df_original=all_measurements_df)

        all_measurements_df['RecordedAt'] = self.convert_to_local_datetime(all_measurements_df['RecordedAt'])

        dict_of_type_measurements_pairs = {}

        if not all_measurements_df.empty:
            dict_of_type_measurements_pairs = seperate_measurements_df(all_measurements_df)

        self._all_measurements_dict = dict_of_type_measurements_pairs

    def get_measurements_by_type(self, measurement_type: str)->pd.DataFrame:
        """Returns a copy of the measurements dataframe with the given mesurement type.

        :param measurement_type: The type of measurements to return (e.g. temperature, humidity or soil moisture).
        :return: A copy of the DataFrame containing the measurements with the given measurement type, or an empty
        DataFrame if no measurements with the given type were found."""
        return self._all_measurements_dict.get(measurement_type, pd.DataFrame()).copy()

    def get_measurements_df_by_type_for_vpd(self)->tuple[pd.DataFrame, pd.DataFrame]:
        """Returns a copy of the measurements dataframe required for the VPD calculation (temperature and humidity).
        :return A Tuple of the copied measurement dataframes, or one/both empty dataframe(s) if no measurements with the
        type temperature and/or humidity were found."""
        temperature_df = pd.DataFrame()
        humidity_df = pd.DataFrame()
        if (len(self._all_measurements_dict)>0 and
                ("temperature" in self._all_measurements_dict.keys() or "humidity" in self._all_measurements_dict.keys())):
            temperature_df = self._all_measurements_dict['temperature'].copy()
            humidity_df = self._all_measurements_dict['humidity'].copy()

        return temperature_df, humidity_df

    @staticmethod
    def convert_to_local_datetime(time_series: pd.Series)->pd.Series:
        """Converts the given time series into a local datetime series, depending on the operatingsystem.

        :param time_series: The time series dataframe to be converted into a local datetime series.
        :return: Pandas Series containing all converted timestamps."""
        local_timezone_info = datetime.now().astimezone().tzinfo
        local_datetime_series = pd.Series()
        if len(time_series)>0:
            local_datetime_series = time_series.dt.tz_convert(local_timezone_info)
        return local_datetime_series