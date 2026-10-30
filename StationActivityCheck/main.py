#!/usr/bin/env python3

import datetime
from datetime import timezone, timedelta

import requests

BASE_URL = "http://192.168.178.21:5000"          #In der Entwicklung
# BASE_URL = "http://localhost:5000"          #Auf dem Pi
TIMEOUT = 5

def main():
    #Alle Station Ids abfragen...
    station_ids = get_all_station_ids()
    #... um damit alle Sensor Ids, geordnet nach Station Ids abzufragen...
    sensor_ids = {station_id: get_all_sensor_ids_by_station_id(station_id) for station_id in station_ids}
    #... um damit den letzten Messwert jedes Sensors abzufragen...
    for station_id in station_ids:
        if station_id != 28:
            last_measurement_timestamp = datetime.datetime.now(timezone.utc)
            for sensor_id in sensor_ids[station_id]:
                if sensor_id == 88 or sensor_id == 319:
                    continue
                #... damit geprüft werden kann, ob der aktuelle Messwert innerhalb der Grenzen (in der API vorgegeben) liegt.
                measurement = get_last_measurement_from_sensor_by_sensor_id(sensor_id)

                if is_sensor_value_valid(measurement["value"]):
                    print(build_local_check_line(0,f"Sensor{sensor_id}({measurement['type']})", measurement['value'], "Der Sensor liefert valide Werte."))
                else:
                    print(build_local_check_line(2,f"Sensor{sensor_id}({measurement['type']})", measurement['value'], "Der Sensor liefert keine validen Werte."))

                sensor_time = datetime.datetime.fromisoformat(measurement["recordedAt"])
                if (last_measurement_timestamp - sensor_time).total_seconds() > 0.0:
                    last_measurement_timestamp = sensor_time

            delta_last_station_update = datetime.datetime.now(timezone.utc)-last_measurement_timestamp
            if delta_last_station_update > datetime.timedelta(minutes=5):
                print(build_local_check_line(2, f"Station{station_id}", delta_last_station_update,
                                             "Die Station hat seit mehr als 5 Minuten keinen neuen Messwert erhalten."))
            elif delta_last_station_update> datetime.timedelta(minutes=2):
                print(build_local_check_line(1, f"Station{station_id}", delta_last_station_update,
                                             "Die Station hat seit mehr als 2 Minuten keinen neuen Messwert erhalten."))
            else:
                print(build_local_check_line(0, f"Station{station_id}", delta_last_station_update,
                                             "Die Station läuft aktiv."))

def check_for_timestamp(timestamp: datetime.datetime):
    if timestamp < datetime.datetime.now(timezone.utc) - datetime.timedelta(minutes=5):
        return False
    return True

def is_sensor_value_valid(sensor_value:float):
    if sensor_value == 9999:
        return False
    return True

def get_all_station_ids():
    response = requests.get(f"{BASE_URL}/api/Station/GetIds",
                            timeout=TIMEOUT)

    station_ids = response.json()

    return station_ids

def get_all_sensor_ids_by_station_id(station_id: int):
    response = requests.get(f"{BASE_URL}/api/Sensor/GetByStationId",
                            params={"stationId": station_id},
                            timeout=TIMEOUT)
    sensors = response.json()
    sensor_ids = [sensor["id"] for sensor in sensors]

    return sensor_ids

def get_last_measurement_from_sensor_by_sensor_id(sensor_id:int):
    response = requests.get(f"{BASE_URL}/api/Measurement/GetLastOfSensor",
                            params={"sensorId": sensor_id, "count": 1},
                            timeout=TIMEOUT)

    measurement = response.json()

    return measurement[0]


def build_local_check_line(status: int, servicename:str, metrics:int|timedelta, text:str):
    if isinstance(metrics, timedelta):
        metrics = int(metrics.total_seconds())
    return f'{status} "{servicename}" value={metrics} {text}'

if __name__ == "__main__":
    main()

