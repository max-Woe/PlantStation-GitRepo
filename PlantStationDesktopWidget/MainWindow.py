import sys
from PySide6.QtWidgets import QMainWindow, QTabWidget, QLabel
from PySide6.QtCore import Qt
from PySide6.QtGui import QAction

from Context.StationDataContext import StationDataContext
from DataAcces.Repositories.MeasurementRepo import MeasurementRepo
from HelperServices.MeasurementValidationService import MeasurementValidationService
from ViewModels.DataBaseDialogViewModel import DataBaseDialogViewModel
from ViewModels.MainWindowViewModel import MainWindowViewModel
from ViewModels.SingleMeasurementWidgetViewModel import SingleMeasurementViewModel
from ViewModels.VpdMeasurementViewModel import VpdMeasurementViewModel
from Widgets.DataBaseConfigDialog import DataBaseConfigDialog
from Widgets.MeasurementWidgets.SingleMeasurementWidget import SingelementsWidget
from Widgets.MeasurementWidgets.VpdMeasurementWidget import VpdMeasurementWidget
from DataAcces.Models.Base import SessionLocal
from pathlib import Path


def resource_path(relative_path):
    if hasattr(sys, '_MEIPASS'):
        return Path(sys._MEIPASS) / relative_path
    return Path(__file__).parent / relative_path



class MainWindow(QMainWindow):
    def __init__(self, view_model:MainWindowViewModel|None = None):
        super().__init__()

        with open(resource_path("StyleSheets/light_stylesheet.qss"), "r") as file:
            self.setStyleSheet(file.read())

        self.database_dialog = None

        self.setAttribute(Qt.WidgetAttribute.WA_ShowWithoutActivating)
        self.setWindowTitle("PlantStation - Messwertübersicht")

        self._view_model = view_model

        default_size =(1024, 768)
        self.resize(default_size[0], default_size[1])
        scale_min_size = 0.75
        min_x_size = int(default_size[0]*scale_min_size)
        min_y_size = int(default_size[1]*scale_min_size)
        self.setMinimumSize(min_x_size, min_y_size)

        menu_bar = self.menuBar()
        database_menu = menu_bar.addMenu("Datenbank")

        open_action = QAction("&Öffnen", self)
        open_action.triggered.connect(self.open_new_window)
        database_menu.addAction(open_action)

        self.dark_mode_menu = menu_bar.addMenu("Dark-Mode")

        self.dark_action = QAction("&Dark", self)
        self.light_action = QAction("&Light", self)
        self.dark_action.triggered.connect(self.set_dark_mode)
        self.dark_mode_menu.addAction(self.dark_action)

        self.measurement_widgets = []

        self.outer_tabs_container = QTabWidget()
        self.outer_tabs_container.setObjectName("OuterTabsContainer")
        self.outer_tabs_container.setAttribute(Qt.WidgetAttribute.WA_StyledBackground, True)
        # self.inner_tabs_container = QTabWidget()

        for station_sensor_pair in self._view_model._station_sensor_pairs:
            station_id = station_sensor_pair[0]['Id']
            station_location = station_sensor_pair[0]['Location']
            sensors = station_sensor_pair[1]
            sensors= sensors.sort_values(by='Type')

            tab_headline = f"{station_location}, #{station_id}"

            # 1. Erzeuge für JEDE Station ein EIGENES TabWidget für die Sensoren
            inner_tabs_container = QTabWidget()
            inner_tabs_container.setObjectName("InnerTabsContainer")
            inner_tabs_container.setAttribute(Qt.WidgetAttribute.WA_StyledBackground, True)

            # 2. Füge diesen NEUEN Container dem äußeren TabWidget hinzu
            self.outer_tabs_container.addTab(inner_tabs_container, tab_headline)
            # 3. erstelle einen Validation-Service für die Messwerte
            measurement_validation_service = MeasurementValidationService()
            station_data_context = StationDataContext(station_id, MeasurementRepo(SessionLocal))

            # 4. Fülle den NEUEN Container mit den Sensoren dieser Station
            if inner_tabs_container.count() == 0:
                # 1. Den Titel des Tabs definieren, um dann gemeinsam mit ...
                inner_tab_headline = "VPD"
                # 2.dem ViewModel, für den Tab,
                vpd_measurement_widget_view_model = VpdMeasurementViewModel(station_data_context,
                                                                            measurement_validation_service,
                                                                            station_id
                                                                          )
                # 3. das Messwertewidget zu erstellen und dann
                vpd_measurement_widget = VpdMeasurementWidget(vpd_measurement_widget_view_model)
                # 4. dem inneren Container hinzuzufügen.
                inner_tabs_container.addTab(vpd_measurement_widget, inner_tab_headline)
            for index, sensor in sensors.iterrows():
                # 1. Den Typen und ...
                sensor_type = sensor['Type']
                # 2. die SensorId auslesen, um ...
                sensor_id = sensor['Id']
                # 3. den Titel des Tabs zu definieren.
                inner_tab_headline = f"{sensor_type}, #{sensor_id}"
                # 4. Das View-Model für den Tab erstellen um damit dann ...
                measurement_widget_view_model = (
                    SingleMeasurementViewModel(station_data_context, measurement_validation_service,
                                                station_id, sensor_id, sensor_type))
                # 5. das Messwerte-Widget für den Sensor zu erstellen und ...
                measurement_widget = SingelementsWidget(measurement_widget_view_model)
                self.measurement_widgets.append(measurement_widget)
                # 6. dieses dem aktuellen inneren Container hinzuzufügen.
                inner_tabs_container.addTab(measurement_widget, inner_tab_headline)

        self.status_label = QLabel()
        self.status_label.setText(view_model.get_status())
        self.statusBar().addPermanentWidget(self.status_label)

        self.setCentralWidget(self.outer_tabs_container)

    def open_new_window(self):
        if self.database_dialog is None:
            viewmodel = DataBaseDialogViewModel()
            self.database_dialog = DataBaseConfigDialog(viewmodel=viewmodel)

        if self.database_dialog.exec():  # exec() öffnet das Fenster modal und wartet
            print("Daten wurden gespeichert")

    def set_light_mode(self):
        theme = "light"
        with open(resource_path("StyleSheets/light_stylesheet.qss"), "r") as file:
            self.setStyleSheet(file.read())

        self._view_model.set_theme(theme)

        for measurement_widget in self.measurement_widgets:
            measurement_widget.plot_widget.current_theme = theme
            measurement_widget.plot_widget.change_theme(theme)

        # self.dark_action.triggered.disconnect(self.set_light_mode)
        self.dark_mode_menu.removeAction(self.light_action)

        self.dark_action.triggered.connect(self.set_dark_mode)
        self.dark_mode_menu.addAction(self.dark_action)

    def set_dark_mode(self):
        theme = "dark"
        with open(resource_path("StyleSheets/dark_stylesheet.qss"), "r") as file:
            self.setStyleSheet(file.read())

        self._view_model.set_theme(theme)

        for measurement_widget in self.measurement_widgets:
            measurement_widget.plot_widget.current_theme = theme
            measurement_widget.plot_widget.change_theme(theme)

        self.dark_mode_menu.removeAction(self.dark_action)

        self.light_action.triggered.connect(self.set_light_mode)
        self.dark_mode_menu.addAction(self.light_action)