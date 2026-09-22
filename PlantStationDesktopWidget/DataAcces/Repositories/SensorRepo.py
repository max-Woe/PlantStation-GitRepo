from typing import List
from DataAcces.Models.Base import SessionLocal
from DataAcces.Models.Sensor import Sensor
from typing import cast

import pandas as pd

class SensorRepo:
    def __init__(self, session_factory=SessionLocal):
        self.session_factory = session_factory

    def get_all(self) -> pd.DataFrame:
        """Fetches all sensors.

        :return: pd.DataFrame containing all sensors"""
        with self.session_factory() as session:
            session.query(Sensor).all()
            df = pd.read_sql(session.query(Sensor).statement, session.bind)
            df = cast(pd.DataFrame, df)

            if df.empty:
                return pd.DataFrame()

            return df

    def get_sensor_ids_by_station_id(self, station_id: int) -> List[int]:
        """Fetches all sensors id's with the given station id.

        :param station_id: The ID of the station whose measurements should be fetched.
        :return: List[int] containing all sensors with the given station id"""
        with self.session_factory() as session:
            sensor_ids = session.query(Sensor).filter(Sensor.station_id == station_id).all()
            return [sensor.id for sensor in sensor_ids]

    def get_sensors_by_station_id(self, station_id: int) -> pd.DataFrame:
        """Fetches all sensors with the given station id.

        :param station_id: The ID of the station whose measurements should be fetched.
        :return: pd.DataFrame containing all sensors with the given station id"""
        with self.session_factory() as session:
            query = session.query(Sensor).filter(Sensor.station_id == station_id)
            df = pd.read_sql(query.statement, session.bind)
            df = cast(pd.DataFrame, df)

            if df.empty:
                return pd.DataFrame()

            return df

