from typing import List
from PySide6.QtCore import Signal
from PySide6.QtWidgets import QWidget, QHBoxLayout, QButtonGroup, QRadioButton

class TimePickerWidget(QWidget):

    timeSpanChanged = Signal(int)

    def __init__(self, radio_button_times: List[int], default_index=1):# parent_view_model):
        super().__init__()

        self.radio_button_times = radio_button_times

        layout = QHBoxLayout(self)
        self.button_group = QButtonGroup(self)

        for i,(text, hours) in enumerate(self.radio_button_times):
            radio_button = QRadioButton(text)
            layout.addWidget(radio_button)
            self.button_group.addButton(radio_button, i)

            if i == default_index:
                radio_button.setChecked(True)

        self.button_group.idClicked.connect(self.handle_clicked)

    def handle_clicked(self, index):

            hours = self.radio_button_times[index][1]
            self.timeSpanChanged.emit(hours)
