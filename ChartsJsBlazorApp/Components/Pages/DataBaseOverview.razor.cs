using DataAccess.Models;
using ConverterService;
using Microsoft.AspNetCore.Components;

namespace ChartsJsBlazorApp.Components.Pages;

public partial class DataBaseMeasurementsBase : ComponentBase
{
    private ApiClient _apiClient;
    public List<string> AvailableTables { get; set; } = new List<string>();
    public string? SelectedTable { get; set; } = null;
    
    
    public List<Station> StationsList { get; set; } = new List<Station>();
    public List<Sensor> SensorsList { get; set; } = new List<Sensor>();
    public List<Measurement> MeasurementsList { get; set; } = new List<Measurement>();

    protected override async Task OnInitializedAsync()
    {
        _apiClient = new ApiClient();
        AvailableTables.Add("Stations");
        AvailableTables.Add("Sensors");
        AvailableTables.Add("Measurements");
    }

    private async Task FetchTableAsync()
    {
        switch (SelectedTable)
        {
            case "Stations":
            {
                StationsList = await _apiClient.GetAllStationsFromApiAsync<Station>();
                break;
            }
            case "Sensors":
            {
                SensorsList = await _apiClient.GetAllSensorsByStationIdFromApiAsync<Sensor>(18);
                break;
            }
            case "Measurements":
            {
                DateTime localDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
                     string? responseString = await _apiClient.GetMeasurementsFromApiAsyncAsString(
                         "GetLastOfSensorSince", 
                         88,
                         0,
                         localDateTime.AddHours(-2));
             
                     if (responseString != null)
                     {
                         MeasurementsList = EntityConverter.ConvertStringToListOfEntities<Measurement>(responseString).Result;
                     }

                     break;
            }
        }
        
    }
    
    protected async Task HandleTableSelected(ChangeEventArgs e)
    {
        SelectedTable = e.Value.ToString();
        
        if (SelectedTable != "0" && SelectedTable != null)
        {
            await FetchTableAsync();
        }
        
        await InvokeAsync(StateHasChanged);
    }
}