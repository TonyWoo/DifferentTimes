using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private Dictionary<string, string> timeZones = new Dictionary<string, string>()
        {
            { "PST", "Pacific Standard Time" },
            { "EST", "Eastern Standard Time" },
            // Add more time zones as needed
        };

        private List<TextBlock> timeDisplays = new List<TextBlock>();
        private bool isDragging = false;
        private Point initialMousePosition;
        private Point initialWindowPosition;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;

            // Set window properties for desktop integration
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            Topmost = true; // Ensure window is always on top
            ShowInTaskbar = false;
            ResizeMode = ResizeMode.NoResize;
            Width = SystemParameters.PrimaryScreenWidth;
            Height = SystemParameters.PrimaryScreenHeight;

            // Make window click-through
            WindowUtils.SetWindowExTransparent(this);

            // Start timer to update times periodically
            var timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();

            // Register mouse event handlers for dragging
            //MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;
            //MouseLeftButtonUp += MainWindow_MouseLeftButtonUp;
            //MouseMove += MainWindow_MouseMove;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Update times
            int index = 0;
            foreach (var timeZone in timeZones)
            {
                var currentTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(timeZone.Value));
                timeDisplays[index].Text = $"{timeZone.Key}: {currentTime.ToString("M/d HH:mm")}";
                index++;
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Create and add time displays
            foreach (var timeZone in timeZones)
            {
                var timeDisplay = new TextBlock();
                timeDisplay.FontSize = 20;
                timeDisplay.Foreground = Brushes.White;
                timeDisplays.Add(timeDisplay);
                canvas.Children.Add(timeDisplay); // Add text block to canvas
            }
            Top = 0;
            // Set the position of the time displays
            SetTimeDisplayPositions();
        }

        private void SetTimeDisplayPositions()
        {
            // Set initial top position
            double topPosition = 0;

            // Set initial left position (right-aligned)
            double leftPosition = SystemParameters.PrimaryScreenWidth-90;

            // Set time display positions
            foreach (var timeDisplay in timeDisplays)
            {
                timeDisplay.TextAlignment = TextAlignment.Right;
                timeDisplay.Width = SystemParameters.PrimaryScreenWidth;
                timeDisplay.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                Canvas.SetLeft(timeDisplay, leftPosition - timeDisplay.DesiredSize.Width);
                Canvas.SetTop(timeDisplay, topPosition);
                topPosition += timeDisplay.DesiredSize.Height + 5;
            }
        }

        private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            initialMousePosition = e.GetPosition(this);
            initialWindowPosition = new Point(Left, Top);
        }

        private void MainWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
        }

        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point newPoint = e.GetPosition(null);
                double deltaX = newPoint.X - initialMousePosition.X;
                double deltaY = newPoint.Y - initialMousePosition.Y;

                // Calculate the new window position
                double newLeft = initialWindowPosition.X + deltaX;
                double newTop = initialWindowPosition.Y + deltaY;

                // Get desktop dimensions
                double screenWidth = SystemParameters.PrimaryScreenWidth;
                double screenHeight = SystemParameters.PrimaryScreenHeight;

                // Check if the window is outside the screen boundaries
                if (newLeft < 0)
                    newLeft = 0;
                else if (newLeft + ActualWidth > screenWidth)
                    newLeft = screenWidth - ActualWidth;

                if (newTop < 0)
                    newTop = 0;
                else if (newTop + ActualHeight > screenHeight)
                    newTop = screenHeight - ActualHeight;

                Left = newLeft;
                Top = newTop;
            }
        }
    }

    public static class WindowUtils
    {
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_LAYERED = 0x80000;
        public const int WS_EX_TRANSPARENT = 0x20;

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        public static void SetWindowExTransparent(Window window)
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT);
        }
    }
}
