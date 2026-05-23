import sys
from PySide6.QtWidgets import QApplication
from DataAcces.Repositories.StationRepo import StationRepo
from DataAcces.Repositories.SensorRepo import SensorRepo
from MainWindow import MainWindow
from ViewModels.MainWindowViewModel import MainWindowViewModel

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