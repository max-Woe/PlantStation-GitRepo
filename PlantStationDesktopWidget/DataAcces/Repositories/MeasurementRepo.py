import pandas as pd
import sqlalchemy
import logging
from DataAcces.Models.Measurement import Measurement, Sensor
from sqlalchemy import select, exc
from datetime import datetime
from typing import cast

logger = logging.getLogger(__name__)

class MeasurementRepo:

    def __init__(self, session_factory):
        self.session_factory = session_factory

    def get_all(self) -> pd.DataFrame:
        with self.session_factory() as session:
            try:
                query = session.query(Measurement)
                df = pd.read_sql(query.statement, session.bind)
                df = cast(pd.DataFrame, df)

                if not df.empty:
                    df["RecordedAt"] = self.convert_time_to_local(df["RecordedAt"])

                return df

            except sqlalchemy.exc.ProgrammingError as ex:
                # Schema-Mismatch, Tabelle existiert nicht
                logger.error(f"DB schema error: {ex}")
                return pd.DataFrame()

            except sqlalchemy.exc.InvalidRequestError as ex:
                # Session-Problem
                logger.error(f"DB session error: {ex}")
                return pd.DataFrame()

    def get_by_id(self, measurement_id: int) -> pd.DataFrame:
        with self.session_factory() as session:
            try:
                query = session.query(Measurement).filter(Measurement.id == measurement_id)
                df = pd.read_sql(query.statement, session.bind)
                df = cast(pd.DataFrame, df)

                if df.empty:
                    return pd.DataFrame()

                df["RecordedAt"] = self.convert_time_to_local(df["RecordedAt"])

                return df

            except sqlalchemy.exc.ProgrammingError as ex:
                # Schema-Mismatch, Tabelle existiert nicht
                logger.error(f"DB schema error: {ex}")
                return pd.DataFrame()

            except sqlalchemy.exc.InvalidRequestError as ex:
                # Session-Problem
                logger.error(f"DB session error: {ex}")
                return pd.DataFrame()

    def get_by_sensor_id(self, sensor_id: int) -> pd.DataFrame:
        with self.session_factory() as session:
            try:
                query = session.query(Measurement).filter(Measurement.sensor_id == sensor_id)
                df = pd.read_sql(query.statement, session.bind)
                df = cast(pd.DataFrame, df)

                if not df.empty:
                    df["RecordedAt"] = self.convert_time_to_local(df["RecordedAt"])

                return df

            except sqlalchemy.exc.ProgrammingError as ex:
                # Schema-Mismatch, Tabelle existiert nicht
                logger.error(f"DB schema error: {ex}")
                return pd.DataFrame()

            except sqlalchemy.exc.InvalidRequestError as ex:
                # Session-Problem
                logger.error(f"DB session error: {ex}")
                return pd.DataFrame()

    def get_by_sensor_id_since(self, sensor_id: int, since: datetime) -> pd.DataFrame:
        with self.session_factory() as session:
            try:
                query = session.query(Measurement).filter(Measurement.sensor_id == sensor_id, Measurement.recorded_at > since)

                df = pd.read_sql(query.statement, session.bind)
                df = cast(pd.DataFrame, df)
                if not df.empty:
                    df["RecordedAt"] = self.convert_time_to_local(df["RecordedAt"])
                    df.sort_values(by="RecordedAt", ascending=False, inplace=True)
                    df.drop_duplicates(subset=['RecordedAt'])
                    df.reset_index(drop=True, inplace=True)

                return df

            except sqlalchemy.exc.ProgrammingError as ex:
                # Schema-Mismatch, Tabelle existiert nicht
                logger.error(f"DB schema error: {ex}")
                return pd.DataFrame()

            except sqlalchemy.exc.InvalidRequestError as ex:
                # Session-Problem
                logger.error(f"DB session error: {ex}")
                return pd.DataFrame()


    def get_current_values_by_station_id(self, station_id: int) -> pd.DataFrame:
        with self.session_factory() as session:
            try:
                query = (
                    session.query(Measurement)
                    .join(Sensor, Measurement.sensor_id == Sensor.id)
                    .filter(Sensor.station_id == station_id)
                    .distinct(Measurement.sensor_id)
                    .order_by(Measurement.sensor_id, Measurement.recorded_at.desc())
                )

                df = pd.read_sql(query.statement, session.bind)
                df = cast(pd.DataFrame, df)
                if not df.empty:
                    df["RecordedAt"] = self.convert_time_to_local(df["RecordedAt"])
                    df.sort_values(by="RecordedAt", ascending=False, inplace=True)
                    df.drop_duplicates(subset=['RecordedAt'])
                    df.reset_index(drop=True, inplace=True)

                return df

            except sqlalchemy.exc.ProgrammingError as ex:
                # Schema-Mismatch, Tabelle existiert nicht
                logger.error(f"DB schema error: {ex}")
                return pd.DataFrame()

            except sqlalchemy.exc.InvalidRequestError as ex:
                # Session-Problem
                logger.error(f"DB session error: {ex}")
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

    def get_vpd_measurements_by_stationId_since(self, station_id:int, since:datetime)->pd.DataFrame:
        with (self.session_factory() as session):
            try:
                statement = (
                    select(Measurement.recorded_at, Measurement.value, Measurement.type)
                    .join(Sensor, Sensor.id == Measurement.sensor_id)
                    .where(
                        Sensor.station_id == station_id,
                        Measurement.type.in_(["temperature", "humidity"]),
                        Measurement.recorded_at > since
                    )
                )
                result = session.execute(statement)

                df = pd.DataFrame(result, columns=["RecordedAt", "Value", "Type"])
                df = df.pivot(index="RecordedAt", columns="Type", values="Value").reset_index()

                # unit, measurement_type = result.first()

                return df

            except sqlalchemy.exc.ProgrammingError as ex:
                # Schema-Mismatch, Tabelle existiert nicht
                logger.error(f"DB schema error: {ex}")
                return pd.DataFrame()

            except sqlalchemy.exc.InvalidRequestError as ex:
                # Session-Problem
                logger.error(f"DB session error: {ex}")
                return pd.DataFrame()

    def get_measurement_data_by_station_id_since(self, station_id: int, since: datetime)->pd.DataFrame:
        df = pd.DataFrame()
        with self.session_factory() as session:
            try:
                statement = (
                    select(Measurement)
                    .join(Sensor, Sensor.id == Measurement.sensor_id)
                    .where(Sensor.station_id == station_id, Measurement.recorded_at >= since)
                )


                df = pd.read_sql(statement, session.bind)


            except sqlalchemy.exc.SQLAlchemyError as ex:
                # Schema-Mismatch, Tabelle existiert nicht
                logger.error(f"DB schema error: {ex}")

        return df