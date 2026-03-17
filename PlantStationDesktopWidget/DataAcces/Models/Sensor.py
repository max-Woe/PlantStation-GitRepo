from sqlalchemy import Column, Integer, Float, String, DateTime, ForeignKey
from sqlalchemy.orm import relationship
from datetime import datetime, timezone

from DataAcces.Models.Station import Station
from DataAcces.Models.Base import Base


class Sensor(Base):
    __tablename__ = "Sensors"

    id = Column("Id", Integer, primary_key=True)
    type = Column("Type", String, nullable=False)
    unit = Column("Unit", String, nullable=False)
    device_id = Column("DeviceId", Integer, nullable=False)
    station_id = Column("StationId", Integer, ForeignKey("Stations.Id"), nullable=False)  # <-- FK
    created_at = Column("CreatedAt", DateTime, nullable=False, default=datetime.now(timezone.utc))
    updated_at = Column("UpdatedAt", DateTime, nullable=False, default=datetime.now(timezone.utc))

    station = relationship("Station")
