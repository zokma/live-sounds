using LiveSounds.Localization;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
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
using Zokma.Libs;
using Zokma.Libs.Logging;

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
        /// Edge lenght of icon on button.
        /// </summary>
        private const float ICON_EDGE_LENGTH_BUTTON = 24.0f;

        /// <summary>
        /// Data directory name.
        /// </summary>
        private const string DATA_DIRECTORY_NAME = "CustomData";

        /// <summary>
        /// Audio Data directory name.
        /// </summary>
        private const string AUDIO_DATA_DIRECTORY_NAME = "AudioFiles";


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


        /// <summary>
        /// SolidColorBrush for Unmuted.
        /// </summary>
        private static readonly Brush SOLID_COLOR_BRUSH_UNMUTED = new SolidColorBrush(Colors.DeepSkyBlue);

        /// <summary>
        /// SolidColorBrush for muted.
        /// </summary>
        private static readonly Brush SOLID_COLOR_BRUSH_MUTED = new SolidColorBrush(Colors.OrangeRed);


        /// <summary>
        /// Icon for Maximize Window.
        /// </summary>
        private static readonly PackIcon ICON_WINDOW_MAXIMIZE = new PackIcon { Kind = PackIconKind.WindowMaximize, Width = ICON_EDGE_LENGTH_TITLE, Height = ICON_EDGE_LENGTH_TITLE };

        /// <summary>
        /// Icon for Restore Windows Size.
        /// </summary>
        private static readonly PackIcon ICON_WINDOW_RESTORE = new PackIcon { Kind = PackIconKind.WindowRestore, Width = ICON_EDGE_LENGTH_TITLE, Height = ICON_EDGE_LENGTH_TITLE };

        /// <summary>
        /// Icon for Playback unmuted.
        /// </summary>
        private static readonly PackIcon ICON_PLAYBACK_UNMUTED = new PackIcon { Kind = PackIconKind.VolumeHigh, Width = ICON_EDGE_LENGTH_BUTTON, Height = ICON_EDGE_LENGTH_BUTTON };

        /// <summary>
        /// Icon for Playback muted.
        /// </summary>
        private static readonly PackIcon ICON_PLAYBACK_MUTED = new PackIcon { Kind = PackIconKind.VolumeMute, Width = ICON_EDGE_LENGTH_BUTTON, Height = ICON_EDGE_LENGTH_BUTTON };


        /// <summary>
        /// true if audio playback is muted. 
        /// </summary>
        private bool isPlaybackMuted;

        /// <summary>
        /// Pathfinder for data directory.
        /// </summary>
        private Pathfinder dataDirectory;

        /// <summary>
        /// Pathfinder for audio data directory.
        /// </summary>
        private Pathfinder audioDataDirectory;


        public MainWindow()
        {
            InitializeComponent();

            InitWindow();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Log.Information("Window_Loaded starting.");

            this.GridApplicationMain.IsEnabled = false;

            try
            {
                var app = Application.Current;

                if (app != null)
                {
                    app.ShutdownMode = ShutdownMode.OnMainWindowClose;
                }

                InitFormImmediateInfo();
                await InitApplicationAsync();

                Log.Information("Window_Loaded done.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error on Window_Loaded.");

                throw;
            }
            finally
            {
                this.GridApplicationMain.IsEnabled = true;
                App.IsMainFormLoaded = true;
            }
        }

        /// <summary>
        /// Inits Window.
        /// </summary>
        private void InitWindow()
        {
            // Adjust Window size.
            this.MaxWidth  = SystemParameters.PrimaryScreenWidth;
            this.MaxHeight = SystemParameters.PrimaryScreenHeight;
            ResetWindowMaximizeButton();

            var settings = App.Settings;

            int windowWidth  = settings.WindowWidth;
            int windowHeight = settings.WindowHeight;

            var workArea = SystemParameters.WorkArea;

            if(windowWidth > 0)
            {
                this.Width = Math.Max(Math.Min((double)windowWidth, workArea.Width), this.MinWidth);
            }

            if (windowHeight > 0)
            {
                this.Height = Math.Max(Math.Min((double)windowHeight, workArea.Height), this.MinHeight);
            }

            this.WindowStartupLocation = settings.WindowStartupLocation;

            // Set Window style.
            var styleName = settings.WindowStyleName?.ToUpper();

            if(styleName != null)
            {
                if(styleName == "MODERN")
                {
                    settings.WindowStyleName = "Modern";
                }
                else if(styleName == "LEGACY")
                {
                    settings.WindowStyleName = "Legacy";

                    this.AllowsTransparency = false;
                    this.WindowStyle        = WindowStyle.SingleBorderWindow;

                    this.ColorZoneTitle.Visibility    = Visibility.Collapsed;
                    this.WindowBorder.BorderThickness = new Thickness(0.0f);
                }
                else
                {
                    var style = settings.WindowStyle;

                    this.AllowsTransparency = (style == WindowStyle.None);
                    this.WindowStyle        = style;

                    if(!this.AllowsTransparency)
                    {
                        this.WindowBorder.BorderThickness = new Thickness(0.0f);
                    }

                    settings.WindowStyle = style;
                }
            }

            if(Log.IsDebugEnabled)
            {
                Log.Debug("Init WindowSize: WindowWidth={w}, WindowHeight={h}, MaxWidth={mw}, MaxHeight={mh}", this.Width, this.Height, this.MaxWidth, this.MaxHeight);
                Log.Debug("Init WindowStyle: Style={style}, Name={name}, AllowsTransparency={allowtrans}", this.WindowStyle, styleName, this.AllowsTransparency);
                Log.Debug("Init WindowStartupLocation: {location}", this.WindowStartupLocation);
            }
        }

        /// <summary>
        /// Inits form info.
        /// </summary>
        private void InitFormImmediateInfo()
        {
            this.dataDirectory      = App.UserDirectory.GetSubPathfinder(DATA_DIRECTORY_NAME);
            this.audioDataDirectory = this.dataDirectory.GetSubPathfinder(AUDIO_DATA_DIRECTORY_NAME);

            var settings = App.Settings;

            this.SliderPlaybackVolume.Value = settings.AudioRenderVolume;
            this.isPlaybackMuted            = settings.IsAudioRenderMuted;
            ResetPlaybackMuteButton();
            ResetPlaybackMuteTextBlock();
        }

        /// <summary>
        /// Creates data directory.
        /// </summary>
        private void CreateDataDirectory()
        {
            var dataDirectoryInfo      = new DirectoryInfo(this.dataDirectory.BaseDirectory);
            var audioDataDirectoryInfo = new DirectoryInfo(this.audioDataDirectory.BaseDirectory);

            bool shouldCreateInitialData = !dataDirectoryInfo.Exists;

            if(!audioDataDirectoryInfo.Exists)
            {
                audioDataDirectoryInfo.Create();
            }
        }

        /// <summary>
        /// Inits this Application.
        /// </summary>
        private void InitApplication()
        {
            CreateDataDirectory();
        }

        /// <summary>
        /// Inits this Application on background task.
        /// </summary>
        /// <returns>Task info.</returns>
        private Task InitApplicationAsync()
        {
            var result = Task.Run(
                () =>
                {
                    InitApplication();
                }
                );

            return result;
        }

        /// <summary>
        /// Saves settings.
        /// </summary>
        private void SaveSettings()
        {
            var settings = App.Settings;

            settings.WindowWidth  = (int)this.Width;
            settings.WindowHeight = (int)this.Height;

            if(settings.WindowStartupLocationName != null)
            {
                settings.WindowStartupLocation = this.WindowStartupLocation;
            }


            settings.AudioRenderVolume  = (int)this.SliderPlaybackVolume.Value;
            settings.IsAudioRenderMuted = this.isPlaybackMuted;

            settings.Save();
        }

        /// <summary>
        /// Resets Window Maximize Button.
        /// </summary>
        private void ResetWindowMaximizeButton()
        {
            if(this.WindowState == WindowState.Maximized)
            {
                this.ButtonWindowMaximize.Content = ICON_WINDOW_RESTORE;
                this.ButtonWindowMaximize.ToolTip = LocalizedInfo.RestoreWindow;

                this.MenuItemMaximizeWindow.IsEnabled    = false;
                this.MenuItemRestoreWindowSize.IsEnabled = true;
            }
            else
            {
                this.ButtonWindowMaximize.Content = ICON_WINDOW_MAXIMIZE;
                this.ButtonWindowMaximize.ToolTip = LocalizedInfo.MaximizeWindow;

                this.MenuItemMaximizeWindow.IsEnabled    = true;
                this.MenuItemRestoreWindowSize.IsEnabled = false;
            }
        }

        /// <summary>
        /// Resets Playback mute Button.
        /// </summary>
        private void ResetPlaybackMuteButton()
        {
            if (this.isPlaybackMuted)
            {
                this.ButtonPlaybackMute.Background = SOLID_COLOR_BRUSH_MUTED;
                this.ButtonPlaybackMute.Content    = ICON_PLAYBACK_MUTED;
            }
            else
            {
                this.ButtonPlaybackMute.Background = SOLID_COLOR_BRUSH_UNMUTED;
                this.ButtonPlaybackMute.Content    = ICON_PLAYBACK_UNMUTED;
            }
        }

        /// <summary>
        /// Resets Playback mute TextBlock.
        /// </summary>
        private void ResetPlaybackMuteTextBlock()
        {
            if (!this.isPlaybackMuted && this.SliderPlaybackVolume.Value > 0.0f)
            {
                this.TextBlockPlaybackMuted.Visibility = Visibility.Hidden;
            }
            else
            {
                this.TextBlockPlaybackMuted.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Toggles Windows Maximize mode.
        /// </summary>
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

        private void MenuItemCloseApplication_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MenuItemToggleMaximizeWindow_Click(object sender, RoutedEventArgs e)
        {
            ToggleWindowMaximize();
        }

        private void MenuItemMinimizeWindow_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void ButtonTitleIcon_Click(object sender, RoutedEventArgs e)
        {
            this.ColorZoneTitle.ContextMenu.IsOpen = true;
        }

        private void SliderPlaybackVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int volume = (int)this.SliderPlaybackVolume.Value;

            this.TextBlockPlaybackVolume.Text = Convert.ToString(volume);

            ResetPlaybackMuteTextBlock();
        }

        private void ButtonPlaybackMute_Click(object sender, RoutedEventArgs e)
        {
            this.isPlaybackMuted = !this.isPlaybackMuted;
            
            ResetPlaybackMuteButton();
            ResetPlaybackMuteTextBlock();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            SaveSettings();
        }
    }
}
