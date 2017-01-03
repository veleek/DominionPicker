using Ben.Dominion.ViewModels;
using System.Diagnostics;
using Windows.UI.Xaml.Controls;

namespace Ben.Dominion
{

    public partial class ConfigurationPage: Page
    {
        public ConfigurationPage()
        {
            this.InitializeComponent();
        }

        public ConfigurationModel ViewModel
        {
            get
            {
                ConfigurationModel viewModel = ConfigurationModel.Instance;
                return viewModel;
            }
        }
    }

}