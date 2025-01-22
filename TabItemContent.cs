using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace Programmka
{
    public class TabItemContent
    {
        // Регистрация привязанного свойства ImageSource
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.RegisterAttached(
            "ImageSource",
            typeof(ImageSource),
            typeof(TabItemContent),
            new PropertyMetadata(default(ImageSource), OnImageSourceChanged));

        // Метод для установки ImageSource
        public static void SetImageSource(UIElement element, ImageSource value)
        {
            element.SetValue(ImageSourceProperty, value);
        }

        // Метод для получения ImageSource
        public static ImageSource GetImageSource(UIElement element)
        {
            return (ImageSource)element.GetValue(ImageSourceProperty);
        }

        // Метод, вызываемый при изменении свойства ImageSource
        private static void OnImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TabItem tabItem)
            {
                // Установка фонового изображения через ImageBrush
                tabItem.Background = e.NewValue is ImageSource newImageSource
                    ? new ImageBrush(newImageSource) { Stretch = Stretch.UniformToFill } // Заполнение всего фона
                    : null; // Очистка фона, если изображение не задано
            }
        }
    }
}
