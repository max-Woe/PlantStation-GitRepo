from PySide6.QtCore import QObject, Signal

class DataBaseDialogViewModel(QObject):

    valuesChanged = Signal()

    def __init__(self, host = "123.123.123.123", port = "1234", database = "databasename", password = "passwort1234"):
        super().__init__()

        self._host = host
        self._port = port
        self._database = database
        self._password = password

    @property
    def host(self) -> str:
        return self._host
    @host.setter
    def host(self, value: str):
        if value!=self._host:
            self._host = value

            self.valuesChanged.emit()

    @property
    def port(self) -> str:
        return self._port
    @port.setter
    def port(self, value: str):
        if value!=self._port:
            self._port = value

            self.valuesChanged.emit()

    @property
    def database(self) -> str:
        return self._database
    @database.setter
    def database(self, value: str):
        if value!=self._database:
            self._database = value

            self.valuesChanged.emit()

    @property
    def password(self) -> str:
        return self._password
    @password.setter
    def password(self, value: str):
        if value!=self._password:
            self._password = value

            self.valuesChanged.emit()

    def set_host(self, value: str):
        self.host = value

    def set_port(self, value: str):
        self.port = value

    def set_database(self, value: str):
        self.database = value

    def set_password(self, value: str):
        self.password = value