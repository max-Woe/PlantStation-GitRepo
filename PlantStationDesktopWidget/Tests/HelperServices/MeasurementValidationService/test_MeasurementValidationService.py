from unittest.mock import patch

import pytest
import pandas as pd
from HelperServices.MeasurementValidationService import MeasurementValidationService, ValidationStatus


@pytest.fixture
def validation_service():
    return MeasurementValidationService()


@pytest.mark.parametrize("measurement_df, expected", [
    (
            pd.DataFrame({"Type": ['temperature','humidity','soil_moisture'], "Value": [20, 50, 30]}),
            pd.DataFrame({"Type": ['temperature', 'humidity', 'soil_moisture'], "Value": [20, 50, 30]}),
    ),
    (
            pd.DataFrame({"Type": ['temperature', 'temperature'], "Value": [20, 120]}),
            pd.DataFrame({"Type": ['temperature'], "Value": [20]})
    ),
    (
            pd.DataFrame({"Type": ['humidity', 'humidity'], "Value": [50, 120]}),
            pd.DataFrame({"Type": ['humidity'], "Value": [50]})
    ),
    (
            pd.DataFrame({"Type": ['soil_moisture', 'soil_moisture'], "Value": [30, 120]}),
            pd.DataFrame({"Type": ['soil_moisture'], "Value": [30]})
    ),
    (
            pd.DataFrame({"Type": ['soil_moisture', 'unknowwn'], "Value": [30, 20]}),
            pd.DataFrame({"Type": ['soil_moisture'], "Value": [30]})
    ),
    (
            pd.DataFrame({"Type": ['soil_moisture', 'soil_moisture'], "Value": [120.0, 120.0]}),
            pd.DataFrame({"Type": pd.Series([], dtype=str), "Value": pd.Series([], dtype=float)})
    )
])

def test_clear_by_limits(validation_service, measurement_df, expected):
    result = validation_service.clear_by_limits(df_original=measurement_df)
    pd.testing.assert_frame_equal(result, expected)



@pytest.mark.parametrize('mock_df, output',[
    (
        pd.DataFrame({'Type': [], 'Value': []}),
        ValidationStatus.INVALID
    ),
    (
        pd.DataFrame({'Type': ['temperature'], 'Value': [30]}),
        ValidationStatus.PARTIAL_VALID
    ),
    (
        pd.DataFrame({'Type': ['temperature', 'temperature'], 'Value': [30, 30]}),
        ValidationStatus.ALL_VALID
    )
])
def test_validate_dataframe(validation_service, mock_df, output):
    measurement_df = pd.DataFrame({"Type": ["temperature", "temperature"], "Value": [30, 30]})

    with patch('HelperServices.MeasurementValidationService.MeasurementValidationService.clear_by_limits') as mock_clean_by_limits:
        mock_clean_by_limits.return_value = mock_df
        ergebnis= validation_service.validate_dataframe(measurement_df)

        mock_clean_by_limits.assert_called_once()
        assert ergebnis == output


def test_validate_dataframe_with_empty_df(validation_service):
    measurement_df = pd.DataFrame({"Type": [], "Value": []})

    validation_status = validation_service.validate_dataframe(measurement_df)

    assert validation_status == ValidationStatus.EMPTY