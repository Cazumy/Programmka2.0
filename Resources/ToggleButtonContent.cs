using System.Windows;
using System.Windows.Media;

namespace Programmka
{
    public class ToggleButtonContent
    {
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.RegisterAttached(
            "ImageSource",
            typeof(ImageSource),
            typeof(ToggleButtonContent),
            new PropertyMetadata(default(ImageSource)));
        public static void SetImageSource(UIElement element, ImageSource value)
        {
            element.SetValue(ImageSourceProperty, value);
        }

        public static ImageSource GetImageSource(UIElement element)
        {
            return (ImageSource)element.GetValue(ImageSourceProperty);
        }
    }
}
