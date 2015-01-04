using System.Windows;
using System.Windows.Controls;

namespace Ben.Controls
{
    public class DataTemplateSelector
    {
        public virtual DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return null;
        }
    }

    public class MultiTemplateItemsControl : ItemsControl
    {
        public DataTemplateSelector TemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(TemplateSelectorProperty); }
            set { SetValue(TemplateSelectorProperty, value); }
        }

        public static readonly DependencyProperty TemplateSelectorProperty =
            DependencyProperty.Register(
                "TemplateSelector",
                typeof(DataTemplateSelector),
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
        public DataTemplateSelector TemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(TemplateSelectorProperty); }
            set { SetValue(TemplateSelectorProperty, value); }
        }

        public static readonly DependencyProperty TemplateSelectorProperty =
            DependencyProperty.Register(
                "TemplateSelector",
                typeof(DataTemplateSelector),
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
