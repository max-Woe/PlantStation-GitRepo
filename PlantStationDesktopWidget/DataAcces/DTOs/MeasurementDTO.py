from dataclasses import dataclass
from datetime import datetime
from DataAcces.DTOs.SensorDTO import SensorDTO

@dataclass
class MeasurementDTO:
    id: int
    value: float
    unit: str
    type: str
    sensor_id: int
    sensor_id_reference: int | None
    sensor: SensorDTO | None
    recorded_at: datetime
    created_at: datetime
    updated_at: datetime