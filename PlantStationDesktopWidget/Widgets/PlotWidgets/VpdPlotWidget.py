import mplcursors
import sys
from PySide6.QtWidgets import QWidget, QGridLayout, QLabel, QVBoxLayout
from PySide6.QtGui import QPixmap, QPainter
from PySide6.QtCore import Qt

from matplotlib.backends.backend_qtagg import FigureCanvasQTAgg as FigureCanvas
from matplotlib.figure import Figure
from pathlib import Path
from numpy.ma.core import ceil, floor
from pandas import DataFrame
from pip._internal.utils import datetime

from Widgets.PlotWidgets.PlotWidgetBase import PlotWidgetBase


def resource_path(relative_path: str) -> Path:
    """Gibt den korrekten Pfad zurück – im Dev-Modus und im PyInstaller-Bundle."""
    if hasattr(sys, '_MEINPASS'):
        # PyInstaller entpackt Dateien nach _MEIPASS
        return Path(sys._MEINPASS) / relative_path
    return Path(__file__).resolve().parent.parent.parent / relative_path

class VpdPlotWidget(PlotWidgetBase):
    def __init__(self, parent_view_model):
        super().__init__(parent_view_model)

        self.setAttribute(Qt.WidgetAttribute.WA_StyledBackground, True)

        self.current_theme = 'light'

        # Grid-Layout erlaubt das Stapeln von Widgets in derselben Zelle
        # self.main_layout = QGridLayout(self)
        # self.main_layout.setContentsMargins(0, 0, 0, 0)

        # 1. Matplotlib Setup
        self.fig = Figure(figsize=(5, 6), dpi=100, constrained_layout=True)        # WICHTIG: Hintergrund der Figure transparent machen
        self.fig.patch.set_alpha(0.0)

        self.ax_temperature = self.fig.add_subplot(311)
        self.ax_humidity = self.fig.add_subplot(312)
        self.ax_vpd = self.fig.add_subplot(313)


        # WICHTIG: Hintergrund des Plot-Bereichs transparent machen
        # self.ax.patch.set_alpha(0.0)

        self.canvas = FigureCanvas(self.fig)
        self.canvas.setStyleSheet("background: transparent;")

        self.current_cursor_temperature = None
        self.current_cursor_humidity = None
        self.current_cursor_vpd = None

        self.background_image = QWidget()
        self.setAttribute(Qt.WidgetAttribute.WA_StyledBackground, True)
        # self.background_image.setStyleSheet("background-color: rgb(0, 0, 0); ")

        self.background_layout = QVBoxLayout(self.background_image)

        # 2. Logo Setup (Hintergrund)
        self.logo_label = QLabel()
        self.logo_label.setAlignment(Qt.AlignmentFlag.AlignCenter)

        # Pfad zum Logo
        base_path = Path(__file__).resolve().parent
        self.logo_path_green = resource_path('images/Logo/PlantStationLogo_KreisGreen1000x1000.png')
        self.logo_path_yellow = resource_path('images/Logo/PlantStationLogo_KreisYellow1000x1000.png')
        self.logo_path_red = resource_path('images/Logo/PlantStationLogo_KreisRed1000x1000.png')

        if self.logo_path_green.exists():
            original_pixmap = QPixmap(str(self.logo_path_green))
            # Wir speichern das Pixmap, um es beim Resize-Event proportional zu skalieren
            self.logo_pixmap = original_pixmap
        else:
            self.logo_pixmap = None

        # 3. Widgets im Layout stapeln (Beide in Zeile 0, Spalte 0)
        self.background_layout.addWidget(self.logo_label)
        self.main_layout.addWidget(self.background_image, 0, 0)
        self.main_layout.addWidget(self.canvas, 0, 0)

    def resizeEvent(self, event):
        """Sorgt dafür, dass das Logo proportional mitwächst und den 10% Rand hält."""
        super().resizeEvent(event)
        if self.logo_pixmap:
            # Berechne 80% der verfügbaren Größe (für 10% Rand auf jeder Seite)
            target_width = self.width() * 0.6
            target_height = self.height() * 0.6

            scaled_pixmap = self.logo_pixmap.scaled(
                int(target_width),
                int(target_height),
                Qt.AspectRatioMode.KeepAspectRatio,
                Qt.TransformationMode.SmoothTransformation
            )

            # Transparenz des Pixmaps direkt setzen (0.15 entspricht 15% Deckkraft)
            transparent_pixmap = QPixmap(scaled_pixmap.size())
            transparent_pixmap.fill(Qt.GlobalColor.transparent)
            painter = QPainter(transparent_pixmap)
            painter.setOpacity(0.6)
            painter.drawPixmap(0, 0, scaled_pixmap)
            painter.end()

            self.logo_label.setPixmap(transparent_pixmap)

    def plot(self, temperature_df, humidity_df, vpd_df: DataFrame):

        if (temperature_df is None or temperature_df.empty or
                humidity_df is None or humidity_df.empty or
                vpd_df is None or vpd_df.empty):
            self.canvas.hide()
            self.logo_label.hide()
            # Falls ein "No Data" Label existiert, hier anzeigen
            if hasattr(self, "no_data_in_df_label"):
                self.no_data_in_df_label.show()
            return

        self.canvas.show()
        self.logo_label.show()

        if self.current_cursor_temperature is not None:
            self.current_cursor_temperature.remove()
        if self.current_cursor_humidity is not None:
            self.current_cursor_humidity.remove()
        if self.current_cursor_vpd is not None:
            self.current_cursor_vpd.remove()

        y_max_temperature = ceil(float(temperature_df["Value"].max()) / 10) * 10
        y_min_temperature = floor(float(temperature_df["Value"].min()) / 10) * 10
        y_max_humidity = ceil(float(humidity_df["Value"].max()) / 10) * 10
        y_min_humidity = floor(float(humidity_df["Value"].min()) / 10) * 10
        y_max_vpd = ceil(float(vpd_df["Value"].max())/0.1) *0.1
        y_min_vpd = floor(float(vpd_df["Value"].min())/0.1) *0.1

        color_line_temperature = 'red'
        color_line_humidity = 'blue'
        color_line_vpd = 'green'

        self.ax_temperature.clear()
        self.ax_humidity.clear()
        self.ax_vpd.clear()

        # WICHTIG: Nach clear() muss Transparenz erneut gesetzt werden
        self.ax_temperature.patch.set_alpha(0.0)
        self.ax_humidity.patch.set_alpha(0.0)
        self.ax_vpd.patch.set_alpha(0.0)

        # Plotten der Daten (zorder=2 damit über dem Grid)
        line_temperature = self.ax_temperature.plot(temperature_df['RecordedAt'], temperature_df["Value"],
                                                    color=color_line_temperature, label='Rohdaten', zorder=2)
        line_humidity = self.ax_humidity.plot(humidity_df['RecordedAt'], humidity_df["Value"],
                                                    color=color_line_humidity, label='Rohdaten', zorder=2)
        line_vpd = self.ax_vpd.plot(vpd_df['RecordedAt'], vpd_df["Value"],
                                                    color=color_line_vpd, label='Rohdaten', zorder=2)

        x_limits_temperature = temperature_df['RecordedAt'].min(),temperature_df['RecordedAt'].max()
        x_limits_humidity = humidity_df['RecordedAt'].min(),humidity_df['RecordedAt'].max()
        x_limits_vpd = vpd_df['RecordedAt'].min(),vpd_df['RecordedAt'].max()
        # Achsen-Konfiguration
        self.ax_temperature.set_title(temperature_df["Type"].iloc[0].capitalize())
        self.ax_temperature.set_ylim(y_min_temperature, y_max_temperature)
        self.ax_temperature.set_xlim(x_limits_temperature)
        self.ax_temperature.set_xlabel('Zeit')
        self.ax_temperature.set_ylabel(f"{temperature_df['Type'].iloc[0].capitalize()} [{temperature_df['Unit'].iloc[0]}]")
        self.ax_temperature.grid(True, zorder=0)

        self.ax_humidity.set_title(humidity_df["Type"].iloc[0].capitalize())
        self.ax_humidity.set_ylim(y_min_humidity, y_max_humidity)
        self.ax_humidity.set_xlim(x_limits_humidity)
        self.ax_humidity.set_xlabel('Zeit')
        self.ax_humidity.set_ylabel(f"{humidity_df['Type'].iloc[0].capitalize()} [{humidity_df['Unit'].iloc[0]}]")
        self.ax_humidity.grid(True, zorder=0)

        self.ax_vpd.set_title(vpd_df["Type"].iloc[0].capitalize())
        self.ax_vpd.set_ylim(y_min_vpd, y_max_vpd)
        self.ax_vpd.set_xlim(x_limits_vpd)
        self.ax_vpd.set_xlabel('Zeit')
        self.ax_vpd.set_ylabel(f"{vpd_df['Type'].iloc[0].capitalize()} [{vpd_df['Unit'].iloc[0]}]")
        self.ax_vpd.grid(True, zorder=0)

        # Cursor Setup
        self.current_cursor_temperature = mplcursors.cursor(line_temperature, hover=True, multiple=False)
        self.current_cursor_humidity = mplcursors.cursor(line_humidity, hover=True, multiple=False)
        self.current_cursor_vpd = mplcursors.cursor(line_vpd, hover=True, multiple=False)

        @self.current_cursor_temperature.connect("add")
        def _(sel):
            idx = int(sel.index)
            x_val_temp = temperature_df['RecordedAt'].iloc[idx]
            y_val_temp = sel.target[1]
            sel.annotation.set_text(f"Zeit: {x_val_temp.strftime('%H:%M:%S')}\nWert: {y_val_temp:.2f}")

        @self.current_cursor_humidity.connect("add")
        def _(sel):
            idx = int(sel.index)
            x_val_hum = humidity_df['RecordedAt'].iloc[idx]
            y_val_hum = sel.target[1]
            sel.annotation.set_text(f"Zeit: {x_val_hum.strftime('%H:%M:%S')}\nWert: {y_val_hum:.2f}")

        @self.current_cursor_vpd.connect("add")
        def _(sel):
            idx = int(sel.index)
            x_val_vpd = vpd_df['RecordedAt'].iloc[idx]
            y_val_vpd = sel.target[1]
            sel.annotation.set_text(f"Zeit: {x_val_vpd.strftime('%H:%M:%S')}\nWert: {y_val_vpd:.2f}")

        self.fig.autofmt_xdate()
        self.canvas.draw()

        self.change_theme(self.current_theme)

    def change_logo_color(self, color:str):
        if (color == "green"):
            self.logo_pixmap = QPixmap(str(self.logo_path_green))
        elif(color == "yellow"):
            self.logo_pixmap = QPixmap(str(self.logo_path_yellow))
        elif(color == "red"):
            self.logo_pixmap = QPixmap(str(self.logo_path_red))


    def change_theme(self, theme:str):
        color = "black"

        if theme == "dark":
            color = 'white'

        for spine in self.ax_temperature.spines.values():
            spine.set_color(color)
        for spine in self.ax_humidity.spines.values():
            spine.set_color(color)
        for spine in self.ax_vpd.spines.values():
            spine.set_color(color)

        self.ax_temperature.tick_params(color=color, labelcolor=color, which='both')
        self.ax_temperature.xaxis.label.set_color(color)
        self.ax_temperature.yaxis.label.set_color(color)
        self.ax_temperature.title.set_color(color)

        self.ax_humidity.tick_params(color=color, labelcolor=color, which='both')
        self.ax_humidity.xaxis.label.set_color(color)
        self.ax_humidity.yaxis.label.set_color(color)
        self.ax_humidity.title.set_color(color)

        self.ax_vpd.tick_params(color=color, labelcolor=color, which='both')
        self.ax_vpd.xaxis.label.set_color(color)
        self.ax_vpd.yaxis.label.set_color(color)
        self.ax_vpd.title.set_color(color)

        self.canvas.draw()
