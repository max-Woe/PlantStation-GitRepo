from PySide6.QtWidgets import QVBoxLayout,QLabel, QWidget, QStackedWidget
from PySide6.QtCore import Qt, QTimer
from HelperServices.ColorManagementService import ColorManagementService
from Widgets.TimePickerWidget import TimePickerWidget
from Widgets.RefreshWidget import RefreshWidget
from Widgets.InfoWidgets.VpdMeasurementInfoWidget import VpdInfoWidget
from HelperServices.MeasurementValidationService import ValidationStatus

from ViewModels.VpdMeasurementViewModel import VpdMeasurementViewModel
from Widgets.MeasurementWidgets.MeasurementsWidgetBase import MeasurementsWidgetBase
from Widgets.PlotWidgets.VpdPlotWidget import VpdPlotWidget


class VpdMeasurementWidget(MeasurementsWidgetBase):
    def __init__(self, view_model: VpdMeasurementViewModel):
        super().__init__(view_model)

        self.setAttribute(Qt.WidgetAttribute.WA_StyledBackground, True)

        self._view_model = view_model
        self._color_management_service = ColorManagementService()


        # ------------------------------------------ALTERNATIVE-LABEL----------------------------------------------------#
        if (self._view_model.temperature_df is not None or self._view_model.humidity_df is not None or
                self._view_model.vpd_df is not None):
            self.no_data_in_df_label = QLabel("Für den gewählten Zeitraum stehen \n keine Messdaten zur verfügung!")
            self.no_data_in_df_label.setStyleSheet(
                "color: rgb(255, 0, 0); background-color: rgb(0,0,0); font-weight: bold; font-size: 20px")
            self.no_data_in_df_label.setAlignment(Qt.AlignmentFlag.AlignCenter)

        self.timer = QTimer(self)
        self.timer.timeout.connect(self.on_refresh_button_clicked)
        self.timer.start(60 * 1000)

    def init_plot_section(self):
        self.plot_widget = VpdPlotWidget(self._view_model)

        self.plot_error_label = QLabel("Es ist ein Fehler beim Plot aufgetreten!")
        self.plot_error_label.setObjectName("plot_error_label")
        # self.plot_error_label.setStyleSheet("color: red; font-size: 25px; font-weight: bold;")
        self.plot_error_label.setAlignment(Qt.AlignmentFlag.AlignCenter)

        self.stack = QStackedWidget()
        self.stack.addWidget(self.plot_widget)  # Index 0
        self.stack.addWidget(self.plot_error_label)  # Index 1
        self.layout.addWidget(self.stack)

        if (self._view_model.validation_status == ValidationStatus.PARTIAL_VALID or
                self._view_model.validation_status == ValidationStatus.ALL_VALID):

            if (self._view_model.humidity_df.empty or self._view_model.temperature_df.empty or
                    len(self._view_model.humidity_df) <= 1 or len(self._view_model.temperature_df) <= 1):
                self.stack.setCurrentWidget(self.plot_error_label)
            else:
                self.stack.setCurrentWidget(self.plot_widget)

            self.change_logo_color()

            self.plot_widget.plot(self._view_model.temperature_df,self._view_model.humidity_df,
                                  self._view_model.vpd_df)

        else:
            if self.stack.currentWidget() is not self.plot_error_label:
                self.stack.setCurrentWidget(self.plot_error_label)
            self.layout.replaceWidget(self.plot_widget, self.plot_error_label)
            self.plot_widget.hide()

            if self._view_model.validation_status == ValidationStatus.INVALID:
                self.plot_error_label.setText("Die Messwerte für diesen Zeitraum sind alle unzulässig, es liegt "
                                              "vermutlich ein Fehler mit dem Sensor vor..")
            elif self._view_model.validation_status == ValidationStatus.EMPTY:
                self.plot_error_label.setText("Es existieren keine Messwerte für diesen Zeitraum.")
            self.plot_error_label.show()

    def init_information_labels(self):
        self.infoWidget = VpdInfoWidget(self._view_model.station_id, self._view_model.temperature_df,self._view_model.humidity_df,
                                  self._view_model.vpd_df)
        self.layout.addWidget(self.infoWidget, 0)

        if (self._view_model.temperature_df is None or self._view_model.temperature_df.empty or
                self._view_model.humidity_df is None or self._view_model.humidity_df.empty or
                self._view_model.vpd_df is None or self._view_model.vpd_df.empty):
            self.infoWidget.hide()

    def on_refresh_button_clicked(self):
        self.update_all_displays()

    def update_all_displays(self):
        self._view_model.update_measurements()

        if (self._view_model.validation_status == ValidationStatus.ALL_VALID or
                self._view_model.validation_status == ValidationStatus.PARTIAL_VALID):
            temperature_df = self._view_model.temperature_df
            humidity_df = self._view_model.humidity_df
            vpd_df = self._view_model.vpd_df

            if self.stack.currentWidget() is not self.plot_widget:
                self.stack.setCurrentWidget(self.plot_widget)

            self.change_logo_color()

            self.plot_widget.plot(temperature_df, humidity_df, vpd_df)
            self.infoWidget.show()
            self.infoWidget.update_labels(temperature_df, humidity_df, vpd_df)

        else:
            if self.stack.currentWidget() is not self.plot_error_label:
                self.stack.setCurrentWidget(self.plot_error_label)
            self.layout.replaceWidget(self.plot_widget, self.plot_error_label)
            self.plot_widget.hide()

            if self._view_model.validation_status == ValidationStatus.INVALID:
                self.plot_error_label.setText("Die Messwerte für diesen Zeitraum sind alle unzulässig, es liegt "
                                              "vermutlich ein Fehler am dem Sensor vor.")
            elif self._view_model.validation_status == ValidationStatus.EMPTY:
                self.plot_error_label.setText("Es existieren keine Messwerte für diesen Zeitraum.")
            else:
                self.plot_error_label.setText("Es gab einen Fehler bei der Validierung.")

            self.plot_error_label.show()

            if self.stack.currentWidget() is not self.plot_error_label:
                self.stack.setCurrentWidget(self.plot_error_label)

            self.layout.replaceWidget(self.plot_widget, self.plot_error_label)
            self.plot_widget.hide()
            self.plot_error_label.show()

    def change_logo_color(self):
        measurement_df = self._view_model.vpd_df

        if 'Type' in measurement_df.columns:
            type = measurement_df.at[measurement_df.index[0], 'Type']
            value = measurement_df.at[measurement_df.index[0], 'Value']

            self._color_management_service.set_current_limits('vpd')
            color = self._color_management_service.get_logo_color(value)
            self.plot_widget.change_logo_color(color)