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


def resource_path(relative_path: str) -> Path:
    """Gibt den korrekten Pfad zurück – im Dev-Modus und im PyInstaller-Bundle."""
    if hasattr(sys, '_MEINPASS'):
        # PyInstaller entpackt Dateien nach _MEIPASS
        return Path(sys._MEINPASS) / relative_path
    return Path(__file__).resolve().parent.parent.parent / relative_path

class PlotWidgetBase(QWidget):
    def __init__(self, parent_view_model):
        super().__init__()

        self.setAttribute(Qt.WidgetAttribute.WA_StyledBackground, True)

        self.current_theme = 'light'
        self.logo_pixmap = None

        # Grid-Layout erlaubt das Stapeln von Widgets in derselben Zelle
        self.main_layout = QGridLayout(self)
        self.main_layout.setContentsMargins(0, 0, 0, 0)


    def resizeEvent(self, event):
        super().resizeEvent(event)
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

    def plot(self, *args, **kwargs):
        pass
        # if measurements_df is None or measurements_df.empty:
        #     self.canvas.hide()
        #     self.logo_label.hide()
        #     # Falls ein "No Data" Label existiert, hier anzeigen
        #     if hasattr(self, "no_data_in_df_label"):
        #         self.no_data_in_df_label.show()
        #     return
        #
        # self.canvas.show()
        # self.logo_label.show()
        #
        # if self.current_cursor is not None:
        #     self.current_cursor.remove()
        #
        # y_max = ceil(float(measurements_df["Value"].max()) / 10) * 10
        # y_min = floor(float(measurements_df["Value"].min()) / 10) * 10
        #
        # color_line = 'black'
        # match measurements_df["Type"].iloc[0]:
        #     case "temperature":
        #         color_line = 'red'
        #     case "humidity":
        #         color_line = 'blue'
        #     case "soil_moisture":
        #         color_line = 'brown'
        #
        # self.ax.clear()
        #
        # # WICHTIG: Nach clear() muss Transparenz erneut gesetzt werden
        # self.ax.patch.set_alpha(0.0)
        #
        # # Plotten der Daten (zorder=2 damit über dem Grid)
        # line = self.ax.plot(measurements_df['RecordedAt'], measurements_df["Value"],
        #                     color=color_line, label='Rohdaten', zorder=2)
        #
        # x_limits = measurements_df['RecordedAt'].min(),measurements_df['RecordedAt'].max()
        # # Achsen-Konfiguration
        # self.ax.set_title(measurements_df["Type"].iloc[0].capitalize())
        # self.ax.set_ylim(y_min, y_max)
        # self.ax.set_xlim(x_limits)
        # self.ax.set_xlabel('Zeit')
        # self.ax.set_ylabel(f"{measurements_df['Type'].iloc[0].capitalize()} [{measurements_df['Unit'].iloc[0]}]")
        # self.ax.grid(True, zorder=0)
        #
        # # Cursor Setup
        # self.current_cursor = mplcursors.cursor(line, hover=True, multiple=False)
        #
        # @self.current_cursor.connect("add")
        # def _(sel):
        #     idx = int(sel.index)
        #     x_val = measurements_df['RecordedAt'].iloc[idx]
        #     y_val = sel.target[1]
        #     sel.annotation.set_text(f"Zeit: {x_val.strftime('%H:%M:%S')}\nWert: {y_val:.2f}")
        #
        # self.fig.autofmt_xdate()
        # self.canvas.draw()
        #
        # self.change_theme(self.current_theme)


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

        for spine in self.ax.spines.values():
            spine.set_color(color)

        self.ax.tick_params(color=color, labelcolor=color, which='both')
        self.ax.xaxis.label.set_color(color)
        self.ax.yaxis.label.set_color(color)
        self.ax.title.set_color(color)

        self.canvas.draw()
