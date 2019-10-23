using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Media;

namespace Nemesis.Wpf.Converters
{
    [ValueConversion(typeof(IFormattable), typeof(SolidColorBrush))]
    public sealed class InvertColorConverter : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = value is string text
                ? ColorConverter.ConvertFromString(text) as Color?
                : (value is PropertyInfo pi
                    ? ColorConverter.ConvertFromString(pi.Name) as Color?
                    : (value is SolidColorBrush solidColorBrush
                        ? solidColorBrush.Color
                        : (value is Color c ? (Color?)c : null)));

            if (!color.HasValue)
                throw new ArgumentException($@"value in this form is not supported:{value} of type {value?.GetType().FullName}", nameof(value));

            color = InvertColor(color.Value);

            if (typeof(Color) == targetType)
                return color.Value;
            else if (typeof(string) == targetType)
                return $"#{color.Value.A:00}{color.Value.R:00}{color.Value.G:00}{color.Value.B:00}";
            else if (typeof(Brush).IsAssignableFrom(targetType))
                return new SolidColorBrush(color.Value);
            else
                throw new ArgumentException($@"targetType is not supported:{targetType?.FullName}", nameof(targetType));
        }

        private static Color InvertColor(Color c) => Color.FromArgb(c.A, (byte)(255 - c.R), (byte)(255 - c.G), (byte)(255 - c.B));

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Convert(value, targetType, parameter, culture);
    }

    [ValueConversion(typeof(IFormattable), typeof(SolidColorBrush))]
    public sealed class HueColorConverter : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            float darken =
                parameter is float f ? f :
                    (parameter is double d ? (float)d :
                        (parameter is string text ? float.Parse(text, CultureInfo.InvariantCulture) : 1f));
            if (value == null) return null;
            if (value is SolidColorBrush solidColorBrush)
                return new SolidColorBrush(ChangeLightness(solidColorBrush.Color, darken));
            else if (value is Color color)
                return new SolidColorBrush(ChangeLightness(color, darken));
            return null;
        }

        private static Color ChangeLightness(Color color, float coefficient) =>
            Color.FromScRgb(color.ScA, MinMax(color.ScR, coefficient), MinMax(color.ScG, coefficient), MinMax(color.ScB, coefficient));

        private static float MinMax(float channel, float coef) => Math.Max(0.0f, Math.Min(1.0f, channel * coef));
    }
}
