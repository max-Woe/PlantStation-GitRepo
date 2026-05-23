import pytest
import pandas as pd
from PlantStationDesktopWidget.HelperServices.MeasurementValidationService import validate_dataframe, ValidationStatus

#TODO: The function test is not completely isolated. The clean_by_limits function has to be mocked.

@pytest.mark.parametrize('input, output',[
    (
        pd.DataFrame({'Type': [], 'Value': []}),
        ValidationStatus.EMPTY
    ),
    (
        pd.DataFrame({'Type': ['temperature', 'temperature'], 'Value': [130, 130]}),
        ValidationStatus.INVALID
    ),
    (
        pd.DataFrame({'Type': ['temperature', 'temperature'], 'Value': [130, 30]}),
        ValidationStatus.PARTIAL_VALID
    ),
    (
        pd.DataFrame({'Type': ['temperature', 'temperature'], 'Value': [30, 30]}),
        ValidationStatus.ALL_VALID
    )
])
def test_validate_dataframe(input, output):
    ergebnis= validate_dataframe(input)
    assert ergebnis == output
