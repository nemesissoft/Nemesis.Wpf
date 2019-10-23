using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using JetBrains.Annotations;

namespace Nemesis.Wpf.Bindings
{
    public class ObservableObject : INotifyPropertyChanged
    {
        protected virtual bool SetProperty<T>(ref T member, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(member, value)) return false;

            member = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void SetProperty1<T>(ref T member, T value, string otherProperty1, [CallerMemberName] string propertyName = null)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            if (SetProperty(ref member, value, propertyName))
                OnPropertyChanged(otherProperty1);
        }

        protected void SetProperty2<T>(ref T member, T value, string otherProperty1, string otherProperty2, [CallerMemberName] string propertyName = null)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            if (SetProperty(ref member, value, propertyName))
            {
                OnPropertyChanged(otherProperty1);
                OnPropertyChanged(otherProperty2);
            }
        }

        protected void SetProperty3<T>(ref T member, T value, string otherProperty1, string otherProperty2, string otherProperty3, [CallerMemberName] string propertyName = null)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            if (SetProperty(ref member, value, propertyName))
            {
                OnPropertyChanged(otherProperty1);
                OnPropertyChanged(otherProperty2);
                OnPropertyChanged(otherProperty3);
            }
        }

        protected void SetProperty4<T>(ref T member, T value, string otherProperty1, string otherProperty2, string otherProperty3, string otherProperty4, [CallerMemberName] string propertyName = null)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            if (SetProperty(ref member, value, propertyName))
            {
                OnPropertyChanged(otherProperty1);
                OnPropertyChanged(otherProperty2);
                OnPropertyChanged(otherProperty3);
                OnPropertyChanged(otherProperty4);
            }
        }

        public static void Reset(object obj)
        {
            object GetSystemDefault(Type type)
            {
                if (type.IsGenericTypeDefinition) throw new ArgumentException($"Open generic type '{type.Name}' cannot be constructed");

                return !type.IsValueType || Nullable.GetUnderlyingType(type) is var underlyingType && underlyingType != null
                    ? null
                    : Activator.CreateInstance(type);
            }

            foreach (PropertyInfo prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead && p.CanWrite))
                prop.SetValue(obj, GetSystemDefault(prop.PropertyType));
        }

        public static T Clone<T>(T obj) where T : new()
        {
            var t = new T();
            foreach (PropertyInfo prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead && p.CanWrite))
                prop.SetValue(t, prop.GetValue(obj));

            return t;
        }

        public void CopyFromByNames(object other)
        {
            foreach (PropertyInfo prop in other.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead && p.CanWrite))
            {
                var otherValue = prop.GetValue(other);

                var localProperty = GetType().GetProperty(prop.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                localProperty?.SetValue(this, otherValue);
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
    }

    public class ValidableObservableObject : ObservableObject, INotifyDataErrorInfo
    {
        private readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged = delegate { };
        public System.Collections.IEnumerable GetErrors(string propertyName) => _errors.TryGetValue(propertyName, out var list) ? list : null;
        public bool HasErrors => _errors.Count > 0;

        protected override bool SetProperty<T>(ref T member, T value, [CallerMemberName] string propertyName = null)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            var isChanged = base.SetProperty(ref member, value, propertyName);
            //if (isChanged)
            ValidateProperty(propertyName, value);
            return isChanged;
        }

        private void ValidateProperty<T>(string propertyName, T value)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(this) { MemberName = propertyName };
            Validator.TryValidateProperty(value, context, results);

            if (results.Count > 0)
                AddToMultiDictionary(_errors, propertyName, results.Select(c => c.ErrorMessage));
            else if (_errors.ContainsKey(propertyName))
                _errors.Remove(propertyName);

            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        private void AddToMultiDictionary<TKeyType, TCollectionType, TMultiElement>(IDictionary<TKeyType, TCollectionType> multiDict,
            TKeyType key, IEnumerable<TMultiElement> elements)
            where TCollectionType : ICollection<TMultiElement>, new()
        {
            if (!multiDict.ContainsKey(key))
                multiDict[key] = new TCollectionType();

            TCollectionType coll = multiDict[key];

            foreach (var element in elements)
                coll.Add(element);
        }
    }
}
