from DataAcces.Models.Measurement import Measurement
from datetime import datetime
from typing import cast
import pandas as pd


class MeasurementRepo:

    def __init__(self, session_factory):
        self.session_factory = session_factory

    def get_all(self) -> pd.DataFrame:
        with self.session_factory() as session:
            query = session.query(Measurement)
            df = pd.read_sql(query.statement, session.bind)
            df = cast(pd.DataFrame, df)

            if df.empty:
                return pd.DataFrame()

            df["RecordedAt"] = self.convert_time_to_local(df["RecordedAt"])

            return df

    def get_by_id(self, measurement_id: int) -> pd.DataFrame:
        with self.session_factory() as session:
            query = session.query(Measurement).filter(Measurement.id == measurement_id)
            df = pd.read_sql(query.statement, session.bind)
            df = cast(pd.DataFrame, df)

            if df.empty:
                return pd.DataFrame()

            df["RecordedAt"] = self.convert_time_to_local(df["RecordedAt"])

            return df

    def get_by_sensor_id(self, sensor_id: int) -> pd.DataFrame:
        with self.session_factory() as session:
            query = session.query(Measurement).filter(Measurement.sensor_id == sensor_id)
            df = pd.read_sql(query.statement, session.bind)
            df = cast(pd.DataFrame, df)

            if df.empty:
                return pd.DataFrame()

            df["RecordedAt"] = self.convert_time_to_local(df["RecordedAt"])

            return df

    def get_by_sensor_id_since(self, sensor_id: int, since: datetime) -> pd.DataFrame:
        with self.session_factory() as session:
            try:
                query = session.query(Measurement).filter(Measurement.sensor_id == sensor_id, Measurement.recorded_at > since)

                df = pd.read_sql(query.statement, session.bind)
                df = cast(pd.DataFrame, df)
                df["RecordedAt"] = self.convert_time_to_local(df["RecordedAt"])
                df.sort_values(by="RecordedAt", ascending=False, inplace=True)
                df.drop_duplicates(subset=['RecordedAt'])
                df.reset_index(drop=True, inplace=True)

                return df

            except Exception as e:
                print(e)
                return pd.DataFrame()


    @staticmethod
    def convert_time_to_local(series: pd.Series):
        series = pd.to_datetime(series, utc= True)

        if series.dt.tz is None:
            series = series.dt.tz_localize('UTC')
        else:
            series = series.dt.tz_convert('UTC')

        local_tz = datetime.now().astimezone().tzinfo
        series = series.dt.tz_convert(local_tz)

        series = series.dt.tz_localize(None)

        return series

