using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Nemesis.Wpf.Bindings;
using Nemesis.Wpf.Controls;

namespace WpfPlayground.Tabs
{
    public partial class Layout : IPrototype
    {
        public Layout() => InitializeComponent();

        public string Header => "Layout";

        private void RectMouseDown(object sender, MouseButtonEventArgs e)
        {
            var rect = (Rectangle)sender;
            if (rect.Fill is SolidColorBrush solid)
            {
                var oldColor = solid.Color;
                if (oldColor.R < 30 && oldColor.G < 30 && oldColor.B < 30)
                    oldColor = Color.FromRgb((byte)~oldColor.R, (byte)~oldColor.G, (byte)~oldColor.B);

                rect.SetCurrentValue(Shape.FillProperty, new SolidColorBrush(ChangeLightness(oldColor, 0.25f)));
            }
        }

        private static Color ChangeLightness(Color color, float coef) =>
            Color.FromScRgb(color.ScA, MinMax(color.ScR, coef), MinMax(color.ScG, coef), MinMax(color.ScB, coef));

        private static float MinMax(float channel, float coef) => Math.Max(0.0f, Math.Min(1f, channel * coef));
    }

    internal class LayoutViewModel : ObservableObject
    {
        public Dictionary<int, string> SpliterItemsSource { get; } = Enumerable.Range(1, 10).ToDictionary(i => i * 1000, i => $"Item {i}");


        private GridUtils.AutoLayoutType _gridAutoLayoutType = GridUtils.AutoLayoutType.FillColumns;
        public GridUtils.AutoLayoutType GridAutoLayoutType
        {
            get => _gridAutoLayoutType;
            set => SetProperty(ref _gridAutoLayoutType, value);
        }

        public ICommand CycleLayoutCommand { get; }

        public LayoutViewModel() => CycleLayoutCommand = new ActionCommandWithParameter<Grid>(
            grid =>
            {
                var newLayout = (GridUtils.AutoLayoutType)(((int)GridAutoLayoutType + 1) % 3);
                GridUtils.AdjustCellSpans(grid, newLayout);
                GridAutoLayoutType = newLayout;
            },
            grid => grid != null);

        public IEnumerable<int> Numbers { get; } = Enumerable.Range(1, 60).ToList();
    }
}
