import pytest
import pandas as pd
from unittest.mock import MagicMock, patch
from datetime import datetime

from DataAcces.Repositories.MeasurementRepo import MeasurementRepo
# from Models.Measurement import Measurement

from Models.Base import SessionLocal

@pytest.fixture
def measurement_repo():
    # session_factory = MagicMock()
    # session = MagicMock()
    # session_factory.return_value.__enter__.return_value = session

    return MeasurementRepo(session_factory=SessionLocal)

@pytest.mark.parametrize('db_result, expected',
    [(
pd.DataFrame({
            'Id' : [1, 2, 3],
            'Value' : [4, 5, 6],
            'Unit': ["°C", "°C", "°C"],
            'SensorId': [33, 33, 33],
            'SensorIdReference' : [999999, 999999, 999999],
            'RecordedAt': [datetime(2026,8,13,11,0,0),
                           datetime(2026,8,13,11,1,0),
                           datetime(2026,8,13,11,2,0)],
            'CreatedAt':  [datetime(2026,8,13,13,0,0),
                           datetime(2026,8,13,13,1,0),
                           datetime(2026,8,13,13,2,0)],
            'Type': ['temperature', 'temperature', 'temperature'],
        }),
        pd.DataFrame({
            'Id' : [1, 2, 3],
            'Value' : [4, 5, 6],
            'Unit': ["°C", "°C", "°C"],
            'SensorId': [33, 33, 33],
            'RecordedAt': [datetime(2026,8,13,13,0,0),
                           datetime(2026,8,13,13,1,0),
                           datetime(2026,8,13,13,2,0)],
            'CreatedAt':  [datetime(2026,8,13,13,0,0),
                           datetime(2026,8,13,13,1,0),
                           datetime(2026,8,13,13,2,0)],
            'Type': ['temperature', 'temperature', 'temperature'],
        })
    )])

def test_get_all(measurement_repo: MeasurementRepo, expected: pd.DataFrame, db_result):
    # with patch("DataAcces.Repositories.MeasurementRepo.pd.read_sql",
    #            return_value = db_result):
    result = measurement_repo.get_all()

    pd.testing.assert_frame_equal(result, expected)

# @pytest.mark.parametrize('sensor_id, db_result, expected',
#                          (
#                              33,
#                              pd.DataFrame({
#                                  'Id' : [1, 2, 3],
#                                  'Value' : [4, 5, 6],
#                                  'Unit' : ["°C", "°C", "°C"],
#                                  'SensorId' : [33, 33, 33],
#                                  'SensorIdReference': [999999, 999999, 999999],
#                                  'RecordedAt': [datetime(2026,8,13,11,0,0),
#                                                 datetime(2026,8,13,11,0,0),
#                                                 datetime(2026,8,13,11,0,0)],
#                                  'CreatedAt': [datetime(2026,8,13,13,0,0),
#                                                datetime(2026,8,13,13,0,0),
#                                                datetime(2026,8,13,13,0,0)],
#                                  "Type": ["temperature", "temperature", "temperature"]
#                              }),
#                              pd.DataFrame({
#                                  'Id' : [1, 2, 3],
#                                  'Value' : [4, 5, 6],
#                                  'Unit' : ["°C", "°C", "°C"],
#                                  'SensorId' : [33, 33, 33],
#                                  'SensorIdReference': [999999, 999999, 999999],
#                                  'RecordedAt': [datetime(2026,8,13,13,0,0),
#                                                 datetime(2026,8,13,13,0,0),
#                                                 datetime(2026,8,13,13,0,0)],
#                                  'CreatedAt': [datetime(2026,8,13,13,0,0),
#                                                datetime(2026,8,13,13,0,0),
#                                                datetime(2026,8,13,13,0,0)],
#                                  "Type": ["temperature", "temperature", "temperature"]
#                              })
#                          ))
#
# def test_get_by_id(measurement_repo: MeasurementRepo, db_result: pd.DataFrame,sensor_id: int, expected: pd.DataFrame):
#     with patch("DataAcces.Repositories.MeasurementRepo.pd.read_sql",return_value=expected):
#         result = measurement_repo.get_by_id(sensor_id)
#
#     pd.testing.assert_frame_equal(result, db_result)