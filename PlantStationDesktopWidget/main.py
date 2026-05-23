import os
from pathlib import Path
import sys
import logging
from PySide6.QtWidgets import QApplication
from DataAcces.Repositories.StationRepo import StationRepo
from DataAcces.Repositories.SensorRepo import SensorRepo
from MainWindow import MainWindow
from ViewModels.MainWindowViewModel import MainWindowViewModel
#
# # Hilfsfunktion für PyInstaller-Pfade
# def resource_path(relative_path):
#     """ Erstellt den absoluten Pfad zur Ressource, passend für Entwicklung und PyInstaller """
#     if hasattr(sys, '_MEIPASS'):
#         return os.path.join(sys._MEIPASS, relative_path)
#     return os.path.join(os.path.abspath("."), relative_path)
#
# # Den Pfad zum 'code'-Ordner feststellen und Arbeitsverzeichnis setzen
# basedir = resource_path(".")
# if basedir not in sys.path:
#     sys.path.insert(0, str(basedir))
#
# os.chdir(basedir)

Path("logs").mkdir(exist_ok=True)

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s - %(name)s - %(levelname)s - %(message)s",
    handlers=[
        logging.StreamHandler(),                        # Konsole
        logging.FileHandler("logs/PlantStationDesktopWidget.log"),            # Datei
    ]
)

def main():
    app = QApplication(sys.argv)

    station_repo = StationRepo()
    sensor_repo = SensorRepo()
    main_window_view_model = MainWindowViewModel(station_repo=station_repo, sensor_repo=sensor_repo)

    with open("StyleSheets/styles.css", "r") as file:
        app.setStyleSheet(file.read())

    window = MainWindow(view_model=main_window_view_model)
    window.show()
    window.raise_()
    window.activateWindow()

    sys.exit(app.exec())

if __name__ == "__main__":
    main()