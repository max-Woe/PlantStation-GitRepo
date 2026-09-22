from datetime import datetime, timezone, timedelta
from typing import List, cast

from PySide6.QtCore import Signal, Qt, QDateTime
from PySide6.QtWidgets import QWidget, QHBoxLayout, QButtonGroup, QRadioButton, QLabel, QDateTimeEdit


class TimePickerWidget(QWidget):

    timeSpanChanged = Signal(datetime, datetime)

    def __init__(self, radio_button_times: List[tuple[str,int]], default_index=1):# parent_view_model):
        super().__init__()

        self.setAttribute(Qt.WidgetAttribute.WA_StyledBackground, True)

        self.radio_button_times = radio_button_times

        layout = QHBoxLayout(self)
        self.button_group = QButtonGroup(self)

        for i,(text, hours) in enumerate(self.radio_button_times):
            radio_button = QRadioButton(text)
            layout.addWidget(radio_button)
            self.button_group.addButton(radio_button, i)

            if i == default_index:
                radio_button.setChecked(True)

        radio_button_own_timespan = QRadioButton('Eigener Zeitraum')

        self.timespan_begin_label = QLabel('Beginn:')
        self.timespan_begin_textfield = QDateTimeEdit(self)
        self.timespan_begin_textfield.setDateTime(QDateTime.currentDateTimeUtc())
        self.timespan_begin_textfield.setCalendarPopup(True)
        self.timespan_end_label = QLabel('Ende:')
        self.timespan_end_textfield = QDateTimeEdit(self)
        self.timespan_end_textfield.setDateTime(QDateTime.currentDateTimeUtc())
        self.timespan_end_textfield.setCalendarPopup(True)

        timespan_layout = QHBoxLayout()
        timespan_layout.addWidget(radio_button_own_timespan)
        timespan_layout.addWidget(self.timespan_begin_label)
        timespan_layout.addWidget(self.timespan_begin_textfield)
        timespan_layout.addWidget(self.timespan_end_label)
        timespan_layout.addWidget(self.timespan_end_textfield)

        self.button_group.addButton(radio_button_own_timespan, len(radio_button_times))

        layout.addLayout(timespan_layout)
        self.button_group.idClicked.connect(self.handle_clicked)

    def handle_clicked(self, index):
        count_of_buttons = len(self.radio_button_times)

        if index== count_of_buttons:
            since = cast(datetime, self.timespan_begin_textfield.dateTime().toPython())
            until = cast(datetime, self.timespan_end_textfield.dateTime().toPython())
        else:
            hours = self.radio_button_times[index][1]
            since = datetime.now(timezone.utc)-timedelta(hours=hours)
            until = datetime.now(timezone.utc)

        if until > since:
            self.timeSpanChanged.emit(since, until)
