from datetime import datetime, timedelta, timezone

from PySide6.QtWidgets import QVBoxLayout, QWidget, QPushButton, QRadioButton, QHBoxLayout, QButtonGroup, QLabel, \
    QGridLayout
from PySide6.QtCore import Qt
from matplotlib.backends.backend_qtagg import FigureCanvasQTAgg as FigureCanvas
from matplotlib.figure import Figure
from numpy.ma.core import ceil, floor
from pandas import DataFrame

from ViewModels.MeasurementWidgetViewModel import MeasurementsWidgetViewModel


class MeasurementsWidget(QWidget):

    def __init__(self, view_model: MeasurementsWidgetViewModel):
        super().__init__()


        self._view_model = view_model

        self._measurement_timespan = 24
        self._latest_measurement_time = datetime.now(timezone.utc)-timedelta(hours= self._measurement_timespan)
        self._last_refresh_time = datetime.now(timezone.utc)

        self.df = self._view_model.getTimesAndValuesAsDataFrameSince(self._latest_measurement_time)

        self.radio_buttons_layout = QHBoxLayout()

        self.button_group = QButtonGroup(self)
        for i,(text, hours) in enumerate(self._view_model.radiobutton_times):
            radio_button = QRadioButton(text)
            self.radio_buttons_layout.addWidget(radio_button)
            self.button_group.addButton(radio_button, i)
        self.button_group.idClicked.connect(self.radio_button_clicked)
        self.button_group.button(self._view_model.selected_radiobutton_index).setChecked(True)

        self.layout = QVBoxLayout(self)
        self.layout.addLayout(self.radio_buttons_layout)
        #--------------------------------------------------------------------------------------------------------------#
        if self._view_model._list_of_measurementDTOs:

            self.info_grid_layout = QGridLayout()

            self.info_station_label = QLabel(f"Station: {self._view_model._sensor_id}")
            self.info_sensor_label = QLabel(f"Sensor: {self._view_model._sensor_id}")
            self.info_unit_label = QLabel(f"Unit: {self._view_model._unit}")
            self.info_type_label = QLabel(f"Type: {self._view_model._type}")

            self.info_grid_layout.addWidget(self.info_station_label,0,0)
            self.info_grid_layout.addWidget(self.info_sensor_label,1,0)
            self.info_grid_layout.addWidget(self.info_unit_label,2,0)
            self.info_grid_layout.addWidget(self.info_type_label,3,0)

            self.info_current_value_lable = QLabel(f"Current value: {self._view_model._list_of_measurementDTOs[-1].value:.2f}")
            self.info_current_time_lable = QLabel(f"Current time: {self._view_model._list_of_measurementDTOs[-1].recorded_at}")

            self.info_grid_layout.addWidget(self.info_current_value_lable,0,1)
            self.info_grid_layout.addWidget(self.info_current_time_lable,1,1)

            self.info_min_lable = QLabel(f"Min value: {self.df[self.df.columns[1]].min():.2f}")
            self.info_max_lable = QLabel(f"Max value: {self.df[self.df.columns[1]].max():.2f}")
            self.info_mean_lable = QLabel(f"Mean value: {self.df[self.df.columns[1]].mean():.2f}")
            self.info_median_lable = QLabel(f"Median value: {self.df[self.df.columns[1]].median():.2f}")

            self.info_grid_layout.addWidget(self.info_min_lable,0,3)
            self.info_grid_layout.addWidget(self.info_max_lable,1,3)
            self.info_grid_layout.addWidget(self.info_mean_lable,2,3)
            self.info_grid_layout.addWidget(self.info_median_lable,3,3)



            self.fig = Figure(figsize=(5, 2), dpi=100)
            self.ax = self.fig.add_subplot(111)
            self.canvas = FigureCanvas(self.fig)
            self.layout.addWidget(self.canvas)

            self.grid_layout_containter = QWidget()
            self.grid_layout_containter.setLayout(self.info_grid_layout)
            self.grid_layout_containter.setMaximumHeight(100)

            self.layout.addWidget(self.grid_layout_containter)

        refresh_button = QPushButton("Refresh")
        refresh_button.clicked.connect(self.refresh_data)
        self.layout.addWidget(refresh_button)

        self.no_data_in_df_label = QLabel("Für den gewählten Zeitraum stehen \n keine Messdaten zur verfügung!")
        self.no_data_in_df_label.setStyleSheet("color: rgb(255, 0, 0); background-color: rgb(0,0,0); font-weight: bold; font-size: 20px")
        self.no_data_in_df_label.setAlignment(Qt.AlignmentFlag.AlignCenter)

        self.plot_df()


    def plot_df(self):
        if self.df is None or self.df.empty:
            # 1. Sicherstellen, dass die Widgets existieren, bevor sie entfernt werden
            for widget_name in ["canvas", "grid_layout_containter"]:
                widget = getattr(self, widget_name, None)
                if widget is not None:
                    self.layout.removeWidget(widget)
                    widget.hide()  # WICHTIG: removeWidget versteckt das Widget nicht automatisch!

            # 2. Das Label anzeigen
            if hasattr(self, "no_data_in_df_label"):
                self.layout.insertWidget(max(0,self.layout.count()-1), self.no_data_in_df_label)
                self.no_data_in_df_label.show()

            return

        y_max = ceil(self.df[self.df.columns[1]].max() / 10) * 10
        y_min = floor(self.df[self.df.columns[1]].min() / 10) * 10

        color = 'black'

        if self.df.columns[1] == 'temperature':
            color = 'red'
        if self.df.columns[1] == 'humidity':
            color = 'blue'
        if self.df.columns[1] == 'soil_moisture':
            color = 'brown'

        self.ax.clear()
        self.ax.plot(self.df['Date'], self.df[self.df.columns[1]], color= color, label='Rohdaten')
        self.ax.set_title(self.df.columns[1].capitalize())
        self.ax.set_ylim(y_min, y_max)
        self.ax.set_xlabel('Zeit')
        self.ax.set_ylabel(f"{self.df.columns[1].capitalize()} [{self._view_model._list_of_measurementDTOs[0].unit}]")
        self.ax.grid(True)
        self.fig.autofmt_xdate()
        self.canvas.draw()

    def refresh_data(self):
        self._latest_measurement_time = datetime.now(timezone.utc)-timedelta(hours=self._measurement_timespan)
        self._view_model.updateMeasurementDTOs()
        self.df = self._view_model.getTimesAndValuesAsDataFrameSince(self._latest_measurement_time)
        if self.df is not None and not self.df.empty:
            self.plot_df()
            self.refresh_info_labels()

    def radio_button_clicked(self, index):
        if self._view_model.selected_radiobutton_index != index:
            self._view_model.selected_radiobutton_index = index
            self._view_model.selected_timespan = self._view_model.radiobutton_times[index][1]
            # self._view_model._last_update_time = datetime.now(timezone.utc)-timedelta(hours=self._view_model.selected_timespan)
            self._latest_measurement_time = datetime.now(timezone.utc)-timedelta(hours=self._view_model.selected_timespan)
            self._view_model.updateMeasurementDTOs()
            # self.df = self._view_model.getTimesAndValuesAsDataFrameSince(self._latest_measurement_time)
            self.df = DataFrame()
            self.df = self._view_model.getTimeAndValuesAsDataFrame()
            if self.df is not None and not self.df.empty:
                self.plot_df()
                self.refresh_info_labels()

    def refresh_info_labels(self):
        self.info_current_value_lable.setText(f"Current value: {self._view_model._list_of_measurementDTOs[-1].value}")
        self.info_current_time_lable.setText(f"Current time: {self._view_model._list_of_measurementDTOs[-1].recorded_at}")

        self.info_min_lable.setText(f"Min value: {self.df[self.df.columns[1]].min()}")
        self.info_max_lable.setText(f"Max value: {self.df[self.df.columns[1]].max()}")
        self.info_mean_lable.setText(f"Mean value: {self.df[self.df.columns[1]].mean()}")
        self.info_median_lable.setText(f"Median value: {self.df[self.df.columns[1]].median()}")