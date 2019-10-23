using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace Nemesis.Wpf.Converters
{
    #region Value types
    //[ValueConversion(typeof(eDouble), typeof(double))]
    //[ValueConversion(typeof(eDouble?), typeof(double?))]

    [ValueConversion(typeof(double), typeof(double))]
    [ValueConversion(typeof(float), typeof(double))]
    [ValueConversion(typeof(decimal), typeof(double))]
    [ValueConversion(typeof(int), typeof(double))]
    [ValueConversion(typeof(uint), typeof(double))]
    [ValueConversion(typeof(short), typeof(double))]
    [ValueConversion(typeof(ushort), typeof(double))]
    [ValueConversion(typeof(byte), typeof(double))]
    [ValueConversion(typeof(sbyte), typeof(double))]
    [ValueConversion(typeof(long), typeof(double))]
    [ValueConversion(typeof(ulong), typeof(double))]

    [ValueConversion(typeof(double?), typeof(double?))]
    [ValueConversion(typeof(float?), typeof(double?))]
    [ValueConversion(typeof(decimal?), typeof(double?))]
    [ValueConversion(typeof(int?), typeof(double?))]
    [ValueConversion(typeof(uint?), typeof(double?))]
    [ValueConversion(typeof(short?), typeof(double?))]
    [ValueConversion(typeof(ushort?), typeof(double?))]
    [ValueConversion(typeof(byte?), typeof(double?))]
    [ValueConversion(typeof(sbyte?), typeof(double?))]
    [ValueConversion(typeof(long?), typeof(double?))]
    [ValueConversion(typeof(ulong?), typeof(double?))]
    #endregion
    public class NumberToDoubleValueConverter : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Nullable.GetUnderlyingType(targetType) != null ? null : Binding.DoNothing;

            switch (value)
            {
                case double d: return d;
                case float f: return (double)f;
                case decimal m: return (double)m;
                case int i: return (double)i;
                case uint ui: return (double)ui;
                case short s: return (double)s;
                case ushort us: return (double)us;
                case byte b: return (double)b;
                case sbyte sb: return (double)sb;
                case long l: return (double)l;
                case ulong ul: return (double)ul;

                //case eDouble ed: return (double)ed;
                default: return Binding.DoNothing;
            }
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(object) && parameter != null)
                targetType = parameter is PropertyInfo pi ? pi.PropertyType : (parameter is Type t ? t : typeof(object));

            if (value == null)
                return Nullable.GetUnderlyingType(targetType) != null ? null : Binding.DoNothing;

            if (value is double d)
                switch (Type.GetTypeCode(targetType))
                {
                    case TypeCode.Double:
                        return d;
                    case TypeCode.Single:
                        return (float)d;
                    case TypeCode.Decimal:
                        return (decimal)d;

                    case TypeCode.Int32:
                        return (int)d;
                    case TypeCode.UInt32:
                        return (uint)d;

                    case TypeCode.Int16:
                        return (short)d;
                    case TypeCode.UInt16:
                        return (ushort)d;

                    case TypeCode.Byte:
                        return (byte)d;
                    case TypeCode.SByte:
                        return (sbyte)d;

                    case TypeCode.Int64:
                        return (long)d;
                    case TypeCode.UInt64:
                        return (ulong)d;

                    case TypeCode.Object:
                        return
                            //targetType == typeof(eDouble) || targetType == typeof(eDouble?) ? new eDouble(d) : 
                            value;

                    default:
                        return Binding.DoNothing;
                }
            else
                return Binding.DoNothing;
        }
    }

    [ValueConversion(typeof(char), typeof(string))]
    [ValueConversion(typeof(char?), typeof(string))]
    public class CharToStringValueConverter : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            return value is char c ? c.ToString() : Binding.DoNothing;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(object) && parameter != null)
                targetType = parameter is PropertyInfo pi ? pi.PropertyType : (parameter is Type t ? t : typeof(object));

            switch (value)
            {
                case null:
                    return targetType == typeof(char?) ? null : (object)'\0';
                case string text:
                    return text.Length > 0 ? text[0] : '\0';
                default: return Binding.DoNothing;
            }
        }
    }

    #region Value types
    //[ValueConversion(typeof(TimeInMinutes), typeof(TimeSpan))]
    //[ValueConversion(typeof(TimeInMinutes?), typeof(TimeSpan?))]

    //[ValueConversion(typeof(TimeHhMm), typeof(TimeSpan))]
    //[ValueConversion(typeof(TimeHhMm?), typeof(TimeSpan?))]

    [ValueConversion(typeof(TimeSpan), typeof(TimeSpan))]
    [ValueConversion(typeof(TimeSpan?), typeof(TimeSpan?))]
    #endregion
    public class TimeInMinutesValueConverter : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return targetType == typeof(TimeSpan) ? (object)TimeSpan.Zero : null;

            if (value is TimeSpan ts) return ts;
            //else if (value is TimeInMinutes tim) return tim.RealTimeSpan;
            //else if (value is TimeHhMm hhmm) return hhmm.RealTimeSpan;
            else
                return Binding.DoNothing;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(object) && parameter != null)
                targetType = parameter is PropertyInfo pi ? pi.PropertyType : (parameter is Type t ? t : typeof(object));


            if (value == null)
            {
                if (Nullable.GetUnderlyingType(targetType) != null) return null;

                else if (targetType == typeof(TimeSpan)) return TimeSpan.Zero;
                //else if (targetType == typeof(TimeInMinutes)) return new TimeInMinutes(TimeSpan.Zero);
                //else if (targetType == typeof(TimeHhMm)) return new TimeHhMm(TimeSpan.Zero);

                else
                    return Binding.DoNothing;
            }

            if (value is TimeSpan ts)
            {
                if (targetType == typeof(TimeSpan) || targetType == typeof(TimeSpan?)) return ts;
                //else if (targetType == typeof(TimeInMinutes) || targetType == typeof(TimeInMinutes?)) return new TimeInMinutes(ts);
                //else if (targetType == typeof(TimeHhMm) || targetType == typeof(TimeHhMm?)) return new TimeHhMm(ts);

                else
                    return Binding.DoNothing;
            }
            else
                return Binding.DoNothing;
        }
    }

    [ValueConversion(typeof(string), typeof(string))]
    public sealed class FilterTooltipConverter : BaseConverter
    {
        private const string LIKE_PATTERN_CHAR = "%";

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value?.ToString() ?? "No filter";
            if (string.IsNullOrWhiteSpace(text)) return null;

            text = text.Trim();

            if (string.Equals(text, LIKE_PATTERN_CHAR, StringComparison.Ordinal))
                return $"Any value";

            var startsWith = text.StartsWith(LIKE_PATTERN_CHAR, StringComparison.Ordinal);
            bool endsWith = text.EndsWith(LIKE_PATTERN_CHAR, StringComparison.Ordinal);
            int firstInstance = text.IndexOf(LIKE_PATTERN_CHAR, StringComparison.Ordinal);
            int lastInstance = text.LastIndexOf(LIKE_PATTERN_CHAR, StringComparison.Ordinal);


            bool endsWithPattern = startsWith && text.IndexOf(LIKE_PATTERN_CHAR, 1, StringComparison.Ordinal) < 0;
            bool startsWithPattern = endsWith && text.IndexOf(LIKE_PATTERN_CHAR, 0, text.Length - 1, StringComparison.Ordinal) < 0;
            bool containsWithPattern = startsWith && endsWith && text.Length > 2 && text.IndexOf(LIKE_PATTERN_CHAR, 1, text.Length - 2, StringComparison.Ordinal) < 0;

            if (endsWithPattern)
                return $"Ends with '{text.Substring(1)}'";

            else if (startsWithPattern)
                return $"Starts with '{text.Substring(0, text.Length - 1)}'";

            else if (containsWithPattern)
                return $"Contains '{text.Substring(1, text.Length - 2)}'";

            else if (firstInstance > -1 && firstInstance == lastInstance)
                return $"Contains something between '{text.Substring(0, firstInstance)}' AND '{text.Substring(firstInstance + 1)}'";

            else if (firstInstance < 0)
                return $"Equals '{text}'";

            return $"Is LIKE '{text}'";
        }
    }

    [ValueConversion(typeof(object), typeof(object))]
    public class UniversalValueConverter : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var converter = TypeDescriptor.GetConverter(targetType);
            try
            {
                return value != null && converter.CanConvertFrom(value.GetType())
                    ? converter.ConvertFrom(value)
                    : converter.ConvertFromInvariantString(value?.ToString());
            }
            catch (Exception)
            {
                return value;
            }
        }
    }

    public static class UniversalValueConverterHelper
    {
        public static void SetValueEx(this DependencyObject element, DependencyProperty property, object value)
        {
            var conv = new UniversalValueConverter();
            var convertedValue = conv.Convert(value, property.PropertyType, null, CultureInfo.InvariantCulture);
            element.SetValue(property, convertedValue);
        }
    }

    [ValueConversion(typeof(Version), typeof(string))]
    public class VersionConverter : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is Version ver ? ver.ToString() : DependencyProperty.UnsetValue;

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            VersionDataAttribute.IsValidVersion(value, out var v) ? v : DependencyProperty.UnsetValue;
    }

    //TODO implement <> >= != etc, multiply, divide etc
    [ValueConversion(typeof(double), typeof(bool))]
    public class IsValueLessThanParameter : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is double actual && parameter is string expectedText &&
            double.TryParse(expectedText, NumberStyles.Any, CultureInfo.InvariantCulture, out _) &&
            double.Parse(expectedText, NumberStyles.Any, CultureInfo.InvariantCulture) is var expected &&
            actual < expected;
    }

    [ValueConversion(typeof(double), typeof(double))]
    public class MultiplyByConverter : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is double actual && parameter is double number ?
            actual * number :
            value;
    }

    [ValueConversion(typeof(bool), typeof(GridLength))]
    public class GridLenConverter : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is bool b && b ?
            GridLength.Auto :
            new GridLength(1, GridUnitType.Star);
    }

    [ValueConversion(typeof(bool), typeof(GridLength))]
    public class InvertGridLenConverter : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is bool b && b ?
            new GridLength(1, GridUnitType.Star) :
            GridLength.Auto
        ;
    }
}
