from PySide6.QtWidgets import QWidget, QGridLayout, QLabel
from pandas import DataFrame


class InfoWidget(QWidget):
    def __init__(self, station_id: int,measurements_df:DataFrame):
        super().__init__()

        self._df = measurements_df
        self.grid_layout = QGridLayout(self)

        self.station_label = QLabel("#id")
        self.station_label.setObjectName("StationId")
        self.sensor_label = QLabel("#id")
        self.unit_label = QLabel("#unit")
        self.type_label = QLabel("#type")

        self.set_info_labels(station_id,self._df["SensorId"][0],self._df["Unit"][0],self._df["Type"][0])

        self.grid_layout.addWidget(self.station_label, 0, 0)
        self.grid_layout.addWidget(self.sensor_label, 1, 0)
        self.grid_layout.addWidget(self.unit_label, 2, 0)
        self.grid_layout.addWidget(self.type_label, 3, 0)

        self.current_value_label = QLabel("#value")
        self.current_time_label = QLabel("#value")

        self.grid_layout.addWidget(self.current_value_label, 0, 1)
        self.grid_layout.addWidget(self.current_time_label, 1, 1)

        self.min_lable = QLabel("#min_value")
        self.max_lable = QLabel("#max_value")
        self.mean_lable = QLabel("#mean_value")
        self.median_lable = QLabel("#median_value")

        self.grid_layout.addWidget(self.min_lable, 0, 3)
        self.grid_layout.addWidget(self.max_lable, 1, 3)
        self.grid_layout.addWidget(self.mean_lable, 2, 3)
        self.grid_layout.addWidget(self.median_lable, 3, 3)

        self.update_labels(self._df)

        self.setMaximumHeight(100)
        
    def set_info_labels(self, station_id:int, sensor_id:int, unit:str, sensor_type:str):
        self.station_label.setText(f"Station Id: {station_id}")
        self.sensor_label.setText(f"Sensor Id: {sensor_id}")
        self.unit_label.setText(f"Unit: {unit}")
        self.type_label.setText(f"Type: {sensor_type}")

    def update_labels(self, df:DataFrame):
        # self.df = DataFrame()
        self.df = df

        # Current measurement
        self.current_value_label.setText(
            f"Current value: {self.df["Value"].iloc[-1]}")
        current_time = self.df['RecordedAt'].iloc[-1]
        self.current_time_label.setText(
            f"Current time: {current_time.strftime('%H:%M:%S %d-%m-%Y')}")

        # statistic of the displayed measurements
        self.min_lable.setText(f"Min value: {self.df["Value"].min()}")
        self.max_lable.setText(f"Max value: {self.df["Value"].max()}")
        self.mean_lable.setText(f"Mean value: {self.df["Value"].mean()}")
        self.median_lable.setText(f"Median value: {self.df["Value"].median()}")