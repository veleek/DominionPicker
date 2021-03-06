using System;
using System.Diagnostics;
using Ben.Dominion.Resources;
using Ben.Dominion.Views;
using Ben.Utilities;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Ben.Dominion.Utilities
{

    public class ApplicationBarManager
    {
        private readonly AppBarButton cardLookupItem;
        private readonly AppBarButton blackMarketItem;
        private readonly AppBarButton settingsItem;
        private readonly AppBarButton aboutItem;
        private readonly AppBarButton debugItem;

        public ApplicationBarManager()
        {
            this.cardLookupItem = ApplicationBarHelper.CreateMenuItem(Strings.Menu_CardLookup, this.CardLookup_Click);
            this.blackMarketItem = ApplicationBarHelper.CreateMenuItem(Strings.Menu_BlackMarket, this.BlackMarket_Click);
            this.settingsItem = ApplicationBarHelper.CreateMenuItem(Strings.Menu_Settings, this.Settings_Click);
            this.aboutItem = ApplicationBarHelper.CreateMenuItem(Strings.Menu_About, this.About_Click);
            if (Debugger.IsAttached)
            {
                this.debugItem = ApplicationBarHelper.CreateMenuItem("Debug", this.Debug_Click);
            }
        }

        public void InitializeDefaultMenu(CommandBar appBar)
        {
            appBar.SecondaryCommands.Add(this.cardLookupItem);
            appBar.SecondaryCommands.Add(this.blackMarketItem);
            appBar.SecondaryCommands.Add(this.settingsItem);
            appBar.SecondaryCommands.Add(this.aboutItem);
            if (Debugger.IsAttached)
            {
                appBar.SecondaryCommands.Add(this.debugItem);
            }
        }

        private void CardLookup_Click(object sender, RoutedEventArgs e)
        {
            PickerView.CardLookup.Go();
        }

        private void BlackMarket_Click(object sender, RoutedEventArgs e)
        {
            PickerView.BlackMarket.Go();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            PickerView.Settings.Go();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            PickerView.About.Go();
        }

        private void Debug_Click(object sender, RoutedEventArgs e)
        {
            PickerView.Debug.Go();
        }

    }

    public static class ApplicationBarHelper
    {

        public static AppBarButton CreateMenuItem(string text, RoutedEventHandler clickHandler)
        {

            var menuItem = new AppBarButton
            {
                Label = text
            };
            menuItem.Click += clickHandler;
            return menuItem;
        }

        public static AppBarButton CreateIconButton(string text, string iconPath, RoutedEventHandler clickHandler)
        {
            return CreateIconButton(text, new Uri("ms-appx:///" + iconPath, UriKind.Absolute), clickHandler);
        }

        public static AppBarButton CreateIconButton(string text, Uri iconUri, RoutedEventHandler clickHandler)
        {
            var iconButton = new AppBarButton();
            iconButton.Label = text;
            iconButton.Click += clickHandler;
            iconButton.Icon = new BitmapIcon { UriSource = iconUri };
            return iconButton;
        }

        public static AppBarButton CreateSymbolButton(string text, Symbol symbol, RoutedEventHandler clickHandler)
        {
            var symbolButton = new AppBarButton
            {
                Label = text,
                Icon = new SymbolIcon(symbol)
            };

            symbolButton.Click += clickHandler;

            return symbolButton;
        }
    }

    public static class ApplicationBarExtensions
    {
        public static void AddMenuItem(this CommandBar appBar, string text, RoutedEventHandler handler)
        {
            appBar.SecondaryCommands.Add(ApplicationBarHelper.CreateMenuItem(text, handler));
        }

        public static void AddIconButton(this CommandBar appBar, string text, string iconPath, RoutedEventHandler clickHandler)
        {
            appBar.PrimaryCommands.Add(ApplicationBarHelper.CreateIconButton(text, iconPath, clickHandler));
        }

        public static void AddSymbolButton(this CommandBar appBar, string text, Symbol symbol, RoutedEventHandler clickHandler)
        {
            appBar.PrimaryCommands.Add(ApplicationBarHelper.CreateSymbolButton(text, symbol, clickHandler));
        }
}
}