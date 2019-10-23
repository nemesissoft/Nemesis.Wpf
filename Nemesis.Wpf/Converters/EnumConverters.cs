using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Markup;

namespace Nemesis.Wpf.Converters
{

#pragma warning disable WPF0081 // MarkupExtensionReturnType must use correct return type.
    /// <example><![CDATA[
    /// <ListBox ItemsSource="{Binding Source={commons:EnumBindingSource {x:Type media:GeometryCombineMode}}}" 
    /// DisplayMemberPath="Name" SelectedValuePath="Value"
    /// SelectionMode="Single" SelectedValue="{Binding CombinedMode}"/>
    /// ]]></example>
    [MarkupExtensionReturnType(typeof(List<object>))]
#pragma warning restore WPF0081 // MarkupExtensionReturnType must use correct return type.
    public class EnumBindingSourceExtension : MarkupExtension
    {
        private Type _enumType;
        public Type EnumType
        {
            get => _enumType;
            set
            {
                if (value != _enumType)
                {
                    if (null != value && !(Nullable.GetUnderlyingType(value) ?? value).IsEnum)
                        throw new ArgumentException("Type must be for an Enum.");
                    _enumType = value;
                }
            }
        }

        public EnumBindingSourceExtension(Type enumType) => EnumType = enumType ?? throw new InvalidOperationException("The EnumType must be specified.");

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (null == _enumType)
                throw new InvalidOperationException("The EnumType must be specified.");

            return GetEnumMeta(_enumType);
        }

        public static IList<object> GetEnumMeta(Type enumType)
        {
            Type actualEnumType = Nullable.GetUnderlyingType(enumType) ?? enumType;
            Array enumValues = Enum.GetValues(actualEnumType);

            var metaType = typeof(EnumerationMeta<>).MakeGenericType(actualEnumType);
            var ctor = metaType.GetConstructors().OrderBy(c => c.GetParameters().Length).Last();

            return enumValues.Cast<object>().Select(enumValue =>
                ctor.Invoke(new[]{enumValue.ToString(),
                    actualEnumType.GetField(enumValue.ToString()).GetCustomAttribute<DescriptionAttribute>()?.Description ?? "",
                    enumValue})
            ).ToList();
        }

        public struct EnumerationMeta<TEnum> where TEnum : Enum
        {
            public string Name { get; }
            public string Description { get; }
            public TEnum Value { get; }

            public EnumerationMeta(string name, string description, TEnum value)
            {
                Name = name;
                Description = description;
                Value = value;
            }
        }
    }

    [ValueConversion(typeof(Type), typeof(IList<object>))]
    public sealed class EnumToListConverter : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is Type type && type.IsEnum ? EnumBindingSourceExtension.GetEnumMeta(type) : Binding.DoNothing;

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
}
