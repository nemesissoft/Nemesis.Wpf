using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Nemesis.Wpf.Bindings
{
    public sealed class ValidatingBinding : Binding
    {
        public ValidatingBinding() => SetDefaults();

        public ValidatingBinding(string path) : base(path) => SetDefaults();

        public void SetDefaults()
        {
            NotifyOnValidationError = true;
            ValidatesOnDataErrors = true;
            //ValidatesOnNotifyDataErrors = true;
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            Mode = BindingMode.TwoWay;
        }
    }

    public sealed class ValidatingDelayedBinding : Binding
    {
        public ValidatingDelayedBinding() => SetDefaults();

        public ValidatingDelayedBinding(string path) : base(path) => SetDefaults();

        public void SetDefaults()
        {
            NotifyOnValidationError = true;
            ValidatesOnDataErrors = true;
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            Mode = BindingMode.TwoWay;
            Delay = 250;
        }
    }

    public sealed class InstantBinding : Binding
    {
        public InstantBinding() => SetDefaults();

        public InstantBinding(string path) : base(path) => SetDefaults();

        private void SetDefaults()
        {
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            Mode = BindingMode.TwoWay;
        }
    }

    public class SwitchBindingExtension : Binding
    {
        public SwitchBindingExtension() => Initialize();

        public SwitchBindingExtension(string path) : base(path) => Initialize();

        public SwitchBindingExtension(string path, object valueIfTrue, object valueIfFalse) : base(path)
        {
            Initialize();
            ValueIfTrue = valueIfTrue;
            ValueIfFalse = valueIfFalse;
        }

        private void Initialize()
        {
            ValueIfTrue = DoNothing;
            ValueIfFalse = DoNothing;
            Converter = new SwitchConverter(this);
        }

        [ConstructorArgument("valueIfTrue")]
        public object ValueIfTrue { get; set; }

        [ConstructorArgument("valueIfFalse")]
        public object ValueIfFalse { get; set; }

        private class SwitchConverter : IValueConverter
        {
            private readonly SwitchBindingExtension _switch;
            public SwitchConverter(SwitchBindingExtension switchExtension) => _switch = switchExtension;

            #region IValueConverter Members

            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                try
                {
                    return System.Convert.ToBoolean(value) ? _switch.ValueIfTrue : _switch.ValueIfFalse;
                }
                catch
                {
                    return DependencyProperty.UnsetValue;
                }
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => DoNothing;

            #endregion
        }

    }
}
