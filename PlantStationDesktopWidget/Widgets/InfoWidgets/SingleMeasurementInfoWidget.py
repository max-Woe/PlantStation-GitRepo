# from json.decoder import NaN
from HelperServices import GrowAnalyticsService
import pandas as pd
from PySide6.QtWidgets import QWidget, QGridLayout, QLabel
from PySide6.QtCore import Qt
from pandas import DataFrame

from Widgets.InfoWidgets.InfoWidgetBase import InfoWidgetBase


class SingleMeasurementInfoWidget(InfoWidgetBase):
    def __init__(self, station_id: int, measurements_df: DataFrame):
        super().__init__(station_id)
        self.setObjectName("SingleMeasurementInfoWidget")

        self.sensor_label = QLabel("")
        self.current_value_label = QLabel("")
        self.min_lable = QLabel("")
        self.max_lable = QLabel("")
        self.mean_lable = QLabel("")
        self.median_lable = QLabel("")

        self.grid_layout.addWidget(self.current_value_label, 0, 1)
        self.grid_layout.addWidget(self.current_time_label, 1, 1)
        self.grid_layout.addWidget(self.min_lable, 0, 3)
        self.grid_layout.addWidget(self.max_lable, 1, 3)
        self.grid_layout.addWidget(self.mean_lable, 2, 3)
        self.grid_layout.addWidget(self.median_lable, 3, 3)

        self.setMaximumHeight(100)

        self._df = measurements_df

        if not (self._df is None or self._df.empty):
            self.init_labels()

    def init_labels(self):
        self.setAttribute(Qt.WidgetAttribute.WA_StyledBackground, True)

        self.set_info_labels(self.station_id, self._df["SensorId"][0], self._df["Unit"][0], self._df["Type"][0])

        self.grid_layout.addWidget(self.station_label, 0, 0)
        self.grid_layout.addWidget(self.sensor_label, 1, 0)
        self.grid_layout.addWidget(self.unit_label, 2, 0)
        self.grid_layout.addWidget(self.type_label, 3, 0)

        self.current_value_label.setText("#value")
        self.current_time_label.setText("#value")

        self.grid_layout.addWidget(self.current_value_label, 0, 1)
        self.grid_layout.addWidget(self.current_time_label, 1, 1)
        self.grid_layout.addWidget(self.min_lable, 0, 3)
        self.grid_layout.addWidget(self.max_lable, 1, 3)
        self.grid_layout.addWidget(self.mean_lable, 2, 3)
        self.grid_layout.addWidget(self.median_lable, 3, 3)

        self.min_lable.setText("#min_value")
        self.max_lable.setText("#max_value")
        self.mean_lable.setText("#mean_value")
        self.median_lable.setText("#median_value")

        self._labels_initialized = True

        if self._df is not None:
            self.update_labels(self._df)

    def set_info_labels(self, station_id: int, sensor_id: int, unit: str, sensor_type: str):
        self.station_label.setText(f"Station Id: {station_id}")
        self.sensor_label.setText(f"Sensor Id: {sensor_id}")
        self.unit_label.setText(f"Unit: {unit}")
        self.type_label.setText(f"Type: {sensor_type}")

    def update_labels(self, df: DataFrame):
        self._df = df

        if not self._labels_initialized:
            self.init_labels()
            self.grid_layout.activate()
            self.updateGeometry()

        # Current measurement
        self.current_value_label.setText(
            f"Current value: {self._df["Value"].iloc[-1]:.2f}")
        current_time = self._df['RecordedAt'].iloc[-1]

        if self._df.iloc[-1]['RecordedAt'] is not pd.NaT:
            self.current_time_label.setText(f"Current time: {current_time.strftime('%H:%M:%S %d-%m-%Y')}")
        else:
            self.current_time_label.setText(f"Current time: {current_time}")

        if len(self._df.columns) == 3:
            self.current_vpd_label.setText(
                f"Current VPD-Value:{GrowAnalyticsService.calculate_room_vapor_pressure_difference()}")

        # statistic of the displayed measurements
        if self._df['Value'][0] != 'None':
            test = self._df["Value"][0]
            self.min_lable.setText(f"Min value: {self._df["Value"].min():.2f}")
            self.max_lable.setText(f"Max value: {self._df["Value"].max():.2f}")
            self.mean_lable.setText(f"Mean value: {self._df["Value"].mean():.2f}")
            self.median_lable.setText(f"Median value: {self._df["Value"].median():.2f}")