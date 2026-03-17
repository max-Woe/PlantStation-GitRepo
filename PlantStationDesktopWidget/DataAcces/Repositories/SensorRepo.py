from typing import List
from DataAcces.DTOs.SensorDTO import SensorDTO
from DataAcces.Models.Base import SessionLocal
from DataAcces.Models.Sensor import Sensor

class SensorRepo:
    def __init__(self, session_factory=SessionLocal):
        self.session_factory = session_factory

    def get_all(self) -> List[SensorDTO]:
        with self.session_factory() as session:
            session.query(Sensor).all()

    def get_sensor_ids_by_station_id(self, station_id: int) -> List[int]:
        with self.session_factory() as session:
            sensor_ids = session.query(Sensor).filter(Sensor.station_id == station_id).all()
            return [sensor.id for sensor in sensor_ids]

