using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Nemesis.Wpf.Converters
{
    [MarkupExtensionReturnType(typeof(BaseConverter))]
    public abstract class BaseConverter : MarkupExtension, IValueConverter
    {
        public sealed override object ProvideValue(IServiceProvider serviceProvider) => this;
        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }

    [ValueConversion(typeof(object), typeof(object))]
    public class NullToUnsetValueConverter : BaseConverter
    {
        // Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
        // ReSharper disable once EmptyConstructor
        static NullToUnsetValueConverter() { }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value ?? DependencyProperty.UnsetValue;

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => DependencyProperty.UnsetValue;
    }

    [ValueConversion(typeof(object), typeof(Visibility))]
    public sealed class NullToVisibilityConverter : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value == null ? Visibility.Collapsed : Visibility.Visible;
    }
}
