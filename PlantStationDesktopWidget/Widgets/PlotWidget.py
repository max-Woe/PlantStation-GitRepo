from PySide6.QtWidgets import QWidget, QHBoxLayout

from matplotlib.backends.backend_qtagg import FigureCanvasQTAgg as FigureCanvas
from matplotlib.figure import Figure
from matplotlib.image import BboxImage
from matplotlib.offsetbox import OffsetImage, AnnotationBbox
from matplotlib.transforms import Bbox, TransformedBbox
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
        # self.logo_ax = self.fig.add_axes((0.75, 0.75, 0.15, 0.35), anchor='NE', zorder=1)
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

        # 1. Logo laden
        base_path = Path(__file__).resolve().parent
        bild_pfad = base_path.parent.parent / 'images' / 'Logo' / 'PlantStationLogo_KreisGreen1000x1000.png'
        logo = mpimg.imread(str(bild_pfad))

        # 2. Seitenverhältnis des Logos berechnen
        h, w = logo.shape[:2]
        img_aspect = w / h  # Bei deinem Logo 1.0

        # 3. Das Aspektverhältnis der Achsen (Plotfläche) holen
        # Wir brauchen das Verhältnis von Breite zu Höhe der Achse in Pixeln
        bbox = self.ax.get_window_extent().transformed(self.fig.dpi_scale_trans.inverted())
        ax_aspect = bbox.width / bbox.height

        # 4. Breite der Box so berechnen, dass die Höhe immer 1.0 (100%) ist
        # x_range bestimmt, wie breit das Logo im Verhältnis zur Achsenbreite ist
        x_range = img_aspect / ax_aspect
        x0 = 0.5 - (x_range / 2)
        x1 = 0.5 + (x_range / 2)

        # Faktor für 10% Rand oben und 10% Rand unten (bleiben 80% Höhe)
        scale = 0.8
        offset = (1.0 - scale) / 2  # Das ergibt 0.1

        # Die vertikalen Grenzen sind dann: offset bis (1.0 - offset)
        # Die horizontalen Grenzen müssen proportional mit schrumpfen:
        x_width = x1 - x0
        new_x0 = x0 + (x_width * offset)
        new_x1 = x1 - (x_width * offset)

        # 5. BboxImage erstellen
        # Die Box ist vertikal fest auf 0 bis 1 (100% Höhe)
        logo_bbox = TransformedBbox(Bbox([[new_x0, offset], [new_x1, 1-offset]]), self.ax.transAxes)
        img_artist = BboxImage(logo_bbox, zorder=-1, alpha=0.15)
        img_artist.set_data(logo)

        # Clipping aktivieren, damit absolut nichts übersteht
        img_artist.set_clip_on(True)
        img_artist.set_clip_box(self.ax.bbox)

        self.ax.add_artist(img_artist)

        # 3. Daten plotten
        line = self.ax.plot(measurements_df['RecordedAt'], measurements_df["Value"],
                            color=color, label='Rohdaten', zorder=2)


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

