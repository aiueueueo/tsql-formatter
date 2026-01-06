using System.Windows;
using TSqlFormatter.Extension.ViewModels;

namespace TSqlFormatter.Extension.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly SettingsViewModel _viewModel;

        public SettingsWindow()
        {
            InitializeComponent();
            _viewModel = new SettingsViewModel();
            DataContext = _viewModel;
            _viewModel.RequestClose += OnRequestClose;
        }

        private void OnRequestClose(object? sender, System.EventArgs e)
        {
            Close();
        }

        protected override void OnClosed(System.EventArgs e)
        {
            _viewModel.RequestClose -= OnRequestClose;
            base.OnClosed(e);
        }
    }
}
