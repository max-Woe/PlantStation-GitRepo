import math
import pandas as pd

def calculate_room_saturation_vapor_pressure(temperature:float) -> float:
    room_vsp = 0.6108*math.exp(17.27*temperature/(temperature+237.3))
    return room_vsp


def calculate_leaf_saturation_vapor_pressure(temperature:float)-> float:
    room_vsp = 0.6108 * math.exp(17.27 * (temperature - 2) / ((temperature - 2) + 237.3))
    return room_vsp

def calculate_vapor_pressure_difference(temperature_df: pd.DataFrame, humidity_df: pd.DataFrame) -> pd.DataFrame:
    vpd_df = temperature_df.copy()
    vpd_df['Type'] = 'vpd'
    vpd_df['Unit'] = 'kPa'

    for index in vpd_df.index:
        temperature: float = temperature_df.loc[index, 'Value'] # type: ignore
        humidity: float = humidity_df.loc[index, 'Value'] # type:ignore
        vpd_df.loc[index,'Value'] = calculate_leaf_saturation_vapor_pressure(temperature) - calculate_room_saturation_vapor_pressure(temperature) * humidity / 100
    return vpd_df