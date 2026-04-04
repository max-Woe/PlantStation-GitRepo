from Widgets import TimePickerWidget, PlotWidget, InfoWidget, RefreshWidget
from PySide6.QtWidgets import QVBoxLayout, QWidget, QLabel
from PySide6.QtCore import Qt, QTimer
from ViewModels.MeasurementWidgetViewModel import MeasurementsWidgetViewModel


class MeasurementsWidget(QWidget):

    def __init__(self, view_model: MeasurementsWidgetViewModel):
        super().__init__()

        self._view_model = view_model
        self.layout = QVBoxLayout(self)

        #-----------------------------------BUTTONS-------------------------------------------------------------------#
        self.time_picker_widget = TimePickerWidget(self._view_model.radiobutton_times)
        self.time_picker_widget.timeSpanChanged.connect(self.on_time_changed)
        self.layout.addWidget(self.time_picker_widget, 0)

        #------------------------------------PLOT---------------------------------------------------------------------#
        self.plot_widget = PlotWidget(self._view_model)
        self.layout.addWidget(self.plot_widget, 1)
        self.plot_widget.plot(self._view_model.measurement_df)

        #------------------------------------LABELS-------------------------------------------------------------------#
        if self._view_model.measurement_df is not None:

            self.infoWidget = InfoWidget(self._view_model.station_id, self._view_model.measurement_df)
            self.layout.addWidget(self.infoWidget, 0)

        #---------------------------------------------------REFRESH-BUTTON--------------------------------------------#
        self.refresh_widget = RefreshWidget(self._view_model)
        self.refresh_widget.button_clicked.connect(self.on_refresh_button_clicked)
        self.layout.addWidget(self.refresh_widget, 0)

        #------------------------------------------ALTERNATIVE-LABEL----------------------------------------------------#
        if self._view_model.measurement_df is not None:
            self.no_data_in_df_label = QLabel("Für den gewählten Zeitraum stehen \n keine Messdaten zur verfügung!")
            self.no_data_in_df_label.setStyleSheet("color: rgb(255, 0, 0); background-color: rgb(0,0,0); font-weight: bold; font-size: 20px")
            self.no_data_in_df_label.setAlignment(Qt.AlignmentFlag.AlignCenter)

        self.timer = QTimer(self)
        self.timer.timeout.connect(self.on_refresh_button_clicked)
        self.timer.start(60*1000)

    def on_time_changed(self, hours):
        self._view_model.selected_timespan = hours
        self.update_all_displays()

    def on_refresh_button_clicked(self):
        self.update_all_displays()

    def update_all_displays(self):
        self._view_model.update_measurements()
        measurement_df = self._view_model.measurement_df
        self.plot_widget.plot(measurement_df)
        self.infoWidget.update_labels(measurement_df)
