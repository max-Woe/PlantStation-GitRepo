# from json.decoder import NaN
from HelperServices import GrowAnalyticsService
import pandas as pd
from PySide6.QtWidgets import QWidget, QGridLayout, QLabel
from PySide6.QtCore import Qt
from pandas import DataFrame


class InfoWidgetBase(QWidget):
    def __init__(self, station_id: int):
        super().__init__()
        self.station_id = station_id

        self._labels_initialized = False

        self.grid_layout = QGridLayout(self)

        self.station_label = QLabel("#id")
        self.station_label.setObjectName("StationId")
        self.current_time_label = QLabel("")
        self.unit_label = QLabel("#unit")
        self.type_label = QLabel("#type")


        self.setMaximumHeight(100)


    def init_labels(self):
        pass

    def set_info_labels(self, *args, **kwargs):
        pass

    def update_labels(self, *args, **kwargs):
        pass