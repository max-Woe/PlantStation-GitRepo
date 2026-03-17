# import os
# import sys
#
# # Den Pfad zum 'code'-Ordner feststellen
# basedir = os.path.dirname(os.path.realpath(__file__))
#
# # Alle Unterordner für Python auffindbar machen
# if basedir not in sys.path:
#     sys.path.insert(0, basedir)
#
# # Arbeitsverzeichnis wechseln, damit secrets.json gefunden wird
# os.chdir(basedir)
#
# import sys
#
# from PySide6.QtWidgets import QApplication, QMainWindow
#
# from DataAcces.Repositories.StationRepo import StationRepo
# from DataAcces.Repositories.SensorRepo import SensorRepo
# from MainWindow import MainWindow
# from ViewModels.MainWindowViewModel import MainWindowViewModel
#
# app = QApplication(sys.argv)
#
# station_repo = StationRepo()
# sensor_repo = SensorRepo()
# mainWindowViewModel = MainWindowViewModel(station_repo = station_repo, sensor_repo = sensor_repo)
#
# window = MainWindow(view_model=mainWindowViewModel)
# window.show()
#
# app.exec()



import os
import sys
from PySide6.QtWidgets import QApplication

# Hilfsfunktion für PyInstaller-Pfade
def resource_path(relative_path):
    """ Erstellt den absoluten Pfad zur Ressource, passend für Entwicklung und PyInstaller """
    if hasattr(sys, '_MEIPASS'):
        return os.path.join(sys._MEIPASS, relative_path)
    return os.path.join(os.path.abspath("."), relative_path)

# Den Pfad zum 'code'-Ordner feststellen und Arbeitsverzeichnis setzen
basedir = resource_path(".")
if basedir not in sys.path:
    sys.path.insert(0, basedir)

os.chdir(basedir)

from DataAcces.Repositories.StationRepo import StationRepo
from DataAcces.Repositories.SensorRepo import SensorRepo
from MainWindow import MainWindow
from ViewModels.MainWindowViewModel import MainWindowViewModel

def main():
    app = QApplication(sys.argv)

    station_repo = StationRepo()
    sensor_repo = SensorRepo()
    mainWindowViewModel = MainWindowViewModel(station_repo=station_repo, sensor_repo=sensor_repo)

    window = MainWindow(view_model=mainWindowViewModel)
    window.show()

    sys.exit(app.exec())

if __name__ == "__main__":
    main()