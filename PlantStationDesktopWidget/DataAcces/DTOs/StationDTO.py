from dataclasses import dataclass
from datetime import datetime

@dataclass
class StationDTO:
    id: int
    mac_address: str
    location: str
    sensor_count: int
    created_at: datetime