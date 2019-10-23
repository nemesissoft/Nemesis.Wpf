using System;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Nemesis.Wpf
{
    [MarkupExtensionReturnType(typeof(float))]
    public sealed class FloatExtension : MarkupExtension
    {
        [ConstructorArgument("value")]
        public float Value { get; set; }

        public FloatExtension(float value) => Value = value;

        public override object ProvideValue(IServiceProvider sp) => Value;
    }

    [MarkupExtensionReturnType(typeof(double))]
    public sealed class DoubleExtension : MarkupExtension
    {
        [ConstructorArgument("value")]
        public double Value { get; set; }

        public DoubleExtension(double value) => Value = value;

        public override object ProvideValue(IServiceProvider sp) => Value;
    }

    [MarkupExtensionReturnType(typeof(AccessText))]
    public sealed class AccessTextExtension : MarkupExtension
    {
        [ConstructorArgument("text")]
        public string Text { get; set; }

        public AccessTextExtension(string text) => Text = text;

        public override object ProvideValue(IServiceProvider sp) => new AccessText { Text = Text };
    }
}
