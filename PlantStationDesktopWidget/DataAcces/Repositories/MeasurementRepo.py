from pandas.core.interchange.dataframe_protocol import DataFrame

from DataAcces.Models.Measurement import Measurement, Base
from DataAcces.DTOs.MeasurementDTO import MeasurementDTO
from datetime import datetime
import pandas as pd
import numpy as np


class MeasurementRepo:

    def __init__(self, session_factory):
        self.session_factory = session_factory

    def get_all(self) -> pd.DataFrame:
        with self.session_factory() as session:
            query = session.query(Measurement)
            df = pd.read_sql(query.statement, session.bind)

            if df.empty:
                return df

            df["RecordedAt"] = self.convert_time_to_local(df["RecordedAt"])

            return df[df.apply(self.is_measurement_valid, axis=1)].reset_index(drop=True)

    def get_by_id(self, measurement_id: int) -> pd.DataFrame:
        with self.session_factory() as session:
            query = session.query(Measurement).filter(Measurement.id == measurement_id)
            df = pd.read_sql(query.statement, session.bind)

            if df.empty:
                return df

            df["RecordedAt"] = self.convert_time_to_local(df["RecordedAt"])

            return df[df.apply(self.is_measurement_valid, axis=1)].reset_index(drop=True)

    def get_by_sensor_id(self, sensor_id: int) -> pd.DataFrame:
        with self.session_factory() as session:
            query = session.query(Measurement).filter(Measurement.sensor_id == sensor_id)
            df = pd.read_sql(query.statement, session.bind)

            if df.empty:
                return df

            df["RecordedAt"] = self.convert_time_to_local(df["RecordedAt"])

            return df[df.apply(self.is_measurement_valid, axis=1)].reset_index(drop=True)

    def get_by_sensor_id_since(self, sensor_id: int, since: datetime) -> pd.DataFrame:
        with self.session_factory() as session:
            try:
                query = session.query(Measurement).filter(Measurement.sensor_id == sensor_id, Measurement.recorded_at > since)

                df = pd.read_sql(query.statement, session.bind)


                df["RecordedAt"] = self.convert_time_to_local(df["RecordedAt"])

                df = df[df.apply(self.is_measurement_valid, axis=1)].reset_index(drop=True)

                if df.empty:
                    new_row = {}
                    for col in df.columns:
                        if pd.api.types.is_numeric_dtype(df[col]):
                            new_row[col] = np.nan
                        elif pd.api.types.is_datetime64_any_dtype(df[col]):
                            new_row[col] = pd.NaT
                        else:
                            new_row[col] = 'None'

                    df.loc[0] = new_row

                return df

            except Exception as e:
                print(e)
                return pd.DataFrame()


    @staticmethod
    def convert_time_to_local(series: pd.Series):
        series = pd.to_datetime(series)

        if series.dt.tz is None:
            series = series.dt.tz_localize('UTC')
        else:
            series = series.dt.tz_convert('UTC')

        local_tz = datetime.now().astimezone().tzinfo
        series = series.dt.tz_convert(local_tz)

        series = series.dt.tz_localize(None)

        return series

    def is_measurement_valid(self, row) -> bool:
        value = row['Value']
        measurement_type = row['Type']

        if not ((isinstance(value, float) or isinstance(value, int)) and isinstance(measurement_type, str)):
            return False

        if measurement_type == 'temperature' and value<=60 and value >= -40:
            return True
        elif measurement_type == 'humidity' and value<=100 and value >= 0:
            return True
        elif measurement_type == 'soil_moisture' and value<=100 and value >= 0:
            return True
        else:
            return False