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
