using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Nemesis.Wpf.Bindings;
using WpfPlayground.Tabs;

namespace WpfPlayground
{
    public partial class MainWindow 
    {
        public MainWindow() => InitializeComponent();
    }
    
    internal class MainWindowViewModel : ObservableObject
    {
        public ObservableCollection<TabItem> TabItems { get; set; }
        public MainWindowViewModel()
        {
            var prototypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => !t.IsInterface && !t.IsAbstract && !t.IsGenericTypeDefinition &&
                            typeof(IPrototype).IsAssignableFrom(t))
                .Select(t => (IPrototype)Activator.CreateInstance(t))
                .Select(ip => new TabItem(ip.Header, ip));

            TabItems = new ObservableCollection<TabItem>(prototypes);
        }
    }

    public class TabItem
    {
        public string Header { get; set; }
        public object Content { get; set; }

        public TabItem(string header, object content)
        {
            Header = header;
            Content = content;
        }
    }
}
