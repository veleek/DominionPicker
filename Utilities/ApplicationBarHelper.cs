using System;
using Microsoft.Phone.Shell;

namespace Ben.Dominion.Utilities
{
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
    }
}
