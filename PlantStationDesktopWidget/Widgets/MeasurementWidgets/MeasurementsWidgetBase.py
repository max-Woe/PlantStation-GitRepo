
from Widgets import TimePickerWidget, PlotWidgetBase, InfoWidgetBase, RefreshWidget
from PySide6.QtWidgets import QVBoxLayout, QWidget, QLabel, QStackedWidget
from PySide6.QtCore import Qt, QTimer
from ViewModels.MeasurementViewModelBase import MeasurementViewModelBase
from HelperServices.MeasurementValidationService import ValidationStatus
from HelperServices.ColorManagementService import ColorManagementService

class MeasurementsWidgetBase(QWidget):

    def __init__(self, view_model):
        super().__init__()

        self.setAttribute(Qt.WidgetAttribute.WA_StyledBackground, True)

        self._view_model = view_model
        self._color_management_service = ColorManagementService()

        self.layout = QVBoxLayout(self)

        #-----------------------------------BUTTONS-------------------------------------------------------------------#
        self.time_picker_widget = TimePickerWidget(self._view_model.radiobutton_times)
        self.time_picker_widget.timeSpanChanged.connect(self.on_time_changed)
        self.layout.addWidget(self.time_picker_widget, 0)

        #------------------------------------PLOT---------------------------------------------------------------------#

        self.init_plot_section()
        #------------------------------------LABELS-------------------------------------------------------------------#
        self.init_information_labels()
        #---------------------------------------------------REFRESH-BUTTON--------------------------------------------#
        self.refresh_widget = RefreshWidget(self._view_model)
        self.refresh_widget.button_clicked.connect(self.on_refresh_button_clicked)
        self.layout.addWidget(self.refresh_widget, 0)


        self.timer = QTimer(self)
        self.timer.timeout.connect(self.on_refresh_button_clicked)
        self.timer.start(60*1000)


    def init_plot_section(self):
        pass

    def init_information_labels(self):
        pass

    def on_time_changed(self, hours):
        self._view_model.since = hours
        self.update_all_displays()

    def on_refresh_button_clicked(self):
        pass

    def update_all_displays(self):
        pass

    def change_logo_color(self):
        pass