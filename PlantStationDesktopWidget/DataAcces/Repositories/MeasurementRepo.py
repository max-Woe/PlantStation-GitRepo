from typing import List

from DataAcces.DTOs.StationDTO import StationDTO
from DataAcces.Models.Base import SessionLocal
from DataAcces.Models.Measurement import Measurement
from DataAcces.DTOs.MeasurementDTO import MeasurementDTO
from DataAcces.DTOs.SensorDTO import SensorDTO

from datetime import datetime, timedelta, timezone


class MeasurementRepo:

    def __init__(self, session_factory=SessionLocal):
        self.session_factory = session_factory

    # Alle Messungen abrufen
    def get_all(self) -> List[MeasurementDTO]:
        with self.session_factory() as session:
            measurements = session.query(Measurement).all()
            return [self._map_to_dto(m) for m in measurements]

    # Einzelne Messung nach ID
    def get_by_id(self, measurement_id: int) -> MeasurementDTO | None:
        with self.session_factory() as session:
            measurements = session.query(Measurement).filter(Measurement.id == measurement_id).first()
            if not measurements:
                return None
            return self._map_to_dto(measurements)

    def get_all_by_sensor_id(self, sensor_id: int) -> list[MeasurementDTO]:
        with self.session_factory() as session:
            # Alle Measurements mit der angegebenen SensorId abrufen
            measurements = session.query(Measurement).filter(Measurement.sensor_id == sensor_id).all()
            return [self._map_to_dto(m) for m in measurements]

    def get_all_by_sensor_id_since(self, sensor_id: int, since: datetime) -> list[MeasurementDTO]:
        with self.session_factory() as session:
            measurements = session.query(Measurement).filter(Measurement.sensor_id == sensor_id,Measurement.recorded_at>since).all()
            return [self._map_to_dto(m) for m in measurements]

    # Messung hinzufügen
    def add(self, dto: MeasurementDTO) -> MeasurementDTO:
        from datetime import datetime
        from DataAcces.Models.Measurement import Measurement

        with self.session_factory() as session:
            entity = Measurement(
                value=dto.value,
                unit=dto.unit,
                type=dto.type,
                sensor_id=dto.sensor_id,
                sensor_id_reference=dto.sensor_id_reference,
                recorded_at=dto.recorded_at or datetime.now(timezone.utc)
            )
            session.add(entity)
            session.commit()
            session.refresh(entity)
            return self._map_to_dto(entity)

    # Hilfsmethode: Entity → DTO
    def _map_to_dto(self, measurement: Measurement) -> MeasurementDTO:
        station_dto = None
        sensor_dto = None
        if measurement.sensor.station:
            station = measurement.sensor.station
            station_dto = StationDTO(
                id=station.id,
                mac_address= station.mac_address,
                location= station.location,
                sensor_count= station.sensor_count,
                created_at= station.created_at

            )
        if measurement.sensor:
            sensor = measurement.sensor
            sensor_dto = SensorDTO(
                id=sensor.id,
                type=sensor.type,
                unit=sensor.unit,
                device_id=sensor.device_id,
                station_id=sensor.station_id,
                station=station_dto,
                created_at=sensor.created_at,
                updated_at=sensor.updated_at
            )

        return MeasurementDTO(
            id=measurement.id,
            value=measurement.value,
            unit=measurement.unit,
            type=measurement.type,
            sensor_id=measurement.sensor_id,
            sensor_id_reference=measurement.sensor_id_reference,
            sensor=sensor_dto,
            recorded_at=measurement.recorded_at,
            created_at=measurement.created_at,
            updated_at=datetime.now(timezone.utc)
        )