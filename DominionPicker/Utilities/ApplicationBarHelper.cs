using System;
using System.Diagnostics;
using Ben.Dominion.Resources;
using Ben.Utilities;
using Microsoft.Phone.Shell;

namespace Ben.Dominion.Utilities
{
    public class ApplicationBarManager
    {
        private readonly ApplicationBarMenuItem cardLookupItem;
        private readonly ApplicationBarMenuItem blackMarketItem;
        private readonly ApplicationBarMenuItem settingsItem;
        private readonly ApplicationBarMenuItem aboutItem;
        private readonly ApplicationBarMenuItem debugItem;

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

        public void InitializeDefaultMenu(ApplicationBar appBar)
        {
            appBar.MenuItems.Add(this.cardLookupItem);
            appBar.MenuItems.Add(this.blackMarketItem);
            appBar.MenuItems.Add(this.settingsItem);
            appBar.MenuItems.Add(this.aboutItem);

            if (Debugger.IsAttached)
            {
                appBar.MenuItems.Add(this.debugItem);
            }
        }

        private void CardLookup_Click(object sender, EventArgs e)
        {
            NavigationServiceHelper.Navigate("/CardFilterPage.xaml");
        }

        private void BlackMarket_Click(object sender, EventArgs e)
        {
            NavigationServiceHelper.Navigate("/BlackMarketPage.xaml");
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            NavigationServiceHelper.Navigate("/ConfigurationPage.xaml");
        }

        private void About_Click(object sender, EventArgs e)
        {
            NavigationServiceHelper.Navigate("/AboutPage.xaml");
        }

        private void Debug_Click(object sender, EventArgs e)
        {
            NavigationServiceHelper.Navigate("/DebugPage.xaml");
        }
    }

    public static class ApplicationBarHelper
    {
        public static ApplicationBarMenuItem CreateMenuItem(string text, EventHandler clickHandler)
        {
            var menuItem = new ApplicationBarMenuItem
            {
                Text = text
            };
            menuItem.Click += clickHandler;

            return menuItem;
        }

        public static ApplicationBarIconButton CreateIconButton(string text, string iconPath, EventHandler clickHandler)
        {
            return CreateIconButton(text, new Uri(iconPath, UriKind.Relative), clickHandler);
        }

        public static ApplicationBarIconButton CreateIconButton(string text, Uri iconUri, EventHandler clickHandler)
        {
            var iconButton = new ApplicationBarIconButton
            {
                Text = text,
                IconUri = iconUri
            };

            iconButton.Click += clickHandler;

            return iconButton;
        }
    }

    public static class ApplicationBarExtensions
    {
        public static void AddMenuItem(this IApplicationBar appBar, string text, EventHandler handler)
        {
            appBar.MenuItems.Add(ApplicationBarHelper.CreateMenuItem(text, handler));
        }

        public static void AddIconButton(this IApplicationBar appBar, string text, string iconPath,
            EventHandler clickHandler)
        {
            appBar.Buttons.Add(ApplicationBarHelper.CreateIconButton(text, iconPath, clickHandler));
        }
    }
}