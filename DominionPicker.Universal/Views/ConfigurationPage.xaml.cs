using Ben.Dominion.ViewModels;
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
            get { return ConfigurationModel.Instance; }
        }
    }

}