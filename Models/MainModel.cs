using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Ben.Dominion.Resources;
using Ben.Dominion.Models;

namespace Ben.Dominion
{
    public class MainModel
    {
        private static MainModel instance;

        public static MainModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MainModel();
                }
                return instance;
            }
        }

        public Strings Strings
        {
            get
            {
                return new Strings();
            }
        }

        public ConfigurationModel Configuration
        {
            get
            {
                return ConfigurationModel.Instance;
            }
        }
    }
}
