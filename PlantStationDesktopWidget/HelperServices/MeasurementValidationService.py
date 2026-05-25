from enum import Enum

import pandas as pd

class ValidationStatus(Enum):
    VALID = 'VALID'
    ALL_VALID = 'ALL_VALID'
    PARTIAL_VALID = 'PARTIAL_VALID'
    INVALID = 'INVALID'
    EMPTY = 'EMPTY'
    VALIDATION_ERROR = 'VALIDATION_ERROR'


class MeasurementValidationService:
    def __init__(self,
                 temp_range = (-30,70),
                 hum_range = (0,100),
                 soil_moisture_range = (0,100)):

        self.temp_range = temp_range
        self.hum_range = hum_range
        self.soil_moisture_range = soil_moisture_range

    def validate_dataframe(self, df_original: pd.DataFrame) -> ValidationStatus:

        if df_original.empty:
            return ValidationStatus.EMPTY

        df = df_original.copy()

        df_cleared = self.clear_by_limits(df)

        if df_cleared.empty:
            return ValidationStatus.INVALID
        elif len(df_cleared) < len(df_original):
            return ValidationStatus.PARTIAL_VALID
        elif len(df_cleared) == len(df_original):
            return ValidationStatus.ALL_VALID
        else:
            return ValidationStatus.VALIDATION_ERROR

    def clear_by_limits(self, df_original: pd.DataFrame) -> pd.DataFrame:
        temperature_mask = (df_original['Type'] == 'temperature') & df_original['Value'].between(*self.temp_range)
        humidity_mask = (df_original['Type'] == 'humidity') & df_original['Value'].between(*self.hum_range)
        soil_moisture_mask = (df_original['Type'] == 'soil_moisture') & df_original['Value'].between(*self.soil_moisture_range)

        df_cleared = df_original[temperature_mask | humidity_mask | soil_moisture_mask].reset_index(drop=True)

        return df_cleared


