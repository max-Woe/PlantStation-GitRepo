# from json.decoder import NaN
from HelperServices import GrowAnalyticsService
import pandas as pd
from PySide6.QtWidgets import QWidget, QGridLayout, QLabel
from PySide6.QtCore import Qt
from pandas import DataFrame

from Widgets.InfoWidgets.InfoWidgetBase import InfoWidgetBase


class VpdInfoWidget(InfoWidgetBase):
    def __init__(self, station_id: int, temperature_df: DataFrame,
                 humidity_df: DataFrame, vpd_df: DataFrame):
        super().__init__(station_id)

        self.setObjectName("VpdMeasurementInfoWidget")


        self.current_temperature_value_label = QLabel("")
        self.current_humidity_value_label = QLabel("")
        self.current_vpd_value_label = QLabel("")

        self.grid_layout.addWidget(self.current_temperature_value_label, 0, 1)
        self.grid_layout.addWidget(self.current_humidity_value_label, 1, 1)
        self.grid_layout.addWidget(self.current_vpd_value_label, 2, 1)


        self._temperature_df = temperature_df
        self._humidity_df = humidity_df
        self._vpd_df = vpd_df

        if not (self._vpd_df is None or self._vpd_df.empty):
            self.init_labels()

    def init_labels(self):
        self.setAttribute(Qt.WidgetAttribute.WA_StyledBackground, True)

        self.set_info_labels(self.station_id,"kPa", "vpd")

        self.grid_layout.addWidget(self.station_label, 0, 0)
        self.grid_layout.addWidget(self.current_time_label, 1, 0)
        self.grid_layout.addWidget(self.unit_label, 2, 0)
        self.grid_layout.addWidget(self.type_label, 3, 0)

        self.current_temperature_value_label.setText("#value")
        self.current_humidity_value_label.setText("#value")
        self.current_vpd_value_label.setText("#value")
        self.current_time_label.setText("#value")

        self.grid_layout.addWidget(self.current_temperature_value_label, 0, 1)
        self.grid_layout.addWidget(self.current_humidity_value_label, 1, 1)
        self.grid_layout.addWidget(self.current_vpd_value_label, 2, 1)

        self._labels_initialized = True

        if (self._temperature_df is not None and
            self._humidity_df is not None and
            self._vpd_df is not None):
            self.update_labels(self._temperature_df,self._humidity_df, self._vpd_df)

    def set_info_labels(self, station_id: int, unit: str, sensor_type: str):
        self.station_label.setText(f"Station Id: {station_id}")
        self.unit_label.setText(f"Unit: {unit}")
        self.type_label.setText(f"Type: {sensor_type}")

    def update_labels(self, temperature_df: DataFrame, humidity_df: DataFrame, vpd_df: DataFrame):
        self._temperature_df = temperature_df
        self._humidity_df = humidity_df
        self._vpd_df = vpd_df

        if not self._labels_initialized:
            self.init_labels()
            self.grid_layout.activate()
            self.updateGeometry()

        # Current measurement
        self.current_temperature_value_label.setText(
            f"Current temperature: {self._temperature_df["Value"].iloc[-1]:.2f}")
        self.current_humidity_value_label.setText(
            f"Current humidity: {self._humidity_df["Value"].iloc[-1]:.2f}")
        self.current_vpd_value_label.setText(
            f"Current Vpd: {self._vpd_df["Value"].iloc[-1]:.2f}")
        current_time = self._vpd_df['RecordedAt'].iloc[-1]

        if self._vpd_df.iloc[-1]['RecordedAt'] is not pd.NaT:
            self.current_time_label.setText(f"Current time: {current_time.strftime('%H:%M:%S %d-%m-%Y')}")
        else:
            self.current_time_label.setText(f"Current time: {current_time}")