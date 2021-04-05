using LiveSounds.Localization;
using LiveSounds.MenuItem;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
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
using Zokma.Libs.Audio;
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
        /// Sample Data directory name.
        /// </summary>
        private const string SAMPLE_DATA_DIRECTORY_NAME = "SampleData";

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
        /// Regex for digit only.
        /// </summary>
        private static readonly Regex REGEX_DIGITS_ONLY = new Regex("^[0-9]*$", RegexOptions.Compiled | RegexOptions.Singleline, TimeSpan.FromSeconds(4.0));


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

        /// <summary>
        /// Audio Render device menu items.
        /// </summary>
        private AudioDeviceItem[] audioRenderDeviceItems;

        /// <summary>
        /// Audio Render device selected index.
        /// </summary>
        private int audioRenderDeviceSelectedIndex;

        /// <summary>
        /// DataPreset menu items.
        /// </summary>
        private DataPresetItem[] dataPresetItems;

        /// <summary>
        /// DataPreset selected index.
        /// </summary>
        private int dataPresetSelectedIndex;


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

                InitWindowImmediateInfo();
                await InitApplicationAsync();
                CompleteWindowInfo();

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
                App.IsMainWindowLoaded             = true;
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

                    settings.WindowStyle = this.WindowStyle;
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
        /// Inits Window info on right after Window_Loaded.
        /// </summary>
        private void InitWindowImmediateInfo()
        {
            this.dataDirectory      = App.UserDirectory.GetSubPathfinder(DATA_DIRECTORY_NAME);
            this.audioDataDirectory = this.dataDirectory.GetSubPathfinder(AUDIO_DATA_DIRECTORY_NAME);

            var settings = App.Settings;

            this.SliderPlaybackVolume.Value = settings.AudioRenderVolume;
            this.isPlaybackMuted            = settings.IsAudioRenderMuted;
            ResetPlaybackMuteButton();
            ResetPlaybackMuteTextBlock();

            this.TextBoxPlayAudioLimitsPerApp.Text  = settings.PlayAudioLimitsPerMinutePerApp.ToString();
            this.TextBoxPlayAudioLimitsPerUser.Text = settings.PlayAudioLimitsPerMinutePerUser.ToString();
        }

        /// <summary>
        /// Completes Window info.
        /// </summary>
        private void CompleteWindowInfo()
        {
            CompleteAudioDeviceMenu();
            CompleteDataPresetMenu();
        }

        /// <summary>
        /// Inits this Application.
        /// </summary>
        private void InitApplication()
        {
            CreateDataDirectory();
            LoadAudioDeviceMenuItems(App.Settings.AudioRenderDeviceId);
            LoadDataPresetMenuItems(App.Settings.DataPresetId);
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

            if(shouldCreateInitialData)
            {
                CreateSampleData();
            }
        }

        /// <summary>
        /// Creates sample data.
        /// </summary>
        private void CreateSampleData()
        {
            var sampleAudioDirectory = new DirectoryInfo(Pathfinder.ApplicationRoot.GetSubPathfinder(SAMPLE_DATA_DIRECTORY_NAME, AUDIO_DATA_DIRECTORY_NAME).BaseDirectory);

            foreach (var fileInfo in sampleAudioDirectory.EnumerateFiles("*.mp3", new EnumerationOptions { RecurseSubdirectories = false, ReturnSpecialDirectories = false, MatchType = MatchType.Simple }))
            {
                try
                {
                    fileInfo.CopyTo(this.audioDataDirectory.FindPathName(fileInfo.Name));
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Audio file already exists.");
                }
            }

            var preset = new DataPreset
            {
                Name = LocalizedInfo.SamplePresetName01,
                AudioItems = new AudioInfo[]
                {
                    new AudioInfo{ Name = LocalizedInfo.SampleAudioName02, File = "Sample02.mp3" },
                    new AudioInfo{ Name = LocalizedInfo.SampleAudioName04, File = "Sample04.mp3" },
                }
            };

            using (var fs     = new FileStream(this.dataDirectory.FindPathName("SamplePreset01.json"), FileMode.CreateNew, FileAccess.Write, FileShare.Read))
            using (var writer = new Utf8JsonWriter(fs, AppSettings.JsonWriterOptionForFile))
            {
                JsonSerializer.Serialize(writer, preset, AppSettings.JsonSerializerOptionsForFileWrite);
            }

            preset = new DataPreset
            {
                Name = LocalizedInfo.SamplePresetName02,
                AudioItems = new AudioInfo[]
                {
                    new AudioInfo{ Name = LocalizedInfo.SampleAudioName01, File = "Sample01.mp3" },
                    new AudioInfo{ Name = LocalizedInfo.SampleAudioName02, File = "Sample02.mp3" },
                    new AudioInfo{ Name = LocalizedInfo.SampleAudioName03, File = "Sample03.mp3" },
                    new AudioInfo{ Name = LocalizedInfo.SampleAudioName04, File = "Sample04.mp3" },
                }
            };

            using (var fs     = new FileStream(this.dataDirectory.FindPathName("SamplePreset02.json"), FileMode.CreateNew, FileAccess.Write, FileShare.Read))
            using (var writer = new Utf8JsonWriter(fs, AppSettings.JsonWriterOptionForFile))
            {
                JsonSerializer.Serialize(writer, preset, AppSettings.JsonSerializerOptionsForFileWrite);
            }
        }

        /// <summary>
        /// Loads Audio device Menu items.
        /// </summary>
        /// <param name="selectedId">Id for the selected item.</param>
        private void LoadAudioDeviceMenuItems(string selectedId)
        {
            var settings = App.Settings;

            this.audioRenderDeviceSelectedIndex = 0;

            var audioRenderDevices = new List<AudioDeviceItem>();

            try
            {
                var devices = AudioDevice.GetAudioRenderDevices(settings.AudioRenderDeviceType, settings.AudioRenderDeviceRole);

                for (int i = 0; i < devices.Length; i++)
                {
                    audioRenderDevices.Add(new AudioDeviceItem(devices[i]));

                    if (this.audioRenderDeviceSelectedIndex == 0 && devices[i].Id == selectedId)
                    {
                        this.audioRenderDeviceSelectedIndex = i;
                    }

                    if (Log.IsDebugEnabled)
                    {
                        var device = devices[i];

                        Log.Debug("Audio Render Device: Id={Id}, Name={Name}, FriendlyName={FriendlyName}, Type={Type}",
                            device.Id, device.Name, device.FriendlyName, device.DeviceType);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error on enumerating audio render devices.");
            }

            if (audioRenderDevices.Count == 0)
            {
                audioRenderDevices.Add(new AudioDeviceItem(null));
            }

            this.audioRenderDeviceItems = audioRenderDevices.ToArray();
        }

        /// <summary>
        /// Loads DataPreset Menu items.
        /// </summary>
        /// <param name="selectedId">Id for the selected item.</param>
        private void LoadDataPresetMenuItems(string selectedId)
        {
            var settings = App.Settings;

            this.dataPresetSelectedIndex = 0;

            var dataPresets = new List<DataPresetItem>();

            var directoryInfo = new DirectoryInfo(this.dataDirectory.BaseDirectory);

            int index = 0;

            foreach (var file in directoryInfo.EnumerateFiles("*.json", new EnumerationOptions { RecurseSubdirectories = false, ReturnSpecialDirectories = false, MatchType = MatchType.Simple }))
            {
                try
                {
                    using (var fs     = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var reader = new StreamReader(fs, new UTF8Encoding(false), true))
                    {
                        var dataPreset = JsonSerializer.Deserialize<DataPreset>(reader.ReadToEnd(), AppSettings.JsonSerializerOptionsForFileRead);

                        dataPreset.Id = file.Name;

                        dataPresets.Add(new DataPresetItem(dataPreset));

                        if (this.dataPresetSelectedIndex == 0 && dataPreset.Id == selectedId)
                        {
                            this.dataPresetSelectedIndex = index;
                        }
                        else
                        {
                            index++;
                        }

                        if (Log.IsDebugEnabled)
                        {
                            Log.Debug("DataPreset: Id={Id}, Name={Name}, AudioItems={AudioItems}", dataPreset.Id, dataPreset.Name, dataPreset.AudioItems?.Length);
                        }
                    }
                }
                catch(Exception ex)
                {
                    Log.Error(ex, "Error on getting data preset.");
                }
            }

            if (dataPresets.Count == 0)
            {
                dataPresets.Add(new DataPresetItem(null));
            }

            this.dataPresetItems = dataPresets.ToArray();
        }

        /// <summary>
        /// Completes AudioDevice menu.
        /// </summary>
        private void CompleteAudioDeviceMenu()
        {
            this.ComboBoxAudioRenderDevices.ItemsSource   = this.audioRenderDeviceItems;
            this.ComboBoxAudioRenderDevices.SelectedIndex = this.audioRenderDeviceSelectedIndex;
        }

        /// <summary>
        /// Completes DataPreset menu.
        /// </summary>
        private void CompleteDataPresetMenu()
        {
            this.ComboBoxDataPresets.ItemsSource   = this.dataPresetItems;
            this.ComboBoxDataPresets.SelectedIndex = this.dataPresetSelectedIndex;
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

            var device = this.ComboBoxAudioRenderDevices.SelectedItem as AudioDeviceItem;
            settings.AudioRenderDeviceId = device?.Device?.Id;

            settings.AudioRenderVolume  = (int)this.SliderPlaybackVolume.Value;
            settings.IsAudioRenderMuted = this.isPlaybackMuted;

            var preset = this.ComboBoxDataPresets.SelectedItem as DataPresetItem;
            settings.DataPresetId = preset?.DataPreset?.Id;

            settings.PlayAudioLimitsPerMinutePerApp  = AppSettings.GetPlayAudioLimits(this.TextBoxPlayAudioLimitsPerApp.Text,  AppSettings.PLAY_AUDIO_LIMITS_PER_APP_DEFAULT);
            settings.PlayAudioLimitsPerMinutePerUser = AppSettings.GetPlayAudioLimits(this.TextBoxPlayAudioLimitsPerUser.Text, AppSettings.PLAY_AUDIO_LIMITS_PER_USER_DEFAULT);

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

        private void ButtonOpenDataDirectory_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("EXPLORER.EXE", this.dataDirectory.BaseDirectory);
        }

        private async void ButtonReloadDataPresets_Click(object sender, RoutedEventArgs e)
        {
            this.GridApplicationMain.IsEnabled = false;

            try
            {
                var selectedItem = this.ComboBoxDataPresets.SelectedItem as DataPresetItem;

                await Task.Run(
                    () =>
                    {
                        LoadDataPresetMenuItems(selectedItem?.DataPreset?.Id);
                    });

                CompleteDataPresetMenu();
            }
            finally
            {
                this.GridApplicationMain.IsEnabled = true;
            }
        }

        private async void ButtonReloadAudioRenderDevices_Click(object sender, RoutedEventArgs e)
        {
            this.GridApplicationMain.IsEnabled = false;

            try
            {
                var selectedItem = this.ComboBoxAudioRenderDevices.SelectedItem as AudioDeviceItem;

                await Task.Run(
                    () =>
                    {
                        LoadAudioDeviceMenuItems(selectedItem?.Device?.Id);
                    });

                CompleteAudioDeviceMenu();
            }
            finally
            {
                this.GridApplicationMain.IsEnabled = true;
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            ResetWindowMaximizeButton();
        }

        private void TextBoxDigitsOnly_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if(e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
            else
            {
                var command = e.Command as RoutedUICommand;

                if(command?.Name == "Space")
                {
                    e.Handled = true;
                }
            }
        }

        private void TextBoxDigitsOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !(REGEX_DIGITS_ONLY.IsMatch(e.Text));
        }

        private void TextBoxPlayAudioLimitsPerApp_LostFocus(object sender, RoutedEventArgs e)
        {
            this.TextBoxPlayAudioLimitsPerApp.Text = AppSettings.GetPlayAudioLimits(this.TextBoxPlayAudioLimitsPerApp.Text, AppSettings.PLAY_AUDIO_LIMITS_PER_APP_DEFAULT).ToString();
        }

        private void TextBoxPlayAudioLimitsPerUser_LostFocus(object sender, RoutedEventArgs e)
        {
            this.TextBoxPlayAudioLimitsPerUser.Text = AppSettings.GetPlayAudioLimits(this.TextBoxPlayAudioLimitsPerUser.Text, AppSettings.PLAY_AUDIO_LIMITS_PER_USER_DEFAULT).ToString();
        }
    }
}
