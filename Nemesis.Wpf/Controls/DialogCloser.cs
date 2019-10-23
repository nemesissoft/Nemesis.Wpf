using System.Windows;
using JetBrains.Annotations;

namespace Nemesis.Wpf.Controls
{
    /// <summary><![CDATA[View:
    /// c:DialogCloser.DialogResult="{Binding DialogResult}"
    ///
    /// View-Model
    /// private bool? _dialogResult;
    /// public bool? DialogResult
    /// {
    ///     get => _dialogResult;
    ///     private set => SetProperty(ref _dialogResult, value);
    /// } ]]> </summary>
    public static class DialogCloser
    {
        public static readonly DependencyProperty DialogResultProperty =
            DependencyProperty.RegisterAttached("DialogResult", typeof(bool?), 
                typeof(DialogCloser), new PropertyMetadata(OnDialogResultChanged));

        private static void OnDialogResultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window)
                window.DialogResult = e.NewValue as bool?;
        }
        /// <summary>Helper for setting <see cref="DialogResultProperty"/> on <paramref name="target"/>.</summary>
        /// <param name="target"><see cref="Window"/> to set <see cref="DialogResultProperty"/> on.</param>
        /// <param name="value">DialogResult property value.</param>
        [UsedImplicitly]
        public static void SetDialogResult(Window target, bool? value) => 
            target.SetValue(DialogResultProperty, value);
    }
}
