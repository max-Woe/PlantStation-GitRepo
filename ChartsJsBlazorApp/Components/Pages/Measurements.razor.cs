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
using Microsoft.AspNetCore.Components;
// Fügen Sie hier alle fehlenden using-Anweisungen hinzu, die im ursprünglichen @code Block nicht enthalten waren
// (z.B. für ApiClient, ChartJs, etc.)

public enum TimeSpans
{
    hour = 60,
    threeHours = 180,
    halfDay = 720,
    day = 1440,
    week = 10080,
    month = 44640,
    year = 525600
}

// Stellen Sie sicher, dass dies derselbe Namespace ist, den das Projekt verwendet, 
// oder passen Sie den Namen der Klasse an, wenn Sie @inherits verwenden.
namespace ChartsJsBlazorApp.Components.Pages
{
    public class MeasurementsBase : ComponentBase
    {
        // Alle privaten Felder aus dem @code Block
        protected List<int> _stationIds = new List<int>();
        protected List<int> _sensorIds = new List<int>();
        protected List<TimeSpans> _timeSpans = Enum.GetValues(typeof(TimeSpans)).Cast<TimeSpans>().ToList();

        // public int _selectedSensorId = 0;

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
        public bool _isPlotReady = false;
        
        public bool IsSensorsComboboxDisabled { get; set; } = true;
        public bool IsTimeSpansComboboxDisabled { get; set; } = true;

        protected List<Measurement> _measurements = new List<Measurement>();
        protected List<double> _values = new List<double>();
        protected List<DateTime> _times = new List<DateTime>();

        // Annahme, dass ApiClient und LineConfig in dieser Klasse bekannt sind
        protected ApiClient _apiClient;
        protected LineConfig _config = new LineConfig();

        // public int _selectedStationId = 0;

        public int SelectedStationId { get; set; } = 0;
        public int SelectedSensorId { get; set; } = 0;
        public int SelectedTimeSpan { get; set; } = 0;

        // Die Konfiguration wird einmal initialisiert
        protected override async Task OnInitializedAsync()
        {
            // 💡 HINWEIS: Hier müssen die Typen (ApiClient, LineConfig, ColorUtil, EntityConverter)
            // durch zusätzliche 'using' Anweisungen (oder global in _Imports.razor) zugänglich gemacht werden.
            _apiClient = new ApiClient();

            await FetchStationIds();
            // double minVal;
            // double maxVal;
            //
            // if (_values.Any())
            // {
            //     double actualMin = _values.Min();
            //     double actualMax = _values.Max();
            //
            //     minVal = Math.Floor(actualMin / 10.0) * 10;
            //     maxVal = Math.Ceiling(actualMax / 10.0) * 10;
            //
            //     if (minVal == maxVal)
            //     {
            //         minVal -= 10;
            //         maxVal += 10;
            //     }
            // }
            // else
            // {
            //     minVal = 0;
            //     maxVal = 10;
            // }
            //
            // if (_measurements.Any())
            // {
            //     InitializeConfig(minVal, maxVal);
            // }
        }

        protected async Task FetchStationIds()
        {
            try
            {
                _stationIds = await _apiClient.GetAllStationIdsFromApiAsync<int>();
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

        private async Task<List<int>> LoadSensorIdsByStationId(int selectedStationId)
        {
            try
            {
                List<int> sensorIdsFromContext =
                    await _apiClient.GetAllSensorIdsByStationIdFromApiAsync<int>(selectedStationId);
                
                return sensorIdsFromContext;
            }
            catch (Exception e)
            {
                return new List<int>() { -1 };
            }
        }


        private async Task SetMeasurementsForPlot(int sensorId, DateTime since)
        {
            ResetPlotAndPlotData();

            string? responseString = await _apiClient.GetMeasurementsFromApiAsyncAsString("GetLastOfSensorSince",
                sensorId, 0, since);

            if (responseString != null)
            {
                // HINWEIS: .Result blockiert hier, besser ist es, eine asynchrone Methode zu verwenden, 
                // die direkt await-fähig ist (z.B. await EntityConverter.ConvertAsync(...)). 
                // Hier wurde es für die Kompatibilität mit dem Originalcode beibehalten.
                _measurements = EntityConverter.ConvertStringToListOfEntities<Measurement>(responseString).Result;
                _measurements.Reverse();

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
            _config = new LineConfig();
        }

        protected void InitializeConfig(double min, double max)
        {
            // Da die LineConfig und die Chart-Klassen viele 'using's benötigen, 
            // wird der Code hier leicht gekürzt, um die Lesbarkeit zu gewährleisten.

            _config = new LineConfig
            {
                Options = new LineOptions
                {
                    Responsive = true,
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
                _config.Data.Labels.Add(measurement.RecordedAt.ToString("yy.MM.dd HH:mm:ss"));
            }

            LineDataset<double> dataset = new LineDataset<double>(_values)
            {
                Label = $"{_measurements[0].Type} [{_measurements[0].Unit}]",
                BackgroundColor = ColorUtil.ColorHexString(54, 162, 235),
                BorderColor = ColorUtil.ColorHexString(54, 162, 235),
                Fill = false,
                PointRadius = 0,
                PointBackgroundColor = ColorUtil.ColorHexString(54, 162, 235)
            };

            _config.Data.Datasets.Add(dataset);
        }
        
        private async Task PlotIfReady()
        {
            if (IsStationSelected && _isSensorSelected && _isTimeSpanSelected)
            {
                _isPlotReady = true;
                
                await SetMeasurementsForPlot(SelectedSensorId, DateTime.UtcNow.AddMinutes(-SelectedTimeSpan));//AddMinutes(-60));//.
                await LoadPlot();
            }
        }
        
        protected async Task HandleStationSelection(ChangeEventArgs e)
        {
            // 1. Wert aus dem Event lesen und konvertieren
            if (int.TryParse(e.Value?.ToString(), out int newStationId) && newStationId > 0)
            {
                IsStationSelected = true; // Sensor-Combobox freischalten
                SelectedStationId = newStationId;
        
                _sensorIds = await LoadSensorIdsByStationId(SelectedStationId);
        
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

            await InvokeAsync(StateHasChanged);
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