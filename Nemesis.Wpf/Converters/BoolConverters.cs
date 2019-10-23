using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Nemesis.Wpf.Converters
{
    [ValueConversion(typeof(object), typeof(bool))]
    public abstract class ToBoolConverter : BaseConverter
    {
        [ConstructorArgument("invert")]
        public bool Invert { get; set; }

        protected ToBoolConverter() { }

        protected ToBoolConverter(bool invert) => Invert = invert;

        public sealed override object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            CanConvert(value) ? (Invert ? !ToBool(value) : ToBool(value)) : Binding.DoNothing;

        protected virtual bool CanConvert(object @object) => true;

        protected abstract bool ToBool(object @object);
    }

    [ValueConversion(typeof(Nullable<>), typeof(bool))]
    public class NullableToBooleanConverter : ToBoolConverter
    {
        //protected override bool CanConvert(object @object) => @object == null || @object.GetType() is var type && (!type.IsValueType || Nullable.GetUnderlyingType(type) != null);
        protected override bool ToBool(object @object) => @object != null;
    }

    [ValueConversion(typeof(bool), typeof(object))]
    public abstract class FromBoolConverter<TTo> : BaseConverter
    {
        [ConstructorArgument("trueValue")]
        public TTo True { get; set; }

        [ConstructorArgument("falseValue")]
        public TTo False { get; set; }

        [ConstructorArgument("invert")]
        public bool Invert { get; set; }

        protected FromBoolConverter(TTo trueValue, TTo falseValue) : this(trueValue, falseValue, false) { }

        protected FromBoolConverter(TTo trueValue, TTo falseValue, bool invert)
        {
            True = trueValue;
            False = falseValue;
            Invert = invert;
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is bool b ?
                (Invert ? (b ? False : True) : (b ? True : False)) :
                False;

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is TTo variable && EqualityComparer<TTo>.Default.Equals(variable, Invert ? False : True);
    }

    /// <example><![CDATA[
    /// <Application.Resources>
    ///     <app:BooleanToVisibilityConverter x:Key="BooleanToVisibilityConverter" True="Collapsed" False="Visible" />
    /// </Application.Resources>
    /// ]]></example>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    [ValueConversion(typeof(bool?), typeof(Visibility))]
    public sealed class BooleanToVisibilityConverter : FromBoolConverter<Visibility>
    {
        public BooleanToVisibilityConverter() : base(Visibility.Visible, Visibility.Collapsed) { }

        public BooleanToVisibilityConverter(bool invert) : base(Visibility.Visible, Visibility.Collapsed, invert) { }
    }

    [ValueConversion(typeof(bool), typeof(string))]
    [ValueConversion(typeof(bool?), typeof(string))]
    public sealed class BooleanToStringConverter : FromBoolConverter<string>
    {
        public BooleanToStringConverter() : base("true", "false") { }

        public BooleanToStringConverter(bool invert) : base("true", "false", invert) { }
    }
}
