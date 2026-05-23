from enum import Enum

import pandas as pd

class ValidationStatus(Enum):
    VALID = 'VALID'
    ALL_VALID = 'ALL_VALID'
    PARTIAL_VALID = 'PARTIAL_VALID'
    INVALID = 'INVALID'
    EMPTY = 'EMPTY'
    VALIDATION_ERROR = 'VALIDATION_ERROR'


def validate_dataframe(df_original: pd.DataFrame) -> ValidationStatus:

    if df_original.empty:
        return ValidationStatus.EMPTY

    df = df_original.copy()

    df_cleared = clear_by_limits(df)

    if df_cleared.empty:
        return ValidationStatus.INVALID
    elif len(df_cleared) < len(df_original):
        return ValidationStatus.PARTIAL_VALID
    elif len(df_cleared) == len(df_original):
        return ValidationStatus.ALL_VALID
    else:
        return ValidationStatus.VALIDATION_ERROR

def validate_single_measurement(row: pd.Series) -> ValidationStatus:
    if row.empty:
        return ValidationStatus.EMPTY
    else:
        if row['Type'] == 'temperature' and row['Value'].between(0, 100):
            return ValidationStatus.VALID
        elif row['Type'] == 'humidity':
            return ValidationStatus.VALID
        elif row['Type'] == 'soil_moisture':
            return ValidationStatus.VALID
        else:
            return ValidationStatus.INVALID


def clear_by_limits(df_original: pd.DataFrame) -> pd.DataFrame:
    temperature_mask = (df_original['Type'] == 'temperature') & df_original['Value'].between(-30, 70)
    humidity_mask = (df_original['Type'] == 'humidity') & df_original['Value'].between(0, 100)
    soil_moisture_mask = (df_original['Type'] == 'soil_moisture') & df_original['Value'].between(0, 100)

    df_cleared = df_original[temperature_mask | humidity_mask | soil_moisture_mask].reset_index(drop=True)

    return df_cleared


