from PySide6.QtWidgets import QMainWindow, QTabWidget, QLabel
from PySide6.QtCore import Qt
from PySide6.QtGui import QAction

from DataAcces.Repositories.MeasurementRepo import MeasurementRepo
from ViewModels.DataBaseDialogViewModel import DataBaseDialogViewModel
from ViewModels.MainWindowViewModel import MainWindowViewModel
from ViewModels.MeasurementWidgetViewModel import MeasurementsWidgetViewModel
from Widgets.DataBaseConfigDialog import DataBaseConfigDialog
from Widgets.MeasurementsWidget import MeasurementsWidget
from DataAcces.Models.Base import SessionLocal


class MainWindow(QMainWindow):
    def __init__(self, view_model:MainWindowViewModel|None = None):
        super().__init__()

        self.database_dialog = None

        self.setAttribute(Qt.WidgetAttribute.WA_ShowWithoutActivating)
        self.setWindowTitle("PlantStation - Messwertübersicht")

        self.view_model = view_model

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

        self.outer_tabs_container = QTabWidget()
        self.inner_tabs_container = QTabWidget()

        for station_sensor_pair in self.view_model._station_sensor_pairs:
            station_id = station_sensor_pair[0]['Id']
            station_location = station_sensor_pair[0]['Location']
            sensors = station_sensor_pair[1]
            sensors= sensors.sort_values(by='Type')

            tab_headline = f"{station_location}, #{station_id}"

            # 1. Erzeuge für JEDE Station ein EIGENES TabWidget für die Sensoren
            new_inner_tabs_container = QTabWidget()

            # 2. Füge diesen NEUEN Container dem äußeren TabWidget hinzu
            self.outer_tabs_container.addTab(new_inner_tabs_container, tab_headline)

            # 3. Fülle den NEUEN Container mit den Sensoren dieser Station
            for index, sensor in sensors.iterrows():
                sensor_id = sensor['Id']
                sensor_type = sensor['Type']

                inner_tab_headline = f"{sensor_type}, #{sensor_id}"

                measurement_widget_view_model = MeasurementsWidgetViewModel(MeasurementRepo(SessionLocal), station_id, sensor_id)
                # Erstelle das Messwerte-Widget für diesen Sensor
                measurement_widget = MeasurementsWidget(measurement_widget_view_model)

                # Füge es dem AKTUELLEN inneren Container hinzu
                new_inner_tabs_container.addTab(measurement_widget, inner_tab_headline)

        self.status_label = QLabel()
        self.status_label.setText(view_model.getStatus())
        self.statusBar().addPermanentWidget(self.status_label)

        self.setCentralWidget(self.outer_tabs_container)

    def open_new_window(self):
        if self.database_dialog is None:
            viewmodel = DataBaseDialogViewModel()
            self.database_dialog = DataBaseConfigDialog(viewmodel=viewmodel)

        if self.database_dialog.exec():  # exec() öffnet das Fenster modal und wartet
            print("Daten wurden gespeichert")

