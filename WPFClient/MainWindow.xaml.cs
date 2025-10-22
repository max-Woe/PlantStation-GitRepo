using System.Windows;
using System.ComponentModel;
using WPFClient.ViewModels;
using System.Threading.Tasks;

namespace WPFClient
{
    public partial class MainWindow : Window
    {
        // Speichern Sie das ViewModel, um später im Closing-Event darauf zugreifen zu können
        private MainWindowViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new MainWindowViewModel();

            _viewModel.MeasurementsPlot = wpfPlot.Plot;

            this.DataContext = _viewModel;

            _viewModel.RefreshRequired += ViewModel_RefreshRequired;

            _ = _viewModel.InitialLoadDataAsync(plotAfterwards: true);
        }

        private void ViewModel_RefreshRequired()
        {
            this.Dispatcher.Invoke(() =>
            {
                wpfPlot.Refresh();
            });
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.RefreshRequired -= ViewModel_RefreshRequired;
            }

            MessageBoxResult messageBoxResult = MessageBox.Show(
                "Sie schließen gerade die Pflanzenübersicht!\n" +
                "Wollen Sie dies wirklich tun?", "PlantStationView schließen?", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

            if (messageBoxResult == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
        }
    }
}