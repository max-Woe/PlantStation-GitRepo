from PySide6.QtWidgets import QWidget, QHBoxLayout

from matplotlib.backends.backend_qtagg import FigureCanvasQTAgg as FigureCanvas
from matplotlib.figure import Figure
import matplotlib.image as mpimg
from pathlib import Path
import mplcursors
from numpy.ma.core import ceil, floor
from pandas import DataFrame
from datetime import datetime


class PlotWidget(QWidget):
    def __init__(self, parent_view_model):
        super().__init__()

        self.layout = QHBoxLayout(self)
        self.fig = Figure(figsize=(5, 2), dpi=100)
        self.ax = self.fig.add_subplot(111)
        self.logo_ax = self.fig.add_axes((0.75, 0.75, 0.15, 0.35), anchor='NE', zorder=1)
        self.canvas = FigureCanvas(self.fig)
        self.layout.addWidget(self.canvas)

    def plot(self, measurements_df: DataFrame):
        if measurements_df is None or measurements_df.empty:
            # 1. Sicherstellen, dass die Widgets existieren, bevor sie entfernt werden
            for widget_name in ["canvas", "grid_layout_containter"]:
                widget = getattr(self, widget_name, None)
                if widget is not None:
                    self.layout.removeWidget(widget)
                    widget.hide()  # WICHTIG: removeWidget versteckt das Widget nicht automatisch!

            # 2. Das Label anzeigen
            if hasattr(self, "no_data_in_df_label"):
                self.layout.insertWidget(max(0, self.layout.count() - 1), self.no_data_in_df_label)
                self.no_data_in_df_label.show()

            return


        y_max = ceil(measurements_df["Value"].max() / 10) * 10
        y_min = floor(measurements_df["Value"].min() / 10) * 10

        color = 'black'

        match measurements_df["Type"].iloc[0]:
            case "temperature":
                color = 'red'
            case "humidity":
                color = 'blue'
            case "soil_moisture":
                color = 'brown'

        self.ax.clear()

        line = self.ax.plot(measurements_df['RecordedAt'], measurements_df["Value"], color=color, label='Rohdaten')

        base_path = Path(__file__).resolve().parent
        bild_pfad = base_path.parent.parent / 'images' / 'Logo' / 'PlantStationLogo_KreisGreen1000x1000.png'

        logo = mpimg.imread(str(bild_pfad))
        self.logo_ax.clear()  # Verhindert Überlagerung bei mehrmaligem Plotten
        self.logo_ax.imshow(logo)
        self.logo_ax.axis('off')

        self.ax.set_title(measurements_df["Type"].iloc[0].capitalize())
        self.ax.set_ylim(y_min, y_max)
        self.ax.set_xlabel('Zeit')
        self.ax.set_ylabel(f"{measurements_df["Type"].iloc[0].capitalize()} [{measurements_df["Unit"].iloc[0]}]")
        self.ax.grid(True)
        cursor = mplcursors.cursor(line, hover=True)
        self.fig.autofmt_xdate()
        self.canvas.draw()

        @cursor.connect("add")
        def _(sel):
            # Den Index explizit in einen Integer umwandeln
            idx = int(sel.index)

            x_val = measurements_df['RecordedAt'].iloc[idx]
            y_val = sel.target[1]

            sel.annotation.set_text(f"Zeit: {x_val.strftime('%H:%M:%S')}\nWert: {y_val:.2f}")

