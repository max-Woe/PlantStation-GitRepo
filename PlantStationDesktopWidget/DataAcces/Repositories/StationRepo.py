from typing import List
from DataAcces.Models.Base import SessionLocal
from DataAcces.Models.Station import Station
from DataAcces.DTOs.StationDTO import StationDTO

class StationRepo:
    def __init__(self, session_factory=SessionLocal):
        self.session_factory = session_factory


    def get_all(self) -> List[StationDTO]:
        with self.session_factory() as session:
            stations = session.query(Station).all()
            return [self._map_to_dto(s) for s in stations]

    def get_all_station_ids(self) -> List[int]:
        with self.session_factory() as session:
            stations = session.query(Station).all()
            return [station.id for station in stations]

    def _map_to_dto(self, station: Station) -> StationDTO:
        return StationDTO(
            id=station.id,
            mac_address=station.mac_address,
            location=station.location,
            sensor_count=station.sensor_count,
            created_at=station.created_at,
        )