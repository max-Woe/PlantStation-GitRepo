import pandas as pd

def seperate_measurements_df(measurements_df: pd.DataFrame) -> dict[str, pd.DataFrame]:
    measurement_types = get_measurement_types(measurements_df['Type'])
    measurements_dict = {
        measurement_type: measurements_df[measurements_df['Type'] == measurement_type].reset_index(drop=True)
        for measurement_type in measurement_types
    }
    return measurements_dict

def get_measurement_types(types_series: pd.Series)-> list[str]:
    types_list = types_series.unique().tolist()

    return types_list
