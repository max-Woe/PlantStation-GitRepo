import sys
from PySide6.QtWidgets import QApplication
from DataAcces.Repositories.StationRepo import StationRepo
from DataAcces.Repositories.SensorRepo import SensorRepo
from MainWindow import MainWindow
from ViewModels.MainWindowViewModel import MainWindowViewModel
from pathlib import Path


def resource_path(relative_path):
    if hasattr(sys, '_MEIPASS'):
        return Path(sys._MEIPASS) / relative_path
    return Path(__file__).parent / relative_path


def main():
    app = QApplication(sys.argv)

    station_repo = StationRepo()
    sensor_repo = SensorRepo()
    main_window_view_model = MainWindowViewModel(station_repo=station_repo, sensor_repo=sensor_repo)

    with open(resource_path("StyleSheets/styles.css"), "r") as file:
        app.setStyleSheet(file.read())

    window = MainWindow(view_model=main_window_view_model)
    window.show()
    window.raise_()
    window.activateWindow()

    sys.exit(app.exec())

if __name__ == "__main__":
    main()