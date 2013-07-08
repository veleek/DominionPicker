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

namespace Ben.Dominion
{
    public class LocalizedResources
    {
        public LocalizedResources() { }

        private static Strings strings = new Strings();

        public Strings Strings { get { return strings; } }
    }
}
