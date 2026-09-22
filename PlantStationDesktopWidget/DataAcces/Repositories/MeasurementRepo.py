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
        """Fetches all measurements.

        :return: Pandas DataFrame containing all measurements."""
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
        """Fetches a specific measurement.

        :param measurement_id: The id of the requested measurement.
        :return: Pandas DataFrame containing the requested measurement."""
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
        """Fetches a specific measurement with the given sensor id.

        :param sensor_id: The ID of the sensor whose measurements should be fetched.
        :return: Pandas DataFrame containing all measurements for the specified sensor."""
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
        """Fetches all measurements with the given sensor id from a given timestamp.

        :param sensor_id: The ID of the sensor whose measurements should be fetched.
        :param since: The timestamp from which the measurements should be fetched.
        :return: Pandas DataFrame containing all measurements for the specified sensor and timestamp."""
        with self.session_factory() as session:
            try:
                query = session.query(Measurement).filter(Measurement.sensor_id == sensor_id, Measurement.recorded_at > since)

                df = pd.read_sql(query.statement, session.bind)
                df = cast(pd.DataFrame, df)
                if not df.empty:
                    df["RecordedAt"] = self.convert_time_to_local(df["RecordedAt"])
                    df.sort_values(by="RecordedAt", ascending=False, inplace=True)
                    df.drop_duplicates(subset=['RecordedAt'], inplace=True)
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

    def get_by_station_id(self, station_id: int) -> pd.DataFrame:
        """Fetches all measurements with the given station id.

        :param station_id: The ID of the station whose measurements should be fetched.
        :return: Pandas DataFrame containing all measurements for the station."""
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

    def get_vpd_measurements_by_stationId_since(self, station_id:int, since:datetime)->pd.DataFrame:
        """Fetches all measurements required for the VPD calculation with the given station id from a given timestamp.

        :param station_id: The ID of the station whose measurements should be fetched.
        :param since: The timestamp from which the measurements should be fetched.
        :return: Pandas DataFrame containing all measurements required for the VPD calculation for the station since the
        specified time."""
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
        """Fetches all measurements with the given station id.

        :param station_id: The ID of the station whose measurements should be fetched.
        :param since: The timestamp from which the measurements should be fetched.
        :return: Pandas DataFrame containing all measurements for the station since the specified time."""
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

    def get_by_sensor_id_and_time_range(self, station_id: int, start_time:datetime, end_time:datetime)->pd.DataFrame:
        df = pd.DataFrame()

        with self.session_factory() as session:
            try:
                statement = (
                    select(Measurement).
                    join(Sensor, Sensor.id == Measurement.sensor_id).
                    where(Sensor.station_id == station_id,
                          Measurement.recorded_at >= start_time,
                          Measurement.recorded_at <= end_time))

                df = pd.read_sql(statement, session.bind)

            except sqlalchemy.exc.SQLAlchemyError as ex:
                logger.error(f"DB schema error: {ex}")

        return df


    @staticmethod
    def convert_time_to_local(series: pd.Series):
        """Converts all timestamps of a Pandas Series to the local timezone of the operating system.

        :param series: Pandas Series containing all timestamps.
        :return: Pandas Series containing all converted timestamps."""
        series = pd.to_datetime(series, utc= True)

        if series.dt.tz is None:
            series = series.dt.tz_localize('UTC')
        else:
            series = series.dt.tz_convert('UTC')

        local_tz = datetime.now().astimezone().tzinfo
        series = series.dt.tz_convert(local_tz)

        series = series.dt.tz_localize(None)

        return series
