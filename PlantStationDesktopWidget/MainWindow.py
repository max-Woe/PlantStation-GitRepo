from PySide6.QtWidgets import QMainWindow, QTabWidget, QStatusBar, QLabel, QWidget
from PySide6.QtCore import Qt

from DataAcces.Repositories.MeasurementRepo import MeasurementRepo
from DataAcces.Repositories.StationRepo import StationRepo
from ViewModels.MainWindowViewModel import MainWindowViewModel
from ViewModels.MeasurementWidgetViewModel import MeasurementsWidgetViewModel
from Widgets.MeasurementsWidget import MeasurementsWidget


class MainWindow(QMainWindow):
    def __init__(self, view_model:MainWindowViewModel = None):
        super().__init__()
        # self.setWindowFlags(
        #     Qt.Window |  # Definiert es als eigenständiges Fenster
        #     Qt.FramelessWindowHint |  # Entfernt die Titelleiste
        #     Qt.WindowStaysOnBottomHint |  # Hält es hinter anderen Fenstern (auf dem Desktop)
        #     Qt.SubWindow  # Verhindert meist den Eintrag in der Taskleiste
        # )
        self.setAttribute(Qt.WA_ShowWithoutActivating)
        # self.setWindowOpacity(0.8)
        self.setWindowTitle("PlantStation - Messwertübersicht")
        self.view_model = view_model

        self.outer_tabs_container = QTabWidget()
        self.inner_tabs_container = QTabWidget()

        for station in self.view_model._list_of_station_and_sensor_ids_as_tuples:
            station_id = station[0]
            sensor_ids = station[1]

            tab_headline = f"Station {station_id}"

            # 1. Erzeuge für JEDE Station ein EIGENES TabWidget für die Sensoren
            new_inner_tabs_container = QTabWidget()

            # 2. Füge diesen NEUEN Container dem äußeren TabWidget hinzu
            self.outer_tabs_container.addTab(new_inner_tabs_container, tab_headline)

            # 3. Fülle den NEUEN Container mit den Sensoren dieser Station
            for sensor_id in sensor_ids:
                inner_tab_headline = f"Sensor {sensor_id}"

                measurement_widget_view_model = MeasurementsWidgetViewModel(MeasurementRepo(), station_id, sensor_id)
                # Erstelle das Messwerte-Widget für diesen Sensor
                measurement_widget = MeasurementsWidget(measurement_widget_view_model)

                # Füge es dem AKTUELLEN inneren Container hinzu
                new_inner_tabs_container.addTab(measurement_widget, inner_tab_headline)

        self.status_label = QLabel()
        self.status_label.setText(view_model.getStatus())
        self.statusBar().addPermanentWidget(self.status_label)

        self.setCentralWidget(self.outer_tabs_container)

