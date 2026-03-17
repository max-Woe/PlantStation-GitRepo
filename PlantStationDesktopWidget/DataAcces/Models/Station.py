from sqlalchemy import Column, Integer, Float, String, DateTime, ForeignKey
from sqlalchemy.orm import relationship
from datetime import datetime, timezone

from DataAcces.Models.Base import Base

class Station(Base):
    __tablename__ = "Stations"

    id = Column("Id", Integer, primary_key=True)
    mac_address = Column("MacAddress", String, nullable=False)
    location = Column("Location", String, nullable=False)
    sensor_count = Column("SensorsCount",Integer, nullable=False)
    created_at = Column("CreatedAt", DateTime, nullable=False, default=datetime.now(timezone.utc))