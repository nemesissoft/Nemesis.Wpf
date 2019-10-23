using JetBrains.Annotations;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Nemesis.Wpf.Controls
{
    public class ClipboardMonitor : UIElement, INotifyPropertyChanged
    {
        private IntPtr _handle;
        private HwndSource _source;
        private IntPtr _nextClipboardViewer;

        /*private Window _window;
        public Window Window
        {
            get => _window;
            set
            {
                if (Equals(value, _window)) return;
                _window = value;
                _window.Closed += (sender, e) =>
                {
                    ChangeClipboardChain(_handle, _nextClipboardViewer);
                    _source?.RemoveHook(WndProc);
                };

                _window.SourceInitialized += (sender1, e1) =>
                {
                    _handle = new WindowInteropHelper(_window).Handle;
                    _nextClipboardViewer = (IntPtr)SetClipboardViewer((int)_handle);
                    _source = (HwndSource)PresentationSource.FromVisual(this);
                    _source?.AddHook(WndProc);
                };

                OnPropertyChanged();
            }
        }*/

        public static readonly DependencyProperty WindowProperty = DependencyProperty.Register(
            nameof(Window), typeof(Window), typeof(ClipboardMonitor), new PropertyMetadata(default(Window), OnWindowChanged));

        private static void OnWindowChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (!e.OldValue.Equals(e.NewValue) && e.NewValue is Window window && dependencyObject is ClipboardMonitor cm)
            {
                window.Closed += (sender, ea) =>
                {
                    ChangeClipboardChain(cm._handle, cm._nextClipboardViewer);
                    cm._source?.RemoveHook(cm.WndProc);
                };

                window.SourceInitialized += (sender1, e1) =>
                {
                    cm._handle = new WindowInteropHelper(window).Handle;
                    cm._nextClipboardViewer = (IntPtr)SetClipboardViewer((int)cm._handle);
                    cm._source = (HwndSource)PresentationSource.FromVisual(cm);
                    cm._source?.AddHook(cm.WndProc);
                };
            }
        }

        public Window Window
        {
            get => (Window) GetValue(WindowProperty);
            set => SetValue(WindowProperty, value);
        }

        #region "Dependency properties"

        public static readonly DependencyProperty ClipboardContainsImageProperty =
            DependencyProperty.Register(
                nameof(ClipboardContainsImage),
                typeof(bool),
                typeof(ClipboardMonitor),
                new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty ClipboardContainsTextProperty =
            DependencyProperty.Register(
                nameof(ClipboardContainsText),
                typeof(bool),
                typeof(ClipboardMonitor),
                new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty ClipboardTextProperty =
            DependencyProperty.Register(
                nameof(ClipboardText),
                typeof(string),
                typeof(ClipboardMonitor),
                new FrameworkPropertyMetadata(string.Empty));
        public static readonly DependencyProperty ClipboardImageProperty =
            DependencyProperty.Register(
                nameof(ClipboardImage),
                typeof(BitmapSource),
                typeof(ClipboardMonitor),
                new FrameworkPropertyMetadata(null));
        public bool ClipboardContainsImage
        {
            get => (bool)GetValue(ClipboardContainsImageProperty);
            set => SetValue(ClipboardContainsImageProperty, value);
        }
        public bool ClipboardContainsText
        {
            get => (bool)GetValue(ClipboardContainsTextProperty);
            set => SetValue(ClipboardContainsTextProperty, value);
        }
        public string ClipboardText
        {
            get => (string)GetValue(ClipboardTextProperty);
            set => SetValue(ClipboardTextProperty, value);
        }
        public BitmapSource ClipboardImage
        {
            get => (BitmapSource)GetValue(ClipboardImageProperty);
            set => SetValue(ClipboardImageProperty, value);
        }
        #endregion

        #region "Routed Events"
        public static readonly RoutedEvent ClipboardDataEvent = EventManager.RegisterRoutedEvent(nameof(ClipboardData), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ClipboardMonitor));
        public event RoutedEventHandler ClipboardData
        {
            add => AddHandler(ClipboardDataEvent, value);
            remove => RemoveHandler(ClipboardDataEvent, value);
        }
        protected virtual void OnRaiseClipboardData(ClipboardDataEventArgs e) => RaiseEvent(e);

        #endregion

        #region "Win32 API"
        private const int WM_DRAWCLIPBOARD = 0x308;
        private const int WM_CHANGECBCHAIN = 0x030D;
        [DllImport("User32.dll")]
        private static extern int SetClipboardViewer(int hWndNewViewer);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);
        #endregion

        #region "Clipboard data"
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_DRAWCLIPBOARD:
                    DrawClipboardData();
                    SendMessage(_nextClipboardViewer, msg, wParam, lParam);
                    break;
                case WM_CHANGECBCHAIN:
                    if (wParam == _nextClipboardViewer)
                        _nextClipboardViewer = lParam;
                    else
                        SendMessage(_nextClipboardViewer, msg, wParam, lParam);
                    break;
            }
            return IntPtr.Zero;
        }
        private void DrawClipboardData()
        {
            IDataObject iData = Clipboard.GetDataObject();
            SetCurrentValue(ClipboardContainsImageProperty, iData != null && iData.GetDataPresent(DataFormats.Bitmap));
            SetCurrentValue(ClipboardContainsTextProperty, iData != null && iData.GetDataPresent(DataFormats.Text));
            if (iData != null)
            {
                SetCurrentValue(ClipboardImageProperty, ClipboardContainsImage ? iData.GetData(DataFormats.Bitmap) as BitmapSource : null);
                SetCurrentValue(ClipboardTextProperty, ClipboardContainsText ? iData.GetData(DataFormats.Text) as string : string.Empty);
                OnRaiseClipboardData(new ClipboardDataEventArgs(ClipboardDataEvent, iData));
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class ClipboardDataEventArgs : RoutedEventArgs
    {
        public IDataObject Data { get; set; }
        public ClipboardDataEventArgs(RoutedEvent routedEvent, IDataObject data)
            : base(routedEvent) => Data = data;
    }
}
