using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Nemesis.Wpf
{
    [PublicAPI]
    public static class ValidationHelper
    {
        public static IEnumerable<string> GetErrorMessagesFor(this INotifyDataErrorInfo notifyDataErrorInfo, string propertyName) =>
            notifyDataErrorInfo.GetErrors(propertyName)?.OfType<string>().ToList();

        public static bool HasErrorMessagesFor(this INotifyDataErrorInfo notifyDataErrorInfo, string propertyName) =>
            notifyDataErrorInfo.GetErrors(propertyName)?.OfType<string>() is IEnumerable<string> list && list.Any();

        public static void FocusFirstControlWithError(DependencyObject parent)
        {
            var errorControl = GetInputControlsWithErrors(parent).FirstOrDefault();

            if (errorControl != null)
            {
                if (errorControl is DependencyObject depObj && ParentOfType<Expander>(depObj) is var expander && expander?.IsExpanded == false)
                    expander.SetCurrentValue(Expander.IsExpandedProperty, true);

                FocusManager.SetFocusedElement(parent, errorControl);
            }
        }

        public static IEnumerable<IInputElement> GetInputControlsWithErrors(DependencyObject parent) =>
            from control in GetAllChildControls(parent)
            where control is IInputElement && control is DependencyObject element && Validation.GetHasError(element)
            select (IInputElement)control;

        public static IEnumerable<object> GetAllChildControls(object parent)
        {
            if (parent is DependencyObject depObj)
            {
                yield return depObj;
                foreach (var child in LogicalTreeHelper.GetChildren(depObj))
                    foreach (var ch in GetAllChildControls(child))
                        yield return ch;
            }
        }

        public static TParentType ParentOfType<TParentType>(DependencyObject element) where TParentType : FrameworkElement
        {
            DependencyObject current = element;
            do
            {
                current = LogicalTreeHelper.GetParent(current);
                if (current is TParentType parentType) return parentType;
            } while (current != null);
            
            return null;
        }
    }

    // ^(?<major>\d+)\.(?<minor>\d+)\.(?<build>\d+)\.(?<revision>\d+)$
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public sealed class VersionDataAttribute : DataTypeAttribute
    {
        public VersionDataAttribute() : base(DataType.Custom) { }

        public override bool IsValid(object value) =>
            value == null || IsValidVersion(value, out _);

        [ContractAnnotation("value:null=>false; =>true,version:notnull; =>false, version:null")]
        public static bool IsValidVersion(object value, out Version version)
        {
            version = null;
            return value is string text && Version.TryParse(text, out version) ||
                   value is IFormattable format &&
                   Version.TryParse(format.ToString(null, CultureInfo.InvariantCulture), out version) ||
                   value is var obj && obj != null && Version.TryParse(obj.ToString(), out version);
        }
    }
}
