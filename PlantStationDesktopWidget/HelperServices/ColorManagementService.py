

class ColorManagementService:
    def __init__(self, temp_limits = (12, 18, 24, 28), humidity_limits = (30, 50, 70, 80), soil_moisture_limits = (20, 40, 70, 85)):
        self.temp_limits = temp_limits
        self.humidity_limits = humidity_limits
        self.soil_moisture_limits = soil_moisture_limits

        self.current_limits = None

    def set_current_limits(self, type):
        if type == "temperature":
            self.current_limits = self.temp_limits
        elif type == "humidity":
            self.current_limits = self.humidity_limits
        elif type == "soil_moisture":
            self.current_limits = self.soil_moisture_limits

    def get_logo_color(self, value):
        lower_yellow, lower_green, upper_green, upper_yellow = self.current_limits
        if lower_green < value < upper_green:
            return "green"
        elif lower_yellow <= value <= upper_yellow:
            return "yellow"
        else:
            return "red"