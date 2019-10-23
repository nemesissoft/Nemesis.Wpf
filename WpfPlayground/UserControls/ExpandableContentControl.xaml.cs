using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfPlayground.UserControls
{
    public partial class ExpandableContentControl
    {
        public ExpandableContentControl() => InitializeComponent();

        private void ExpandButtonClick(object sender, RoutedEventArgs e)
        {
            var expandButton = (Button)sender;
            var mainWindow = VisualParentOfType<Window>(expandButton) ?? Application.Current.MainWindow;
            expandButton.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);


            var cc = (ContentControl)Template.FindName("ExpanderContentControl", this);
            var master = (Grid)Template.FindName("MainExpanderGrid", this);


            master.Children.Remove(cc);

            ShowControlInNewWindow(cc, mainWindow);

            master.Children.Insert(0, cc);
            expandButton.SetCurrentValue(VisibilityProperty, Visibility.Visible);
        }

        private static void ShowControlInNewWindow(UIElement control, Window mainWindow)
        {
            var dialogWindowGrid = new Grid();
            dialogWindowGrid.Children.Add(control);
            var dialogWindow = new MetroWindow
            {
                Content = dialogWindowGrid,
                ShowInTaskbar = false,
                ShowMinButton = false,
                IsMinButtonEnabled = false,
                ShowSystemMenuOnRightClick = false,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.CanResizeWithGrip,
                Owner = mainWindow,
                MinHeight = 200,
                MinWidth = 300,
                Height = 400,
                Width = 600,
            };

            dialogWindow.ShowDialog();
            dialogWindowGrid.Children.Remove(control);
        }

        private static TParentType VisualParentOfType<TParentType>(DependencyObject element) where TParentType : FrameworkElement
        {
            DependencyObject current = element;
            do
            {
                current = VisualTreeHelper.GetParent(current);
                if (current is TParentType parentType) return parentType;
            } while (current != null);

            return null;
        }
    }
}
