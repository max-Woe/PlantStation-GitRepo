using DataAccess.Models;
using PlantStationHelperService;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using WPFClient.DelegateCommands;
using WPFClient.Models;
using Brush = System.Windows.Media.Brush;
using Colors = System.Windows.Media.Colors;

namespace WPFClient.ViewModels
{
    /// <summary>
    /// Defines the time spans available for data visualization, measured in minutes.
    /// </summary>
    public enum TimeSpans
    {
        hour = 60,
        threeHours = 180,
        halfDay = 720,
        day = 1440,
        week = 7 * day,
        month = 31*day,
        year = 365*day
    }

    /// <summary>
    /// ViewModel for the main window of the PlantStationOverView.
    /// It handles data binding, control of UI elements, loading measurement data, and updating the plot.
    /// Implements <see cref="INotifyPropertyChanged"/> for data binding.
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Properties, Commands und Events

        private readonly ApiClient _apiClient = new ApiClient();

        private DispatcherTimer _refreshTimer;

        /// <summary>
        /// Gets or sets the title of the main window.
        /// </summary>
        public string Title { get; set; } = "PlantStation Overview";

        // Properties of the status bar at the bottom of the main window
        private Brush _statusUpdateColor;
        private string _statusUpdateTime = "DD:MM:YYYY hh:mm:ss";
        private string _statusUpdateStationId = "id";
        private string _statusUpdateSensorId = "id";
        private string _statusUpdateMeasurementValue = "0°C";
        /// <summary>
        /// Gets or sets the background color for the status bar.
        /// Raises <see cref="PropertyChanged"/>.
        /// </summary>
        public Brush StatusColor
        {
            get
            {
                return _statusUpdateColor;
            }
            set
            {
                if (value != _statusUpdateColor)
                {
                    _statusUpdateColor = value;
                    OnPropertyChanged(nameof(StatusColor));
                }
            }
        }
        /// <summary>
        /// Gets or sets the timestamp of the last successful update.
        /// Raises <see cref="PropertyChanged"/>.
        /// </summary>
        public string StatusUpdateTime
        {
            get
            {
                return _statusUpdateTime;
            }
            set
            {
                if (value != _statusUpdateTime)
                {
                    _statusUpdateTime = value;
                    OnPropertyChanged(nameof(StatusUpdateTime));
                }
            }
        }
        /// <summary>
        /// Gets or sets the ID of the currently selected station for the status bar.
        /// Raises <see cref="PropertyChanged"/>.
        /// </summary>
        public string StatusUpdateStationId
        {
            get
            {
                return _statusUpdateStationId;
            }
            set
            {
                if (value != _statusUpdateStationId)
                {
                    _statusUpdateStationId = value;
                    OnPropertyChanged(nameof(StatusUpdateStationId));
                }
            }
        }
        /// <summary>
        /// Gets or sets the ID of the currently selected sensor for the status bar.
        /// Raises <see cref="PropertyChanged"/>.
        /// </summary>
        public string StatusUpdateSensorId
        {
            get
            {
                return _statusUpdateSensorId;
            }
            set
            {
                if (value != _statusUpdateSensorId)
                {
                    _statusUpdateSensorId = value;
                    OnPropertyChanged(nameof(StatusUpdateSensorId));
                }
            }
        }
        /// <summary>
        /// Gets or sets the value and unit of the latest measurement for the status bar.
        /// Raises <see cref="PropertyChanged"/>.
        /// </summary>
        public string StatusUpdateMeasurementValue
        {
            get
            {
                return _statusUpdateMeasurementValue;
            }
            set
            {
                if (value != _statusUpdateMeasurementValue)
                {
                    _statusUpdateMeasurementValue =value;
                    OnPropertyChanged(nameof(StatusUpdateMeasurementValue));
                }
            }
        }

        // Properties of the side bar for selecting the measurement to plot
        private ObservableCollection<int> _comboBoxStations = new ObservableCollection<int>();
        private int _stationId = 0;
        private ObservableCollection<int> _comboBoxSensors = new ObservableCollection<int>();
        private int _sensorId = 0;
        private ObservableCollection<TimeSpans> _comboBoxTimeSpans = new ObservableCollection<TimeSpans>();
        private int _timeSpanId = 0;
        private TimeSpans _selectedTimeSpan = TimeSpans.day;

        /// <summary>
        /// Gets or sets the collection of available Station IDs for the ComboBox.
        /// Raises <see cref="PropertyChanged"/>.
        /// </summary>
        public ObservableCollection<int> ComboBoxStations
        {
            get
            {
                return _comboBoxStations;
            }
            set
            { 
                if (value != _comboBoxStations)
                {
                    _comboBoxStations = value;
                    OnPropertyChanged(nameof(ComboBoxStations));
                }
            }
        }
        /// <summary>
        /// Gets or sets the currently selected Station ID.
        /// Setting this value triggers an asynchronous update of the sensor list via <see cref="LoadSensorComboBox"/>.
        /// Raises <see cref="PropertyChanged"/>.
        /// </summary>
        public int StationId
        {
            get
            {
                return _stationId;
            }
            set
            {
                if (value != _stationId)
                {
                    _stationId = value;
                    OnPropertyChanged(nameof(StationId));

                    _ = LoadSensorComboBox();
                    //Task.Run(async () => await LoadSensorComboBox());
                }
            }
        }
        /// <summary>
        /// Gets or sets the collection of available Sensor IDs for the ComboBox.
        /// Raises <see cref="PropertyChanged"/>.
        /// </summary>
        public ObservableCollection<int> ComboBoxSensors
        {
            get
            {
                return _comboBoxSensors;
            }
            set
            {
                if (value != _comboBoxSensors)
                {
                    _comboBoxSensors = value;
                    OnPropertyChanged(nameof(ComboBoxSensors));
                }
            }
        }
        /// <summary>
        /// Gets or sets the currently selected <see cref="Sensor"/> object.
        /// </summary>
        public Sensor SelectedSensor { get; set; }
        /// <summary>
        /// Gets or sets the currently selected Sensor ID.
        /// Raises <see cref="PropertyChanged"/>.
        /// </summary>
        public int SensorId
        {
            get
            {
                return _sensorId;
            }
            set
            {
                if (value != _sensorId)
                {
                    _sensorId = value;
                    OnPropertyChanged(nameof(SensorId));
                }
            }
        }
        /// <summary>
        /// Gets or sets the collection of available <see cref="TimeSpans"/> for the ComboBox.
        /// Raises <see cref="PropertyChanged"/>.
        /// </summary>
        public ObservableCollection<TimeSpans> ComboBoxTimeSpans
        {
            get
            {
                return _comboBoxTimeSpans;
            }
            set
            {
                if (value != _comboBoxTimeSpans)
                {
                    _comboBoxTimeSpans = value;
                    OnPropertyChanged(nameof(ComboBoxTimeSpans));
                }
            }
        }
        /// <summary>
        /// Gets or sets the currently selected <see cref="TimeSpans"/>.
        /// Raises <see cref="PropertyChanged"/>.
        /// </summary>
        public TimeSpans SelectedTimeSpan 
        { 
            get
            {
                return _selectedTimeSpan;
            }
            set
            {
                if (value != _selectedTimeSpan)
                {
                    _selectedTimeSpan = value;
                    OnPropertyChanged(nameof(SelectedTimeSpan));
                }
            }
                }
        /// <summary>
        /// Gets or sets the Time Span ID (Note: This property mistakenly sets <see cref="SensorId"/> instead of a private field).
        /// Raises <see cref="PropertyChanged"/>.
        /// </summary>
        public int TimeSpanId
        {
            get
            {
                return _timeSpanId;
            }
            set
            {
                if (value != _timeSpanId)
                {
                    _sensorId = value;
                    OnPropertyChanged(nameof(TimeSpanId));
                }
            }
        }

        // Propperties of the plot it self
        /// <summary>
        /// The ScottPlot object used for graphical representation of measurements.
        /// </summary>
        public Plot MeasurementsPlot { get; set; }
        /// <summary>
        /// The data model containing the retrieved <see cref="Measurement"/> objects for plotting.
        /// </summary>
        public PlotData MeasurementPlotData { get; set; }

        /// <summary>
        /// Event fired to signal the View that the plot needs to be redrawn/refreshed.
        /// </summary>
        public event Action? RefreshRequired;

        /// <summary>
        /// The command used to manually trigger the data loading and plot refresh.
        /// </summary>
        public DelegateCommand LoadDataCommand { get; set; }

        /// <summary>
        /// The standard event handler for notifying the View of property changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;
        #endregion


        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
        /// Sets up the initial title, status color, timer, and starts the initial data load.
        /// </summary>
        public MainWindowViewModel()
        {
            Title = "PlantStation Overview";
            StatusColor = new SolidColorBrush(Colors.Red);

            InitializeTimer();

            // Asynchronously starts the initial data load and plotting.
            _ = InitialLoadDataAsync(true);

            // Defines the command logic for manual data loading.
            LoadDataCommand = new DelegateCommand(async o =>
            {
                StatusColor = new SolidColorBrush(Colors.Red);
                await RefreshPlot();
            });
        }

        /// <summary>
        /// Configures and starts the <see cref="DispatcherTimer"/> for automatic data refreshing every minute.
        /// </summary>
        private void InitializeTimer()
        {
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1)
            };
            _refreshTimer.Tick += RefreshTimer_Tick;
            _refreshTimer.Start();
        }

        /// <summary>
        /// Event handler triggered by the refresh timer. Initiates an asynchronous data reload.
        /// </summary>
        private async void RefreshTimer_Tick(object sender, EventArgs e)
        {
            await InitialLoadDataAsync(plotAfterwards: true);
        }

        /// <summary>
        /// Asynchronously loads all necessary initial data: station IDs, sensor IDs, and time spans.
        /// It also creates the initial <see cref="MeasurementPlotData"/>.
        /// </summary>
        /// <param name="plotAfterwards">If true, the plot is drawn immediately after data is loaded.</param>
        /// <returns>A Task representing the asynchronous data loading operation.</returns>
        public async Task InitialLoadDataAsync(bool plotAfterwards)
        {
            List<int> stationIdList = await _apiClient.GetAllStationIdsFromApiAsync<int>();
            ComboBoxStations = new ObservableCollection<int>(stationIdList);
            if (StationId == 0)
            {
                StationId = stationIdList.FirstOrDefault();
            }

            List<int> sensorList = await LoadSensorComboBox();

            List<TimeSpans> timeSpanList = Enum.GetValues(typeof(TimeSpans)).Cast<TimeSpans>().ToList();

            ComboBoxTimeSpans = new ObservableCollection<TimeSpans>(timeSpanList);
            if (SensorId == 0)
            {
                SensorId = sensorList.FirstOrDefault();
            }


            MeasurementPlotData = await PlotData.CreatePlot(SensorId, (int)_selectedTimeSpan);

            if (plotAfterwards)
            {
                PlotSingleSetOfMeasurements();
            }
        }

        /// <summary>
        /// Asynchronously retrieves all Sensor IDs associated with the currently selected station,
        /// updates the <see cref="ComboBoxSensors"/>, and selects the first sensor if available.
        /// </summary>
        /// <returns>A Task representing the list of retrieved sensor IDs.</returns>
        private async Task<List<int>> LoadSensorComboBox()
        {
            List<int> sensorList = await _apiClient.GetAllSensorIdsByStationIdFromApiAsync<int>(StationId);
            
            ComboBoxSensors = new ObservableCollection<int>(sensorList);

            if (sensorList.Count > 0)
            {
                SensorId = sensorList.FirstOrDefault();
            }
            else
            {
                _sensorId = 0;
                OnPropertyChanged(nameof(SensorId));
            }

            return sensorList;
        }

        /// <summary>
        /// Asynchronously refreshes the plot data by creating a new <see cref="PlotData"/> object
        /// and then redrawing the plot.
        /// </summary>
        /// <returns>A Task representing the refresh operation.</returns>
        private async Task RefreshPlot()
        {
            int test = (int)_selectedTimeSpan;
            MeasurementPlotData = await PlotData.CreatePlot(SensorId, (int)_selectedTimeSpan);
            PlotSingleSetOfMeasurements();

        }

        /// <summary>
        /// Takes the measurements from <see cref="MeasurementPlotData"/>, processes them (smoothing, time conversion),
        /// draws them into the <see cref="MeasurementsPlot"/> control, and updates the status bar.
        /// </summary>
        private void PlotSingleSetOfMeasurements()
        {
            if (MeasurementsPlot == null || MeasurementPlotData?.Measurements == null) return;

            try
            {
                MeasurementsPlot.Clear();

                List<Measurement> measurements = MeasurementPlotData.Measurements;
                double[] yValues = measurements.Select(m => Math.Round(m.Value,0)).ToArray();

                double[] smoothedY = MathService.CalculateMovingAverage(yValues, 60);

                System.DateTime[] zeiten = measurements.Select(m => m.RecordedAt.AddHours(2)).ToArray();
                double[] xAchseWerte = zeiten.Select(dt => dt.ToOADate()).ToArray();

                var linie = MeasurementsPlot.Add.Scatter(xAchseWerte, smoothedY);
                linie.LegendText = $"{measurements[0].Type} [{measurements[0].Unit}]";
                linie.MarkerSize = 1;

                string plotUnit = "";
                string plotType = "";
                if (measurements.Count>0)
                {
                    plotUnit = measurements[0].Unit;
                    plotType = measurements[0].Type;
                }

                MeasurementsPlot.Axes.Bottom.Label.Text = "Zeit";
                MeasurementsPlot.Axes.Left.Label.Text = $"{plotType}[{plotUnit}]";
                MeasurementsPlot.Axes.DateTimeTicksBottom();
                MeasurementsPlot.Axes.AutoScale();

                ScottPlot.AxisLimits limits = MeasurementsPlot.Axes.GetLimits();

                double yMinPlot = Math.Floor(limits.Bottom / 10.0) * 10.0;
                double yMaxPlot = Math.Ceiling(limits.Top / 10.0) * 10.0;

                if (yMaxPlot - yMinPlot < 10)
                {

                    double center = (yMinPlot + yMaxPlot) / 2.0;
                    yMinPlot = center - 5.0;
                    yMaxPlot = center + 5.0;
                }

                MeasurementsPlot.Axes.SetLimitsY(yMinPlot, yMaxPlot);

                RefreshRequired?.Invoke();
                StatusUpdateTime = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                StatusUpdateStationId = StationId.ToString();
                StatusUpdateSensorId = SensorId.ToString();

                Measurement newestMeasurement = MeasurementPlotData.Measurements[1];
                StatusUpdateMeasurementValue = Math.Round(newestMeasurement.Value, 2).ToString() + newestMeasurement.Unit;
                StatusColor = new SolidColorBrush(Colors.LightGreen);
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Helper method to raise the <see cref="PropertyChanged"/> event,
        /// notifying the View that a property value has changed.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}