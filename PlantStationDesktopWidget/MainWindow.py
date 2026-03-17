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

                # Erstelle das Messwerte-Widget für diesen Sensor
                measurement_widget = MeasurementsWidget(
                    MeasurementsWidgetViewModel(MeasurementRepo(), sensor_id)
                )

                # Füge es dem AKTUELLEN inneren Container hinzu
                new_inner_tabs_container.addTab(measurement_widget, inner_tab_headline)
        # for station in self.view_model._list_of_station_and_sensor_ids_as_tuples:
        #     tab_headline = f"Station {station[0]}"
        #     outer_tab = self.outer_tabs_container.addTab(self.inner_tabs_container, tab_headline)
        #     # outer_tab_widget = self.outer_tabs_container.addTab(
        #     #     MeasurementsWidget(MeasurementsWidgetViewModel(MeasurementRepo(), StationRepo())), tab_headline)
        #     for sensor_id in station[1]:
        #         inner_tab_headline = f"Sensor {sensor_id}"
        #         inner_tab_widget = self.inner_tabs_container.addTab(MeasurementsWidget(MeasurementsWidgetViewModel(MeasurementRepo(), StationRepo())), inner_tab_headline)



        # self.tabs_container.addTab(
        #     MeasurementsWidget(MeasurementsWidgetViewModel(MeasurementRepo(), StationRepo())), 'Measurements2')
        # self.measurements = MeasurementsWidget()

        self.status_label = QLabel()
        self.status_label.setText(view_model.getStatus())
        self.statusBar().addPermanentWidget(self.status_label)

        self.setCentralWidget(self.outer_tabs_container)

#
# from PySide6.QtWidgets import QVBoxLayout, QTabWidget, QLabel, QWidget
# from PySide6.QtCore import Qt
#
# # Deine Repositories und ViewModels
# from DataAcces.Repositories.MeasurementRepo import MeasurementRepo
# from DataAcces.Repositories.StationRepo import StationRepo
# from ViewModels.MainWindowViewModel import MainWindowViewModel
# from ViewModels.MeasurementWidgetViewModel import MeasurementsWidgetViewModel
# from Widgets.MeasurementsWidget import MeasurementsWidget
#
#
# class MainWindow(QWidget):
#     def __init__(self, view_model: MainWindowViewModel = None, parent=None):
#         # Wichtig: parent an super() übergeben, damit das Widget im Container bleibt
#         super().__init__(parent)
#         self.setMinimumSize(400, 500)
#         self.resize(600, 400)
#
#         # Test-Hintergrundfarbe (kann später wieder raus)
#         self.setStyleSheet("background-color: white; color: black;")
#         self.view_model = view_model
#
#         # Haupt-Layout für dieses Widget
#         # Wir nutzen ein vertikales Layout, um Tabs und Statuszeile zu stapeln
#         self.main_layout = QVBoxLayout(self)
#         self.main_layout.setContentsMargins(0, 0, 0, 0)
#         self.main_layout.setSpacing(0)
#
#         # Container für die Stationen (Äußere Tabs)
#         self.outer_tabs_container = QTabWidget()
#
#         # Iteration über die Datenstruktur des ViewModels
#         if self.view_model and self.view_model._list_of_station_and_sensor_ids_as_tuples:
#             print(f"DEBUG: {len(self.view_model._list_of_station_and_sensor_ids_as_tuples)} Stationen gefunden.")
#
#             for station_data in self.view_model._list_of_station_and_sensor_ids_as_tuples:
#                 station_id = station_data[0]
#                 sensor_ids = station_data[1]
#
#                 tab_headline = f"Station {station_id}"
#
#                 # Für jede Station ein eigener Tab-Container für deren Sensoren
#                 new_inner_tabs_container = QTabWidget()
#                 self.outer_tabs_container.addTab(new_inner_tabs_container, tab_headline)
#
#                 # Sensoren der jeweiligen Station hinzufügen
#                 for sensor_id in sensor_ids:
#                     inner_tab_headline = f"Sensor {sensor_id}"
#
#                     # Erstellung des Inhalts-Widgets für den Sensor
#                     measurement_widget = MeasurementsWidget(
#                         MeasurementsWidgetViewModel(MeasurementRepo(), sensor_id)
#                     )
#
#                     new_inner_tabs_container.addTab(measurement_widget, inner_tab_headline)
#         else:
#             print("DEBUG: Keine Stationen im ViewModel gefunden!")
#             # Optional: Zeige eine Nachricht direkt im Widget an
#             error_label = QLabel("Keine Daten geladen. Bitte Verbindung/Secrets prüfen.")
#             self.main_layout.addWidget(error_label)
#
#         # Tabs zum Hauptlayout hinzufügen (ersetzt setCentralWidget)
#         self.main_layout.addWidget(self.outer_tabs_container)
#
#         # Statuszeile manuell als QLabel am unteren Rand hinzufügen
#         self.status_label = QLabel()
#         if self.view_model:
#             self.status_label.setText(self.view_model.getStatus())
#
#         # Ein bisschen Styling, damit es sich vom Inhalt abhebt
#         self.status_label.setStyleSheet("""
#             QLabel {
#                 padding: 4px;
#                 background-color: rgba(0, 0, 0, 0.05);
#                 border-top: 1px solid rgba(255, 255, 255, 0.1);
#                 font-size: 10px;
#             }
#         """)
#         self.status_label.setAlignment(Qt.AlignLeft | Qt.AlignVCenter)
#
#         self.main_layout.addWidget(self.status_label)