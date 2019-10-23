using JetBrains.Annotations;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;

namespace Nemesis.Wpf.Bindings
{
    public interface IDelegateCommand : ICommand
    {
        [UsedImplicitly]
        void RaiseCanExecuteChanged();
    }

    public sealed class ActionCommand : IDelegateCommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public ActionCommand([NotNull] Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        bool ICommand.CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        void ICommand.Execute(object parameter) => _execute.Invoke();

        public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
    }

    public class ActionCommandWithParameter<TParameter> : IDelegateCommand
    {
        private readonly Action<TParameter> _execute;
        private readonly Func<TParameter, bool> _canExecute;

        public ActionCommandWithParameter(Action<TParameter> execute, Func<TParameter, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        bool ICommand.CanExecute(object parameter) => _canExecute?.Invoke(ConvertParameter(parameter)) ?? true;

        void ICommand.Execute(object parameter) => _execute.Invoke(ConvertParameter(parameter));

        protected virtual TParameter ConvertParameter(object parameter) => (TParameter)parameter;

        public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
    }

    public sealed class ActionCommandWithConvertibleParameter<TParameter> : ActionCommandWithParameter<TParameter>
    {
        public ActionCommandWithConvertibleParameter(Action<TParameter> execute, Func<TParameter, bool> canExecute = null) : base(execute, canExecute) { }

        protected override TParameter ConvertParameter([NotNull] object parameter)
        {
            if (parameter == null) throw new ArgumentNullException(nameof(parameter));

            var converter = TypeDescriptor.GetConverter(typeof(TParameter));
            return converter.CanConvertFrom(parameter.GetType())
                // ReSharper disable once AssignNullToNotNullAttribute
                ? (TParameter)converter.ConvertFrom(null, CultureInfo.InvariantCulture, parameter)
                : (TParameter)converter.ConvertFromInvariantString(parameter.ToString());
        }
    }

    public class SimpleDelegateCommandWithParameter<TParameter> : ICommand
    {
        public Key GestureKey { get; set; }
        public ModifierKeys GestureModifier { get; set; }
        public MouseAction MouseGesture { get; set; }

        private readonly Action<TParameter> _executeDelegate;

        public SimpleDelegateCommandWithParameter([NotNull] Action<TParameter> executeDelegate) => _executeDelegate = executeDelegate ?? throw new ArgumentNullException(nameof(executeDelegate));

        public void Execute(object parameter) => _executeDelegate((TParameter)parameter);

        public bool CanExecute(object parameter) => true;

        [UsedImplicitly]
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
