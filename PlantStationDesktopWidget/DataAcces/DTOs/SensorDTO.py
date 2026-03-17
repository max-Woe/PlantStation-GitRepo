from dataclasses import dataclass
from DataAcces.DTOs.StationDTO import *

@dataclass
class SensorDTO:
    id: int
    type: str
    unit: str
    device_id: int
    station_id: int
    station: StationDTO
    created_at: datetime
    updated_at: datetime