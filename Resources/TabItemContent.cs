using System.Windows;
using System.Windows.Media;

namespace Programmka
{
    public class TabItemContent
    {
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.RegisterAttached(
            "ImageSource",
            typeof(ImageSource),
            typeof(TabItemContent),
            new PropertyMetadata(default(ImageSource), OnImageSourceChanged));
        public static void SetImageSource(UIElement element, ImageSource value)
        {
            element.SetValue(ImageSourceProperty, value);
        }
        public static ImageSource GetImageSource(UIElement element)
        {
            return (ImageSource)element.GetValue(ImageSourceProperty);
        }
        private static void OnImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is System.Windows.Controls.TabItem tabItem)
            {
                tabItem.Background = e.NewValue is ImageSource newImageSource
                    ? new ImageBrush(newImageSource) { Stretch = Stretch.UniformToFill }
                    : null;
            }
        }
    }
}