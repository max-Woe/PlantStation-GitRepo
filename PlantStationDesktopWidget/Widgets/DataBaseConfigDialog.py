from PySide6.QtWidgets import QDialog, QLabel, QVBoxLayout, QHBoxLayout, QLineEdit, QPushButton
from PySide6.QtCore import Qt
from ViewModels.DataBaseDialogViewModel import DataBaseDialogViewModel
from HelperServices import DatabaseValidationService

class DataBaseConfigDialog(QDialog):
    def __init__(self, viewmodel: DataBaseDialogViewModel):
        super().__init__()

        self.viewmodel = viewmodel

        self.viewmodel.valuesChanged.connect(self.update_ui)

        # ------------------------------------------------WINDOW- SETTINGS------------------------------------------------#
        self.setWindowTitle("Datenbank-Verbindung anpassen")
        self.resize(500, 10)
        self.setFixedSize(500,150)



        # ------------------------------------------------LAYOUTS------------------------------------------------#
        self.main_layout = QVBoxLayout(self)

        self.input_layout = QHBoxLayout()
        self.main_layout.addLayout(self.input_layout)

        self.button_layout = QHBoxLayout()
        self.button_layout.setAlignment(Qt.AlignmentFlag.AlignCenter)
        self.main_layout.addLayout(self.button_layout)

        self.input_left_side_layout = QVBoxLayout()
        self.input_layout.addLayout(self.input_left_side_layout)

        self.input_rigth_side_layout = QVBoxLayout()
        self.input_layout.addLayout(self.input_rigth_side_layout)

        self.host_port_layout = QHBoxLayout()
        self.input_rigth_side_layout.addLayout(self.host_port_layout)



        #------------------------------------------------CONTENT------------------------------------------------#
        self.host_label = QLabel("Host (IP/URL):")
        self.host_label.setMargin(3)
        self.host_label.setAlignment(Qt.AlignmentFlag.AlignRight)
        self.input_left_side_layout.addWidget(self.host_label)

        self.host_textEdit = QLineEdit(self.viewmodel.host)
        self.host_textEdit.editingFinished.connect(lambda: self.viewmodel.set_host(self.host_textEdit.text()))
        self.host_port_layout.addWidget(self.host_textEdit)


        self.port_label = QLabel("Port:")
        self.port_label.setMargin(3)
        self.port_label.setAlignment(Qt.AlignmentFlag.AlignRight)
        self.host_port_layout.addWidget(self.port_label)

        self.port_textEdit = QLineEdit(str(self.viewmodel.port))
        self.port_textEdit.setStyleSheet("max-width: 50px")
        self.port_textEdit.editingFinished.connect(lambda: self.viewmodel.set_port(self.port_textEdit.text()))
        self.host_port_layout.addWidget(self.port_textEdit)


        self.database_label = QLabel("Datenbankname:")
        self.database_label.setMargin(3)
        self.database_label.setAlignment(Qt.AlignmentFlag.AlignRight)
        self.input_left_side_layout.addWidget(self.database_label)

        self.database_textEdit = QLineEdit(self.viewmodel.database)
        self.database_textEdit.editingFinished.connect(lambda: self.viewmodel.set_database(self.database_textEdit.text()))
        self.input_rigth_side_layout.addWidget(self.database_textEdit)


        self.password_label = QLabel("Password:")
        self.password_label.setMargin(3)
        self.password_label.setAlignment(Qt.AlignmentFlag.AlignRight)
        self.input_left_side_layout.addWidget(self.password_label)

        self.password_textEdit = QLineEdit(self.viewmodel.password)
        self.password_textEdit.textEdited.connect(self._on_password_typing)
        self.password_textEdit.editingFinished.connect(lambda: self.viewmodel.set_password(self.password_textEdit.text()))
        self.input_rigth_side_layout.addWidget(self.password_textEdit)


        self.accept_button = QPushButton("Accept")
        self.accept_button.clicked.connect(self.accept)
        self.button_layout.addWidget(self.accept_button)

        self.cancel_button = QPushButton("Cancel")
        self.cancel_button.clicked.connect(self.reject)
        self.button_layout.addWidget(self.cancel_button)

    def update_ui(self):
        # Liste aller Paare von (Widget, ViewModel-Wert)
        mapping = [
            (self.host_textEdit,
             self.viewmodel.host,
             DatabaseValidationService.validate_host),
            (self.port_textEdit,
             str(self.viewmodel.port),
             DatabaseValidationService.validate_port),
            (self.database_textEdit,
             self.viewmodel.database,
             DatabaseValidationService.validate_database),
            (self.password_textEdit,
             self.viewmodel.password, 
             DatabaseValidationService.validate_password),
        ]

        for widget, value, validate_func in mapping:
            widget.blockSignals(True)

            if not validate_func(value):
                widget.setText("ungültig")
                widget.setStyleSheet("color: red;")

                if widget == self.password_textEdit:
                    widget.setEchoMode(QLineEdit.EchoMode.Normal)
            else:
                widget.setText(value)
                widget.setStyleSheet("")

                if widget == self.password_textEdit:
                    widget.setEchoMode(QLineEdit.EchoMode.Password)

            widget.blockSignals(False)

    def _on_password_typing(self):
        if self.password_textEdit.echoMode() != QLineEdit.EchoMode.Password:
            self.password_textEdit.setEchoMode(QLineEdit.EchoMode.Password)