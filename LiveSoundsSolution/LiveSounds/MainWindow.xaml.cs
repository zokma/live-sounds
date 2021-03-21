using LiveSounds.Localization;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LiveSounds
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Edge lenght of icon on title.
        /// </summary>
        private const float ICON_EDGE_LENGTH_TITLE = 24.0f;

        /// <summary>
        /// SolidColorBrush for Transparent.
        /// </summary>
        private static readonly Brush SOLID_COLOR_BRUSH_TRANSPARENT = new SolidColorBrush(Colors.Transparent);

        /// <summary>
        /// SolidColorBrush for Transparent.
        /// </summary>
        private static readonly Brush SOLID_COLOR_BRUSH_GRAY = new SolidColorBrush(Colors.Gray);

        /// <summary>
        /// SolidColorBrush for Red.
        /// </summary>
        private static readonly Brush SOLID_COLOR_BRUSH_RED = new SolidColorBrush(Colors.Red);

        private static readonly PackIcon ICON_WINDOW_MAXIMIZE = new PackIcon { Kind = PackIconKind.WindowMaximize, Width = ICON_EDGE_LENGTH_TITLE, Height = ICON_EDGE_LENGTH_TITLE };
        private static readonly PackIcon ICON_WINDOW_RESTORE = new PackIcon { Kind = PackIconKind.WindowRestore, Width = ICON_EDGE_LENGTH_TITLE, Height = ICON_EDGE_LENGTH_TITLE };


        public MainWindow()
        {
            InitializeComponent();

            InitForm();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            App.IsMainFormLoaded = true;
        }

        private void InitForm()
        {
            this.MaxHeight = SystemParameters.PrimaryScreenHeight;
            ResetWindowMaximizeButton();
        }

        private void ResetWindowMaximizeButton()
        {
            if(this.WindowState == WindowState.Maximized)
            {
                this.ButtonWindowMaximize.Content = ICON_WINDOW_RESTORE;
                this.ButtonWindowMaximize.ToolTip = LocalizedInfo.RestoreWindow;
            }
            else
            {
                this.ButtonWindowMaximize.Content = ICON_WINDOW_MAXIMIZE;
                this.ButtonWindowMaximize.ToolTip = LocalizedInfo.MaximizeWindow;
            }
        }

        private void ToggleWindowMaximize()
        {
            if (this.WindowState != WindowState.Maximized)
            {
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.WindowState = WindowState.Normal;
            }

            ResetWindowMaximizeButton();
        }

        private void ColorZoneTitle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void ButtonWindowClose_MouseEnter(object sender, MouseEventArgs e)
        {
            var button = sender as Button;

            if(button != null)
            {
                button.Background = SOLID_COLOR_BRUSH_RED;
            }
        }

        private void ButtonWindow_MouseLeave(object sender, MouseEventArgs e)
        {
            var button = sender as Button;

            if (button != null)
            {
                button.Background = SOLID_COLOR_BRUSH_TRANSPARENT;
            }
        }

        private void ButtonWindow_MouseEnter(object sender, MouseEventArgs e)
        {
            var button = sender as Button;

            if (button != null)
            {
                button.Background = SOLID_COLOR_BRUSH_GRAY;
            }
        }

        private void ButtonWindowClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonWindowMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void ButtonWindowMaximize_Click(object sender, RoutedEventArgs e)
        {
            ToggleWindowMaximize();
        }

        private void ColorZoneTitle_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            ToggleWindowMaximize();
        }

        private void CloseApplicationMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
