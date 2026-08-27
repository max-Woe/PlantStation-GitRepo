import pytest
import pandas as pd
from unittest.mock import Mock
from datetime import datetime
from Context.StationDataContext import StationDataContext
from DataAcces.Repositories.MeasurementRepo import MeasurementRepo

@pytest.fixture
def measurement_repo():
    measurement_repo = Mock(spec=MeasurementRepo)
    measurement_repo.get_measurements_by_station_id_since.return_value = pd.DataFrame(
        {
            "RecordedAt": [
                datetime(2026, 8, 13, 13, 00, 00),
                           datetime(2026, 8, 13, 13, 1, 00)
            ],
            "Value": [24, 55],
            "Type": ["temperatur", "humidity"]
        })
    return measurement_repo

@pytest.fixture
def station_data(measurement_repo):
    return StationDataContext( station_id = 27,
                               measurements_repo = measurement_repo,
                               since = datetime(2026, 8, 13, 13, 00, 00))


@pytest.mark.parametrize("expected",
    [
        {
            'temperatur': pd.DataFrame({
                'RecordedAt': [datetime(2026, 8, 13, 13, 00, 00)],
                'Value': [24],
                'Type': ['temperatur']}),
            'humidity': pd.DataFrame({
                'RecordedAt': [datetime(2026, 8, 13, 13, 1, 00)],
                'Value': [55],
                'Type': ['humidity']}),
        }
    ]
)
def test_get_all_measurements_from_repo(station_data, expected):
    result = station_data._set_measurements_df_and_dict_by_type()

    assert result.keys() == expected.keys()

    for measurement_type in expected:
        pd.testing.assert_frame_equal(
            result[measurement_type],
            expected[measurement_type]
        )

@pytest.mark.parametrize("measurement_type, expected", [
        (
            "temperatur",
            pd.DataFrame({
                'RecordedAt': [datetime(2026, 8, 13, 13, 00, 00)],
                'Value': [24],
                'Type': ['temperatur']})
        ),
        (
            'humidity',
            pd.DataFrame({
                'RecordedAt': [datetime(2026, 8, 13, 13, 1, 00)],
                'Value': [55],
                'Type': ['humidity']}),
        )
    ])

def test_get_measurements_by_type(station_data, measurement_type, expected):
    result = station_data.get_measurements_by_type(measurement_type)

    pd.testing.assert_frame_equal(result, expected)