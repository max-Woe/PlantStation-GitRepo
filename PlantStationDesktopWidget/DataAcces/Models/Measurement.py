from sqlalchemy import Column, Integer, Float, String, DateTime, ForeignKey
from sqlalchemy.orm import relationship
from datetime import datetime, timezone
from DataAcces.Models.Base import Base
from DataAcces.Models.Sensor import Sensor

class Measurement(Base):
    __tablename__ = "Measurements"

    id = Column("Id", Integer, primary_key=True)
    value = Column("Value", Float, nullable=False)
    unit = Column("Unit", String, nullable=False)
    type = Column("Type", String, nullable=False)
    sensor_id = Column("SensorId", Integer, ForeignKey("Sensors.Id"), nullable=False)
    sensor_id_reference = Column("SensorIdReference", Integer, nullable=True)
    recorded_at = Column("RecordedAt", DateTime, default=datetime.now(timezone.utc))
    created_at = Column("CreatedAt", DateTime, default=datetime.now(timezone.utc))

    sensor = relationship(Sensor)