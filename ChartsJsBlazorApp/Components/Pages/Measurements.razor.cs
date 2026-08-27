using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChartJs.Blazor.Common;
using ChartJs.Blazor.Common.Axes;
using ChartJs.Blazor.Common.Axes.Ticks;
using ChartJs.Blazor.LineChart;
using ChartJs.Blazor.Util;
using ChartsJsBlazorApp;
using ConverterService;
using DataAccess.Models;
using LoggingService;
using Microsoft.AspNetCore.Components;
// Fügen Sie hier alle fehlenden using-Anweisungen hinzu, die im ursprünglichen @code Block nicht enthalten waren
// (z.B. für ApiClient, ChartJs, etc.)

public enum TimeSpans
{
    Hour = 60,
    ThreeHours = 180,
    HalfDay = 720,
    Day = 1440,
    Week = 10080,
    Month = 44640,
    Year = 525600
}

// Stellen Sie sicher, dass dies derselbe Namespace ist, den das Projekt verwendet, 
// oder passen Sie den Namen der Klasse an, wenn Sie @inherits verwenden.
namespace ChartsJsBlazorApp.Components.Pages
{
    public class MeasurementsBase : ComponentBase
    {
        private List<Station> _avalibaleStations = new List<Station>();
        private List<(int, string)> _sensorIdsAndTypes = new List<(int, string)>();
        private ILoggingService _logger = new SeriLoggingService();
        
        public List<(int,string)> SensorIdsAndTypes 
        {
            get
            {
                return _sensorIdsAndTypes;
            }
            set
            {
                if (_sensorIdsAndTypes != value)
                {
                    _sensorIdsAndTypes = value;
                }
            }
        }
        protected List<int> StationIds = new List<int>();
        private List<string> StationNames = new List<string>();
        
        private List<int> _sensorIds = new List<int>();
        private readonly List<string> _sensorTypes = new List<string>();
        protected readonly List<TimeSpans> TimeSpans = Enum.GetValues(typeof(TimeSpans)).Cast<TimeSpans>().ToList();

        private bool _isStationSelected = false; 
        public bool IsStationSelected
        {
            get
            {
                return _isStationSelected;
            }
            set
            {
                if (_isStationSelected != value)
                {
                    _isStationSelected = value;
                    IsSensorsComboboxDisabled = !value;
                }
            }
        }
        
        private bool _isSensorSelected = false;
        public bool IsSensorSelected
        {
            get
            {
                return _isSensorSelected;    
            }
            set
            {
                if (_isSensorSelected != value)
                {
                    _isSensorSelected = value;
                    IsTimeSpansComboboxDisabled = !value;
                }
            }
        }
        
        private bool _isTimeSpanSelected = false;
        protected bool IsPlotReady = false;
        
        public bool IsSensorsComboboxDisabled { get; set; } = true;
        public bool IsTimeSpansComboboxDisabled { get; set; } = true;

        private List<Measurement> _measurements = new List<Measurement>();
        private readonly List<double> _values = new List<double>();
        protected List<DateTime> Times = new List<DateTime>();
        private string _plotColor = ColorUtil.ColorHexString(255, 255, 255);

        // Annahme, dass ApiClient und LineConfig in dieser Klasse bekannt sind
        private ApiClient _apiClient;
        protected LineConfig Config = new LineConfig();

        // public int _selectedStationId = 0;

        private int SelectedStationId { get; set; } = 0;
        private int SelectedSensorId { get; set; } = 0;
        private int SelectedTimeSpan { get; set; } = 0;

        // Die Konfiguration wird einmal initialisiert
        protected override async Task OnInitializedAsync()
        {
            // _config = new LineConfig();
            _apiClient = new ApiClient();

            await FetchStations();
        }

        protected async Task FetchStations()
        {
            try
            {
                if (StationIds.Count > 0) StationIds.Clear();
                if (StationNames.Count > 0) StationNames.Clear();
                
                _avalibaleStations = await _apiClient.GetAllStationsFromApiAsync<Station>();
                foreach (Station station in _avalibaleStations)
                {
                    StationIds.Add(station.Id);
                } 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async Task LoadPlot()
        {
            double minVal;
            double maxVal;
            
            if (_values.Any())
            {
                double actualMin = _values.Min();
                double actualMax = _values.Max();

                minVal = Math.Floor(actualMin / 10.0) * 10;
                maxVal = Math.Ceiling(actualMax / 10.0) * 10;

                if (minVal == maxVal)
                {
                    minVal -= 10;
                    maxVal += 10;
                }
            }
            else
            {
                minVal = 0;
                maxVal = 10;
            }

            if (_measurements.Any())
            {
                InitializeConfig(minVal, maxVal);
            }
        }

        private async Task<List<(int,string)>> FetchSensorIdsAndTypesByStationId(int selectedStationId)
        {
            try
            {
                List<(int, string)> stationIdsAndTypes = new List<(int, string)>();
                List<Sensor> sensorsFromContext = await _apiClient.GetAllSensorsByStationIdFromApiAsync<Sensor>(selectedStationId);

                foreach (Sensor sensor in sensorsFromContext)
                {
                    if (sensor.Id != 0 && !string.IsNullOrEmpty(sensor.Type))
                    {
                        stationIdsAndTypes.Add((sensor.Id, sensor.Type));
                    }
                }
                
                return stationIdsAndTypes;
            }
            catch (Exception e)
            {
                return new List<(int,string)>() { (-1, "") };
            }
        }


        private async Task SetMeasurementsForPlot(int sensorId, DateTime since)
        {
            ResetPlotAndPlotData();

            since = since.ToUniversalTime();

            string? responseString = await _apiClient.GetMeasurementsFromApiAsyncAsString(
                "GetLastOfSensorSince",
                sensorId,
                0, 
                since);

            if (responseString != null)
            {
                // HINWEIS: .Result blockiert hier, besser ist es, eine asynchrone Methode zu verwenden, 
                // die direkt await-fähig ist (z.B. await EntityConverter.ConvertAsync(...)). 
                // Hier wurde es für die Kompatibilität mit dem Originalcode beibehalten.
                _measurements = EntityConverter.ConvertStringToListOfEntities<Measurement>(responseString).Result;
                _measurements = _measurements.DistinctBy(m => m.RecordedAt).ToList();
                _measurements = _measurements.OrderByDescending(m => m.RecordedAt).ToList();
                if (_measurements[0].Type == "temperature")
                {
                    _measurements.RemoveAll(m => (m.Value < -30 | m.Value > 70));
                }
                else if (_measurements[0].Type == "humidity" | _measurements[0].Type == "soil_moisture")
                {
                    _measurements.RemoveAll(m => (m.Value < 0 | m.Value > 100));
                }

                _measurements.Reverse();

                switch (_measurements[0].Type)
                {
                    case "soil_moisture":
                    {
                        _plotColor = ColorUtil.ColorHexString(204, 51, 0); // braun
                        break;
                    }
                    case "humidity":
                    {
                        _plotColor = ColorUtil.ColorHexString(0, 102, 255); // blau
                        break;
                    }
                    case "temperature":
                    {
                        _plotColor = ColorUtil.ColorHexString(255, 0, 0); // rot
                        break;
                    }
                }

                foreach (Measurement measurement in _measurements)
                {
                    _values.Add(measurement.Value);
                }
            }
        }

        private void ResetPlotAndPlotData()
        {
            _measurements.Clear();
            _values.Clear();
            Config = new LineConfig();
        }

        private void InitializeConfig(double min, double max)
        {
            Config = new LineConfig
            {
                Options = new LineOptions
                {
                    Responsive = true,
                    MaintainAspectRatio = false,
                    AspectRatio = 1.778,
                    Title = new OptionsTitle { Display = true, Text = _measurements[0].Type },
                    Scales = new Scales
                    {
                        XAxes = new List<CartesianAxis>
                        {
                            new CategoryAxis
                                { ScaleLabel = new ScaleLabel { LabelString = "Zeitpunkt", Display = true } }
                        },
                        YAxes = new List<CartesianAxis>
                        {
                            new LinearCartesianAxis
                            {
                                ScaleLabel = new ScaleLabel { LabelString = "Wert", Display = true },
                                Ticks = new LinearCartesianTicks { Min = min, Max = max }
                            }
                        }
                    }
                }
            };

            foreach (Measurement measurement in _measurements)
            {
                Config.Data.Labels.Add(measurement.RecordedAt.ToLocalTime().ToString("yy.MM.dd HH:mm:ss"));
            }

            LineDataset<double> dataset = new LineDataset<double>(_values)
            {
                Label = $"{_measurements[0].Type} [{_measurements[0].Unit}]",
                BackgroundColor = _plotColor,
                BorderColor = _plotColor,
                Fill = false,
                PointRadius = 0,
                PointBackgroundColor = _plotColor
            };

            Config.Data.Datasets.Add(dataset);
        }
        
        private async Task PlotIfReady()
        {
            if (IsStationSelected && _isSensorSelected && _isTimeSpanSelected)
            {
                IsPlotReady = true;
                DateTime localTime =  TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
                await SetMeasurementsForPlot(SelectedSensorId, localTime.AddMinutes(-SelectedTimeSpan));//AddMinutes(-60));//.
                await LoadPlot();
            }
        }
        
        protected async Task HandleStationSelection(ChangeEventArgs e)
        {
            // 1. Wert aus dem Event lesen und konvertieren
            if (int.TryParse(e.Value?.ToString(), out int newStationId) && newStationId > 0)
            {
                if (_sensorIds.Count > 0)
                {
                    _sensorIds.Clear();
                }
                if (_sensorTypes.Count > 0)
                {
                    _sensorTypes.Clear();
                }
                
                IsStationSelected = true; // Sensor-Combobox freischalten
                SelectedStationId = newStationId;
                _sensorIdsAndTypes= await FetchSensorIdsAndTypesByStationId(SelectedStationId);
                foreach ((int,string) sensorIdAndType in _sensorIdsAndTypes)
                {
                    _sensorIds.Add(sensorIdAndType.Item1);
                    _sensorTypes.Add(sensorIdAndType.Item2);
                }
        
                // 3. UI zwingen, sich zu aktualisieren (wird meist automatisch gemacht, aber ist sicherer)
                await PlotIfReady();
            }
            else
            {
                // Auswahl zurückgesetzt oder ungültig
                SelectedStationId = 0;
                _sensorIds = new List<int>();
                IsStationSelected = false;
                _isSensorSelected = false;
            }

            try
            {
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "InvokeAsync(StateHasChanged)","HandleStationSelection",null);
            }
        }


        protected async Task HandleSensorSelection(ChangeEventArgs e)
        {
            // 1. Wert aus dem Event lesen und konvertieren
            if (int.TryParse(e.Value?.ToString(), out int newSensorId) && newSensorId > 0)
            {

                IsSensorSelected = true; // Plot freigeben
                SelectedSensorId = newSensorId;
        
                await PlotIfReady();
        
            }
            else
            {
                // Auswahl zurückgesetzt oder ungültig
                SelectedSensorId = 0;
                IsSensorSelected = false;
            }
            // 3. UI zwingen, sich zu aktualisieren (wird meist automatisch gemacht, aber ist sicherer)
            await InvokeAsync(StateHasChanged);
        }
        
        protected async Task HandleTimeSpanSelection(ChangeEventArgs e)
        {
            string? selectedTimeSpanString = e.Value?.ToString();
            
            if(Enum.TryParse(selectedTimeSpanString, ignoreCase:true, out TimeSpans newTimeSpan))
            {
                
                // Konvertierung erfolgreich und gültig
                SelectedTimeSpan = (int)newTimeSpan;
                
                _isTimeSpanSelected = true;
                
                await PlotIfReady();
            }
            else
            {
                // Auswahl zurückgesetzt oder ungültig
                SelectedSensorId = 0;
                _isSensorSelected = false;
            }
            // 3. UI zwingen, sich zu aktualisieren (wird meist automatisch gemacht, aber ist sicherer)
            await InvokeAsync(StateHasChanged);
        }

    }
}