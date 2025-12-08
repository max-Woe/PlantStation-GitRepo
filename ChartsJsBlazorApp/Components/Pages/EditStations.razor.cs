using DataAccess.Models;
using Microsoft.AspNetCore.Components;


namespace ChartsJsBlazorApp.Components.Pages
{
    public class EditStationsBase: ComponentBase
    {
        private ApiClient _apiClient;
        public List<Station> AvailaleStations { get; set; } = new List<Station>();
        public int SelectedStationIdCombobox { get; set; } = -1;
        public string? SelectedStationMacAdress { get; set; } = "--------------------";
        
        private string? _selectedStationLoctaion = "--------------------";
        public string? SelectedStationLoctaion
        {
            get
            {
                return _selectedStationLoctaion;
            }
            set
            {
                if (value != _selectedStationLoctaion)
                {
                    _selectedStationLoctaion = value;
                    NewStationLocation = value;
                }
            }
        }

        public int? SelectedStationSensorCount { get; set; } = -1;
        public DateTime? SelectedStationCreatedAt { get; set; } = new DateTime(1, 1, 1,1,1,1);
        
        public string? NewStationLocation { get; set; }
        

        protected override async Task OnInitializedAsync()
        {
            _apiClient = new ApiClient();

            await FetchStations();
            
        }
        
        protected async Task FetchStations()
        {
            if (AvailaleStations.Count > 0) AvailaleStations.Clear();
            
            try
            {
                AvailaleStations = await _apiClient.GetAllStationsFromApiAsync<Station>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
        protected async Task HandleStationUpdateSelction(ChangeEventArgs e)
        {
            if (int.TryParse(e.Value.ToString(), out int stationId))
            {
                SelectedStationIdCombobox = stationId;
                foreach (Station availaleStation in AvailaleStations)
                {
                    if (availaleStation.Id == SelectedStationIdCombobox)
                    {
                        SelectedStationMacAdress = availaleStation.MacAddress;
                        SelectedStationLoctaion =  availaleStation.Location;
                        SelectedStationSensorCount = availaleStation.SensorsCount;
                        SelectedStationCreatedAt = availaleStation.CreatedAt;
                    }
                }
            }
            else
            {
                Console.WriteLine(e);
            }
            await InvokeAsync(StateHasChanged);
        }
        
        public void SelectStation(int stationId)
        {
            // Setzt die ID der ausgewählten Station
            SelectedStationIdCombobox = stationId;

            // Hier könnten Sie weitere Aktionen ausführen, z.B. Daten laden
            Console.WriteLine($"Station {stationId} ausgewählt.");
        }
    }
}