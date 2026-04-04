from PySide6.QtCore import Signal
from PySide6.QtWidgets import QWidget, QPushButton, QVBoxLayout
from datetime import datetime, timedelta, timezone

class RefreshWidget(QWidget):
    button_clicked = Signal()

    def __init__(self, parent_view_model):
        super().__init__()

        self._view_model = parent_view_model
        layout = QVBoxLayout(self)

        refresh_button = QPushButton("Refresh")
        refresh_button.clicked.connect(self.refresh_data)

        layout.addWidget(refresh_button)

    def refresh_data(self):
        self.button_clicked.emit()
