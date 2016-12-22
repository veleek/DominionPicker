using Ben.Dominion.ViewModels;
using System.Diagnostics;
using Windows.UI.Xaml.Controls;

namespace Ben.Dominion
{

    public partial class ConfigurationPage: Page
    {
        public ConfigurationPage()
        {
            Debug.WriteLine("ConfigurationPage constructor");
            this.InitializeComponent();
            Debug.WriteLine("ConfigurationPage constructor done");
        }

        public ConfigurationModel ViewModel
        {
            get
            {
                Debug.WriteLine("Loading Configuration Instance");
                ConfigurationModel viewModel = ConfigurationModel.Instance;
                Debug.WriteLine("Configuration Instance loaded.");
                return viewModel;
            }
        }
    }

}