import pytest
import pandas as pd
from PlantStationDesktopWidget.HelperServices import MeasurementValidationService

@pytest.mark.parametrize('measurement_df, erwartet',[
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
def test_clear(measurement_df, erwartet):
    ergebnis= MeasurementValidationService.clear_by_limits(measurement_df)
    pd.testing.assert_frame_equal(ergebnis, erwartet)



