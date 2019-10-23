using JetBrains.Annotations;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Nemesis.Wpf.Converters
{
    [MarkupExtensionReturnType(typeof(BinaryConverter<>))]
    public abstract class BinaryConverter<TResult> : MarkupExtension, IMultiValueConverter
    {
        [ConstructorArgument("trueValue")]
        public TResult True { get; set; }
        [ConstructorArgument("falseValue")]
        public TResult False { get; set; }
        [ConstructorArgument("operation")]
        public BoolOperation Operation { get; set; }

        protected BinaryConverter(TResult trueValue, TResult falseValue, BoolOperation operation)
        {
            True = trueValue;
            False = falseValue;
            Operation = operation;
        }

        public object Convert([NotNull] object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));

            return ApplyOperator(values) ? True : False;
        }

        private bool ApplyOperator(object[] values)
        {
            switch (Operation)
            {
                case BoolOperation.And:
                    return values.All(ToBool);
                case BoolOperation.Or:
                    return values.Any(ToBool);
                case BoolOperation.Xor:
                    bool first = ToBool(values.First());
                    return values.Skip(1).Aggregate(first, (x, y) => ToBool(x) ^ ToBool(y));
                default:
                    throw new ArgumentOutOfRangeException(nameof(Operation));
            }
        }

        protected virtual bool ToBool(object o) => o is bool b ? b : bool.Parse(o?.ToString() ?? "false");

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture) => throw new NotSupportedException("Cannot convert back aggregated value");

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }

    public sealed class BoolMultiConverter : BinaryConverter<bool>
    {
        public BoolMultiConverter() : base(true, false, 0) { }

        public BoolMultiConverter(BoolOperation operation) : base(true, false, operation) { }
    }

    public sealed class VisibilityMultiConverter : BinaryConverter<Visibility>
    {
        public VisibilityMultiConverter() : base(Visibility.Visible, Visibility.Collapsed, 0) { }

        public VisibilityMultiConverter(BoolOperation operation) : base(Visibility.Visible, Visibility.Collapsed, operation) { }
    }

    public abstract class NotEmptyMultiConverter<TResult> : BinaryConverter<TResult>
    {
        protected NotEmptyMultiConverter(TResult trueValue, TResult falseValue, BoolOperation operation) : base(trueValue, falseValue, operation)
        {
        }

        protected sealed override bool ToBool(object o) => o is string text ? !string.IsNullOrWhiteSpace(text) : !(o is null);
    }

    public sealed class BoolNotEmptyMultiConverter : NotEmptyMultiConverter<bool>
    {
        public BoolNotEmptyMultiConverter() : base(true, false, 0) { }

        public BoolNotEmptyMultiConverter(BoolOperation operation) : base(true, false, operation) { }
    }

    public enum BoolOperation : byte
    {
        And,
        Or,
        Xor
    }
}
