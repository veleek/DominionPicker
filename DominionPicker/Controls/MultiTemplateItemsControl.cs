using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Ben.Controls
{
    public interface IDataTemplateSelector
    {
	    DataTemplate SelectTemplate(object item, DependencyObject container);
    }

	public class DataTemplateMapSelector : IDataTemplateSelector
	{
		public DataTemplateMapSelector()
		{
			this.DataTemplateMap = new Dictionary<Type, DataTemplate>();
		}

		public DataTemplateMapSelector(Dictionary<Type, DataTemplate> dataTemplateMap)
		{
			this.DataTemplateMap = dataTemplateMap;
		}

		public Dictionary<Type, DataTemplate> DataTemplateMap { get; private set; }

		public DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}

			Type t = item.GetType();
			DataTemplate dataTemplate;
			if (!this.DataTemplateMap.TryGetValue(t, out dataTemplate))
			{
				throw new ArgumentException("No DataTemplate defined for Type " + t);
			}

			return dataTemplate;
		}

	}

    public class MultiTemplateItemsControl : ItemsControl
    {
        public IDataTemplateSelector TemplateSelector
        {
            get { return (IDataTemplateSelector)GetValue(TemplateSelectorProperty); }
            set { SetValue(TemplateSelectorProperty, value); }
        }

        public static readonly DependencyProperty TemplateSelectorProperty =
            DependencyProperty.Register(
                "TemplateSelector",
                typeof(IDataTemplateSelector),
                typeof(MultiTemplateItemsControl),
                new PropertyMetadata(new PropertyChangedCallback(OnTemplateChanged)));

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            var contentItem = element as ContentPresenter;

            if (contentItem != null)
            {
                contentItem.ContentTemplate = this.TemplateSelector.SelectTemplate(item, this);
            }
        }

        private static void OnTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
    }

    public class MultiTemplateContentControl : ContentControl
    {
        public IDataTemplateSelector TemplateSelector
        {
            get { return (IDataTemplateSelector)GetValue(TemplateSelectorProperty); }
            set { SetValue(TemplateSelectorProperty, value); }
        }

        public static readonly DependencyProperty TemplateSelectorProperty =
            DependencyProperty.Register(
                "TemplateSelector",
                typeof(IDataTemplateSelector),
                typeof(MultiTemplateContentControl),
                new PropertyMetadata(new PropertyChangedCallback(OnTemplateChanged)));

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            if (newContent != null)
            {
                this.ContentTemplate = this.TemplateSelector.SelectTemplate(newContent, this);
            }
        }

        private static void OnTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
    }
}
