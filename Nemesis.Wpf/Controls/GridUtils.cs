using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Nemesis.Wpf.Converters;

namespace Nemesis.Wpf.Controls
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    internal class GridUtils
    {
        #region Auto layout 

        public enum AutoLayoutType : byte
        {
            None,
            FillRows,
            FillColumns
        }

        public enum AutoLayoutOptions : byte
        {
            Standard,
            Skip1,
            Skip2,
            Skip3,
            Skip4,
            Skip5,
            Skip6,
            Skip7,
            Skip8,
            Skip9,
            Skip10,
            DoNotAdvance,
        }

        [AttachedPropertyBrowsableForType(typeof(UIElement))]
        public static AutoLayoutOptions GetAutoLayoutOptions(DependencyObject obj) => (AutoLayoutOptions)obj.GetValue(AutoLayoutOptionsProperty);
        public static void SetAutoLayoutOptions(DependencyObject obj, AutoLayoutOptions value) => obj.SetValue(AutoLayoutOptionsProperty, value);
        public static readonly DependencyProperty AutoLayoutOptionsProperty = DependencyProperty.RegisterAttached("AutoLayoutOptions", typeof(AutoLayoutOptions), typeof(GridUtils), new FrameworkPropertyMetadata(AutoLayoutOptions.Standard));


        [AttachedPropertyBrowsableForType(typeof(Grid))]
        public static AutoLayoutType GetAutoLayoutType(DependencyObject obj) => (AutoLayoutType)obj.GetValue(AutoLayoutTypeProperty);
        public static void SetAutoLayoutType(DependencyObject obj, AutoLayoutType value) => obj.SetValue(AutoLayoutTypeProperty, value);
        public static readonly DependencyProperty AutoLayoutTypeProperty = DependencyProperty.RegisterAttached("AutoLayoutType", typeof(AutoLayoutType), typeof(GridUtils), new FrameworkPropertyMetadata(AutoLayoutType.None, OnAutoLayoutTypeChanged));

        private static void OnAutoLayoutTypeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            void GridLoaded(object sender, EventArgs ea)
            {
                if (sender is Grid grd)
                {
                    grd.Loaded -= GridLoaded;
                    PerformAutoLayout(grd, GetAutoLayoutType(grd));
                }
            }

            if (dependencyObject is Grid grid && e.NewValue is AutoLayoutType newValue)
            {
                if (newValue == AutoLayoutType.None)
                    PerformAutoLayout(grid, AutoLayoutType.None);
                else
                {
                    if (grid.IsLoaded)
                        PerformAutoLayout(grid, newValue);
                    else
                        grid.Loaded += GridLoaded;
                }
            }
            else
                throw new NotSupportedException(
                    "AutoLayoutTypeProperty is supported only for grids and for values of type AutoLayoutType");
        }

        public static void AdjustCellSpans(Grid grid, AutoLayoutType desiredLayoutType)
        {
            if (desiredLayoutType == AutoLayoutType.FillColumns) //only RowSpan
                foreach (var child in grid.Children.Cast<UIElement>())
                {
                    var rowSpan = (int)(child.GetValue(Grid.RowSpanProperty) ?? 1);
                    var colSpan = (int)(child.GetValue(Grid.ColumnSpanProperty) ?? 1);
                    if (rowSpan == 1 && colSpan > 1)
                        child.SetCurrentValue(Grid.RowSpanProperty, colSpan);
                    child.ClearValue(Grid.ColumnSpanProperty);
                }
            else if (desiredLayoutType == AutoLayoutType.FillRows) //only ColSpan
                foreach (var child in grid.Children.Cast<UIElement>())
                {
                    var rowSpan = (int)(child.GetValue(Grid.RowSpanProperty) ?? 1);
                    var colSpan = (int)(child.GetValue(Grid.ColumnSpanProperty) ?? 1);
                    if (rowSpan > 1 && colSpan == 1)
                        child.SetCurrentValue(Grid.ColumnSpanProperty, rowSpan);
                    child.ClearValue(Grid.RowSpanProperty);
                }
        }

        public static void PerformAutoLayout(Grid grid, AutoLayoutType layoutType)
        {
            foreach (var child in grid.Children.Cast<UIElement>())
            {
                child.ClearValue(Grid.RowProperty);
                child.ClearValue(Grid.ColumnProperty);
            }

            if (layoutType == AutoLayoutType.None) return;

            var colCount = grid.ColumnDefinitions.Count;
            var rowCount = grid.RowDefinitions.Count;
            if (colCount == 0 && rowCount == 0)
                throw new NotSupportedException("AutoLayout is only supported for grids with ColumnDefinitions and/or RowDefinitions specified");

            colCount = Math.Max(colCount, 1);
            rowCount = Math.Max(rowCount, 1);
            int colIndex = 0, rowIndex = 0;

            foreach (var child in grid.Children.Cast<UIElement>())
            {
                Grid.SetRow(child, rowIndex);
                Grid.SetColumn(child, colIndex);

                var layoutOptions = GetAutoLayoutOptions(child);

                if (layoutOptions == AutoLayoutOptions.DoNotAdvance) continue;

                byte skip;
                switch (layoutOptions)
                {
                    case AutoLayoutOptions.Skip1:
                    case AutoLayoutOptions.Skip2:
                    case AutoLayoutOptions.Skip3:
                    case AutoLayoutOptions.Skip4:
                    case AutoLayoutOptions.Skip5:
                    case AutoLayoutOptions.Skip6:
                    case AutoLayoutOptions.Skip7:
                    case AutoLayoutOptions.Skip8:
                    case AutoLayoutOptions.Skip9:
                    case AutoLayoutOptions.Skip10:
                        skip = (byte)layoutOptions;
                        break;
                    default:
                        skip = 0; break;
                }


                var rowSpan = (int)(child.GetValue(Grid.RowSpanProperty) ?? 1);
                var colSpan = (int)(child.GetValue(Grid.ColumnSpanProperty) ?? 1);

                if (layoutType == AutoLayoutType.FillRows)//only ColSpan
                {
                    //child.ClearValue(Grid.RowSpanProperty);

                    colIndex += colSpan + skip;
                    if (colIndex >= colCount)
                    {
                        rowIndex += colIndex / colCount;
                        colIndex %= colCount;
                    }
                }
                else if (layoutType == AutoLayoutType.FillColumns)//only RowSpan
                {
                    //child.ClearValue(Grid.ColumnSpanProperty);

                    rowIndex += rowSpan + skip;
                    if (rowIndex >= rowCount)
                    {
                        colIndex += rowIndex / rowCount;
                        rowIndex %= rowCount;
                    }
                }
            }
        }

        #endregion

        #region RowDefinitions/ColumnDefinitions

        public static readonly DependencyProperty RowDefinitionsProperty =
            DependencyProperty.RegisterAttached("RowDefinitions", typeof(string), typeof(GridUtils),
                new PropertyMetadata("", OnRowDefinitionsChanged));

        public static readonly DependencyProperty ColumnDefinitionsProperty =
            DependencyProperty.RegisterAttached("ColumnDefinitions", typeof(string), typeof(GridUtils),
                new PropertyMetadata("", OnColumnDefinitionsChanged));

        [AttachedPropertyBrowsableForType(typeof(Grid))]
        public static string GetRowDefinitions(DependencyObject d) => (string)d.GetValue(RowDefinitionsProperty);
        public static void SetRowDefinitions(DependencyObject d, string value) => d.SetValue(RowDefinitionsProperty, value);

        [AttachedPropertyBrowsableForType(typeof(Grid))]
        public static string GetColumnDefinitions(DependencyObject d) => (string)d.GetValue(ColumnDefinitionsProperty);
        public static void SetColumnDefinitions(DependencyObject d, string value) => d.SetValue(ColumnDefinitionsProperty, value);

        private static void OnRowDefinitionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Grid targetGrid && e.NewValue is string rowDefinitions)
            {
                targetGrid.RowDefinitions.Clear();

                //var definitions = SpanCellDefinitionParser.ParseDefinitions(rowDefinitions);
                var definitions = StandardCellDefinitionParser.Instance.ParseDefinitions(rowDefinitions);
                foreach (var (length, sharedSizeGroup, count) in definitions)
                {
                    for (byte i = 0; i < count; i++)
                        targetGrid.RowDefinitions.Add(new RowDefinition { Height = length, SharedSizeGroup = sharedSizeGroup });
                }
            }
            else
                throw new NotSupportedException("RowDefinitions property is only supported for Grid with value of string");
        }

        private static void OnColumnDefinitionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Grid targetGrid && e.NewValue is string columnDefinitions)
            {
                targetGrid.ColumnDefinitions.Clear();

                //var definitions = SpanCellDefinitionParser.ParseDefinitions(columnDefinitions);
                var definitions = StandardCellDefinitionParser.Instance.ParseDefinitions(columnDefinitions);
                foreach (var (length, sharedSizeGroup, count) in definitions)
                {
                    for (byte i = 0; i < count; i++)
                        targetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = length, SharedSizeGroup = sharedSizeGroup });
                }
            }
            else
                throw new NotSupportedException("ColumnDefinitions property is only supported for Grid with value of string");
        }

        #endregion

        #region Auto target 

        [AttachedPropertyBrowsableForType(typeof(Grid))]
        public static bool GetAutoTargetForLabels(DependencyObject obj) => (bool)obj.GetValue(AutoTargetForLabelsProperty);

        public static void SetAutoTargetForLabels(DependencyObject obj, bool value) => obj.SetValue(AutoTargetForLabelsProperty, value);

        public static readonly DependencyProperty AutoTargetForLabelsProperty = DependencyProperty.RegisterAttached("AutoTargetForLabels", typeof(bool), typeof(GridUtils),
            new FrameworkPropertyMetadata(false, OnAutoTargetForLabelsChanged));

        private static void OnAutoTargetForLabelsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            void GridLoaded(object sender, EventArgs ea)
            {
                if (sender is Grid grd)
                    UpdateLabelTargets(grd);
            }

            if (dependencyObject is Grid grid && e.NewValue is bool shouldUpdateAutoTarget)
            {
                if (shouldUpdateAutoTarget)
                    grid.Loaded += GridLoaded;
                else
                    grid.Loaded -= GridLoaded;

                if (grid.IsLoaded && shouldUpdateAutoTarget)
                    UpdateLabelTargets(grid);
            }
            else
                throw new NotSupportedException(
                    "AutoLayoutTypeProperty is supported only for grids and for values of type AutoLayoutType");
        }

        public static void UpdateLabelTargets(Grid grid)
        {
            for (int i = 0; i < grid.Children.Count; i++)
            {
                var child = grid.Children[i];
                if (child is Label label && i + 1 < grid.Children.Count)
                    label.SetCurrentValue(Label.TargetProperty, grid.Children[i + 1]);
            }
        }

        #endregion
    }

    [ValueConversion(typeof(DefinitionBase), typeof(int))]
    public class GridDefinitionConverter : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DefinitionBase definition && definition.Parent is Grid grid)
            {
                if (definition is RowDefinition rd)
                    return grid.RowDefinitions.IndexOf(rd);

                if (definition is ColumnDefinition cd)
                    return grid.ColumnDefinitions.IndexOf(cd);
            }
            return 0;
        }
    }
}