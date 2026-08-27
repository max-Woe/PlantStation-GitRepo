import pandas as pd

from datetime import datetime, timedelta

from DataAcces.Repositories.MeasurementRepo import MeasurementRepo
from HelperServices.MeasurementsSeperationService import seperate_measurements_df, get_measurement_types

class StationDataContext:
    def __init__(self, station_id: int, measurements_repo: MeasurementRepo, since = (datetime.now()- timedelta(hours=1))):
        self._station_id =station_id
        self._measurements_repo = measurements_repo
        self._since = since

        self._all_measurements_dict = {}

        self._initial_load()

    @property
    def all_measurements_dict(self):
        return {key: df.copy() for key, df in self._all_measurements_dict.items()}

    @property
    def available_types(self):
        return list(self._all_measurements_dict.keys())


    def _initial_load(self):
        self._set_measurements_df_and_dict_by_type()

    def _set_measurements_df_and_dict_by_type(self):
        all_measurements_df = self._measurements_repo.get_measurement_data_by_station_id_since(self._station_id, self._since)

        all_measurements_df['RecordedAt'] = self.get_local_datetime(all_measurements_df['RecordedAt'])

        dict_of_type_measurements_pairs = {}

        if not all_measurements_df.empty:
            dict_of_type_measurements_pairs = seperate_measurements_df(all_measurements_df)

        self._all_measurements_dict = dict_of_type_measurements_pairs

    def update_all_measurement_data(self, since):
        self._since = since
        self._set_measurements_df_and_dict_by_type()

    def get_measurements_by_type(self, measurement_type: str)->pd.DataFrame:
        return self._all_measurements_dict.get(measurement_type, pd.DataFrame()).copy()

    def get_measurements_df_by_type_for_vpd(self)->tuple[pd.DataFrame, pd.DataFrame]:
        temperature_df = pd.DataFrame()
        humidity_df = pd.DataFrame()
        if len(self._all_measurements_dict)>0:
            temperature_df = self._all_measurements_dict['temperature'].copy()
            humidity_df = self._all_measurements_dict['humidity'].copy()

        return temperature_df, humidity_df

    def get_local_datetime(self, time_series: pd.Series)->pd.Series:
        local_timezone_info = datetime.now().astimezone().tzinfo
        local_datetime_series = pd.Series()
        if len(time_series)>0:
            local_datetime_series = time_series.dt.tz_convert(local_timezone_info)
        return local_datetime_series