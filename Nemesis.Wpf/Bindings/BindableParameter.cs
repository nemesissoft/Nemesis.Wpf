using System;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Nemesis.Wpf.Bindings
{
    [ValueConversion(typeof(object), typeof(string))]
    [MarkupExtensionReturnType(typeof(SampleConverter))]
    public class SampleConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) =>
            value != null && parameter != null ? $"{value}, {parameter}" : null;

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) =>
            value is string text1 && parameter is string textParameter ? text1.Replace(textParameter, "") : value;

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }

    /// <summary>
    /// BindableParameter is the class that changes the ConverterParameter Value
    /// This must inherit from freezable so that it can be in the inheritance context and thus be able to use the DataContext and to specify ElementName binding as a ConverterParameter
    /// <see cref="http://www.drwpf.com/Blog/Default.aspx?tabid=36&EntryID=36"/>
    /// </summary>
    public class BindableParameter : Freezable
    {
        //this is a hack to trick the WPF platform in thinking that the binding is not sealed yet and then change the value of the converter parameter
        private static readonly FieldInfo _isSealedFieldInfo;

        public static readonly DependencyProperty ParameterProperty =
            DependencyProperty.Register(nameof(Parameter), typeof(object), typeof(BindableParameter),
                new FrameworkPropertyMetadata(null,
                    (d, e) =>
                    {
                        var param = (BindableParameter)d;
                        //set the ConverterParameterValue before calling invalidate because the invalidate uses that value to sett the converter parameter
                        param.ConverterParameterValue = e.NewValue;
                        //update the converter parameter 
                        InvalidateBinding(param);
                    }
                ));

        public object Parameter
        {
            get => GetValue(ParameterProperty);
            set => SetValue(ParameterProperty, value);
        }


        public static readonly DependencyProperty BindParameterProperty =
            DependencyProperty.RegisterAttached("BindParameter", typeof(BindableParameter), typeof(BindableParameter),
                new FrameworkPropertyMetadata(null,
                    OnBindParameterChanged));

        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static BindableParameter GetBindParameter(DependencyObject d) => (BindableParameter)d.GetValue(BindParameterProperty);

        public static void SetBindParameter(DependencyObject d, BindableParameter value) => d.SetValue(BindParameterProperty, value);

        private static void OnBindParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is FrameworkElement element))
                throw new InvalidOperationException("BindableParameter can be applied to a FrameworkElement only");

            var parameter = (BindableParameter)e.NewValue;

            element.Initialized += delegate
            {
                if (parameter != null)
                {
                    parameter.TargetExpression = BindingOperations.GetBindingExpression(element, parameter.TargetProperty);
                    parameter.TargetBinding = BindingOperations.GetBinding(element, parameter.TargetProperty);

                    InvalidateBinding(parameter); //update the converter parameter 
                }
            };
        }

        public object ConverterParameterValue { get; set; }

        public BindingExpression TargetExpression { get; set; }

        public Binding TargetBinding { get; private set; }

        public DependencyObject TargetObject { get; private set; }

        public DependencyProperty TargetProperty { get; internal set; }


        static BindableParameter()
        {
            //initialize the field info once
            _isSealedFieldInfo =
                typeof(BindingBase).GetField("_isSealed", BindingFlags.NonPublic | BindingFlags.Instance);

            if (_isSealedFieldInfo == null)
                throw new InvalidOperationException("Oops, we have a problem, it seems like the WPF team decided to change the name of the _isSealed field of the BindingBase class.");
        }

        private static void InvalidateBinding(BindableParameter param)
        {
            if (param.TargetBinding != null && param.TargetExpression != null)
            {
                //this is a hack to trick the WPF platform in thinking that the binding is not sealed yet and then change the value of the converter parameter
                bool isSealed = (bool)_isSealedFieldInfo.GetValue(param.TargetBinding);

                if (isSealed)//change the is sealed value
                    _isSealedFieldInfo.SetValue(param.TargetBinding, false);

                param.TargetBinding.ConverterParameter = param.ConverterParameterValue;

                if (isSealed)//put the is sealed value back as it was...
                    _isSealedFieldInfo.SetValue(param.TargetBinding, true);

                //TODO make binding one way
                if (param.TargetExpression.Status != BindingStatus.Detached)
                    param.TargetExpression.UpdateTarget();//force an update to the binding
            }
        }

        protected override Freezable CreateInstanceCore() => this;
    }

    /// <summary>
    /// Markup extension so that it is easier to create an instance of the BindableParameter from XAML
    /// </summary>
    [MarkupExtensionReturnType(typeof(BindableParameter))]
    public class BindableParameterExtension : MarkupExtension
    {
        /// <summary>
        /// Gets or sets the Dependency property you want to change the binding's ConverterParameter
        /// </summary>
        [ConstructorArgument("property")]
        public DependencyProperty TargetProperty { get; set; }

        /// <summary>
        /// Gets or sets the Binding that you want to use for the converter parameter
        /// </summary>
        public Binding Binding { get; set; }

        /// <summary>
        /// constructor that accepts a Dependency Property so that you do not need to specify TargetProperty
        /// </summary>
        /// <param name="property">The Dependency property you want to change the binding's ConverterParameter</param>
        public BindableParameterExtension(DependencyProperty property) => TargetProperty = property;

        public BindableParameterExtension() { }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            _bindableParam = new BindableParameter();
            //set the binding of the parameter
            BindingOperations.SetBinding(_bindableParam, BindableParameter.ParameterProperty, Binding);
            _bindableParam.TargetProperty = TargetProperty;
            return _bindableParam;
        }

        private BindableParameter _bindableParam;

    }
}