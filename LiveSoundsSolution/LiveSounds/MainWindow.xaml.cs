using LiveSounds.Audio;
using LiveSounds.Localization;
using LiveSounds.MenuItem;
using LiveSounds.Ngrok;
using LiveSounds.Notification;
using LiveSounds.Service;
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
        /// Notification Area name.
        /// </summary>
        private const string NOTIFICATION_AREA_NAME = "NotificationAreaMain";

        /// <summary>
        /// Delay before audio device destory.
        /// </summary>
        private const int SAFE_DELAY_BEFORE_AUDIO_DEVICE_DESTROY_MS = 100;

        /// <summary>
        /// Delay after audio device destory.
        /// </summary>
        private const int SAFE_DELAY_AFTER_AUDIO_DEVICE_DESTROY_MS = 1000;


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
        /// Default duration for notification.
        /// </summary>
        private static readonly TimeSpan NOTIFICATION_DURATION_DEFAULT = TimeSpan.FromSeconds(5.0f);

        /// <summary>
        /// Long duration for notification.
        /// </summary>
        private static readonly TimeSpan NOTIFICATION_DURATION_LONG = TimeSpan.FromSeconds(15.0f);

        /// <summary>
        /// Long duration for notification.
        /// </summary>
        private static readonly TimeSpan NOTIFICATION_DURATION_INFINITE = TimeSpan.MaxValue;


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

        /// <summary>
        /// Notification manager.
        /// </summary>
        private NotificationManager notification;

        /// <summary>
        /// Service manager.
        /// </summary>
        private ServiceManager serviceManager;

        /// <summary>
        /// Audio Player.
        /// </summary>
        private AudioPlayer audioPlayer;

        /// <summary>
        /// User Web Info Window.
        /// </summary>
        private UserWebInfoWindow userWebInfoWindow;

        /// <summary>
        /// Saved width to restore size.
        /// </summary>
        private double savedWidth;

        /// <summary>
        /// Saved height to restore size.
        /// </summary>
        private double savedHeight;


        public MainWindow()
        {
            InitializeComponent();

            InitWindow();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Information("Window_Loaded starting.");

                this.GridApplicationMain.IsEnabled = false;

                InitWindowImmediateInfo();
                await InitApplicationAsync();
                CompleteWindowInfo();

                var settings = App.Settings;

                this.serviceManager = new ServiceManager(this.notification, settings.NgrokApiPort, settings.PlayAudioLimitsPerMinutePerApp, settings.PlayAudioLimitsPerMinutePerUser, this.Dispatcher, PerformStopService);

                this.notification.ShowNotification(LocalizedInfo.MessageAppStartSuccess, NotificationLevel.Success);

                if(!settings.HasEncryptedToken)
                {
                    this.notification.ShowNotification(LocalizedInfo.MessageTokenInfoNotFound, NotificationLevel.Warn, NOTIFICATION_DURATION_LONG,
                        () =>
                        {
                            this.Dispatcher.Invoke(() => ShowTokenInfoDialog());
                        });
                }

                Log.Information("Window_Loaded done.");
            }
            catch (Exception ex)
            {
                this.notification.ShowNotification(LocalizedInfo.MessageAppStartWithError, NotificationLevel.Warn);
                
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

            this.TextBoxLocalPort.Text = settings.LocalPort.ToString();


            this.notification = new NotificationManager(settings.NotificationMax, settings.HistoryMax, this.Dispatcher, NOTIFICATION_AREA_NAME);

            this.NotificationAreaMain.MaxItems = this.notification.NotificationMax;

            if (this.notification.HistoryMax > 0)
            {
                this.DataGridHistory.DataContext = this.notification.HistoryTable;
            }
            else
            {
                this.DataGridHistory.Visibility = Visibility.Collapsed;
            }
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
                    using var device = devices[i];

                    audioRenderDevices.Add(new AudioDeviceItem(device));

                    if (this.audioRenderDeviceSelectedIndex == 0 && device.Id == selectedId)
                    {
                        this.audioRenderDeviceSelectedIndex = i;
                    }

                    if (Log.IsDebugEnabled)
                    {
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

                        if (dataPreset.AudioItems != null && dataPreset.AudioItems.Length > 0)
                        {
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

            if (this.WindowState == WindowState.Normal)
            {
                settings.WindowWidth  = (int)this.Width;
                settings.WindowHeight = (int)this.Height;
            }
            else
            {
                settings.WindowWidth  = (int)this.savedWidth;
                settings.WindowHeight = (int)this.savedHeight;
            }

            if (settings.WindowStartupLocationName != null)
            {
                settings.WindowStartupLocation = this.WindowStartupLocation;
            }

            var device = this.ComboBoxAudioRenderDevices.SelectedItem as AudioDeviceItem;
            settings.AudioRenderDeviceId = device.DeviceId;

            settings.AudioRenderVolume  = (int)this.SliderPlaybackVolume.Value;
            settings.IsAudioRenderMuted = this.isPlaybackMuted;

            var preset = this.ComboBoxDataPresets.SelectedItem as DataPresetItem;
            settings.DataPresetId = preset?.DataPreset?.Id;

            settings.PlayAudioLimitsPerMinutePerApp  = AppSettings.GetPlayAudioLimits(this.TextBoxPlayAudioLimitsPerApp.Text,  AppSettings.PLAY_AUDIO_LIMITS_PER_APP_DEFAULT);
            settings.PlayAudioLimitsPerMinutePerUser = AppSettings.GetPlayAudioLimits(this.TextBoxPlayAudioLimitsPerUser.Text, AppSettings.PLAY_AUDIO_LIMITS_PER_USER_DEFAULT);

            settings.LocalPort = AppSettings.GetNetworkPort(this.TextBoxLocalPort.Text, AppSettings.LOCAL_PORT_DEFAULT);

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
                // Save the size to restore;
                this.savedWidth  = this.Width;
                this.savedHeight = this.Height;

                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.WindowState = WindowState.Normal;
            }
        }


        /// <summary>
        /// Minimized the Window.
        /// </summary>
        private void MinimizeWindow()
        {
            // Save the size to restore;
            this.savedWidth  = this.Width;
            this.savedHeight = this.Height;

            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Creates AudioPlayer.
        /// </summary>
        /// <param name="audioDeviceId">Audio Device id.</param>
        /// <returns>AudioPlayer.</returns>
        private AudioPlayer CreateAudioPlayer(string audioDeviceId)
        {
            AudioPlayer player = null;

            try
            {
                var settings = App.Settings;

                AudioDevice device = null;

                if (audioDeviceId != null)
                {
                    var devices = AudioDevice.GetAudioRenderDevices(settings.AudioRenderDeviceType, settings.AudioRenderDeviceRole);

                    foreach (var item in devices)
                    {
                        if (item.Id == audioDeviceId)
                        {
                            device = item;
                            break;
                        }
                        else
                        {
                            item.Dispose();
                        }
                    }
                }

                if (device != null)
                {
                    player = new AudioPlayer(
                        device,
                        settings.AudioWaveFormat,
                        settings.AudioRenderEngineShareMode,
                        settings.AudioRenderLatency);

                    player.Init();
                    player.Start();
                }
                else
                {
                    this.notification.ShowNotification(LocalizedInfo.MessageValidAudioRenderDeviceNotFound, NotificationLevel.Error);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error on creating AudioPlayer.");

                player?.Dispose();
                player = null;
            }

            if(player == null)
            {
                this.notification.ShowNotification(LocalizedInfo.MessageInitAudioPlayerFailed, NotificationLevel.Error);
            }

            return player;
        }

        /// <summary>
        /// Inits AudioPlayer.
        /// </summary>
        /// <returns>true if the operation was succeeded.</returns>
        private async Task<bool> InitAudioPlayer(AudioDeviceItem audioDeviceItem)
        {
            if(this.audioPlayer != null)
            {
                await DestroyAudioPlayer();
            }

            bool result = false;

            var player = await Task.Run(
                () =>
                {
                    return CreateAudioPlayer(audioDeviceItem?.DeviceId);
                });

            if(player != null)
            {
                this.audioPlayer = player;
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Destroys AudioPlayer().
        /// </summary>
        /// <returns>true if the operation was succeeded.</returns>
        private async Task<bool> DestroyAudioPlayer()
        {
            bool result = false;

            try
            {
                var player = this.audioPlayer;

                this.audioPlayer = null;

                if(player != null)
                {
                    if(player.State == AudioEngineState.Started)
                    {
                        player.Stop();

                        if (SAFE_DELAY_BEFORE_AUDIO_DEVICE_DESTROY_MS > 0)
                        {
                            await Task.Delay(SAFE_DELAY_BEFORE_AUDIO_DEVICE_DESTROY_MS);
                        }
                    }

                    player.Dispose();

                    if (SAFE_DELAY_AFTER_AUDIO_DEVICE_DESTROY_MS > 0)
                    {
                        await Task.Delay(SAFE_DELAY_AFTER_AUDIO_DEVICE_DESTROY_MS);
                    }

                    result = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error on deleting AudioPlayer.");
            }

            return result;
        }

        /// <summary>
        /// Sets MasterVolume for AudioPlayer.
        /// </summary>
        /// <param name="player">AudioPlayer to be configured.</param>
        /// <returns>true if the operation was succeeded.</returns>
        public bool SetAudioPlayerMasterVolume(AudioPlayer player)
        {
            bool result = false;

            if(player != null)
            {
                player.MasterVolume = (this.isPlaybackMuted ? 0.0f : (float)(this.SliderPlaybackVolume.Value / 100.0f));

                result = true;
            }

            return result;
        }

        /// <summary>
        /// Shows token info dialog.
        /// </summary>
        private void ShowTokenInfoDialog()
        {
            try
            {
                this.GridApplicationMain.IsEnabled = false;

                var tokenSettingWindow = new TokenSettingWindow
                {
                    Owner = this
                };

                bool? result = tokenSettingWindow.ShowDialog();

                if (result == true)
                {
                    this.notification.ShowNotification(LocalizedInfo.MessageTokenInfoUpdated, NotificationLevel.Info);
                }
            }
            finally
            {
                this.GridApplicationMain.IsEnabled = true;
            }
        }

        private void ColorZoneTitle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void ButtonWindowClose_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                button.Background = SOLID_COLOR_BRUSH_RED;
            }
        }

        private void ButtonWindow_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                button.Background = SOLID_COLOR_BRUSH_TRANSPARENT;
            }
        }

        private void ButtonWindow_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
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
            MinimizeWindow();
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
            MinimizeWindow();
        }

        private void ButtonTitleIcon_Click(object sender, RoutedEventArgs e)
        {
            this.ColorZoneTitle.ContextMenu.IsOpen = true;
        }

        private void SliderPlaybackVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SetAudioPlayerMasterVolume(this.audioPlayer);

            int volume = (int)this.SliderPlaybackVolume.Value;

            this.TextBlockPlaybackVolume.Text = Convert.ToString(volume);

            ResetPlaybackMuteTextBlock();
        }

        private void ButtonPlaybackMute_Click(object sender, RoutedEventArgs e)
        {
            this.isPlaybackMuted = !this.isPlaybackMuted;

            SetAudioPlayerMasterVolume(this.audioPlayer);

            ResetPlaybackMuteButton();
            ResetPlaybackMuteTextBlock();
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.GridApplicationMain.IsEnabled = false;

            this.notification.ShowNotification(LocalizedInfo.MessageExitingApplication, NotificationLevel.Info);

            await this.serviceManager?.Stop();
            this.serviceManager?.Dispose();

            await DestroyAudioPlayer();

            Log.Information("Window Closing.");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            SaveSettings();

            Log.Information("Window Closed.");
        }

        private void ButtonOpenDataDirectory_Click(object sender, RoutedEventArgs e)
        {
            var info = new ProcessStartInfo()
            {
                FileName        = "EXPLORER.EXE",
                Arguments       = $"\"{this.dataDirectory.BaseDirectory}\"",
                UseShellExecute = false,
            };

            using var process = Process.Start(info);
        }

        private async void ButtonReloadDataPresets_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.GridApplicationMain.IsEnabled = false;

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
            try
            {
                this.GridApplicationMain.IsEnabled = false;

                var selectedItem = this.ComboBoxAudioRenderDevices.SelectedItem as AudioDeviceItem;

                await Task.Run(
                    () =>
                    {
                        LoadAudioDeviceMenuItems(selectedItem?.DeviceId);
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
            int value = AppSettings.GetPlayAudioLimits(this.TextBoxPlayAudioLimitsPerApp.Text, AppSettings.PLAY_AUDIO_LIMITS_PER_APP_DEFAULT);
    
            this.TextBoxPlayAudioLimitsPerApp.Text = value.ToString();

            this.serviceManager.PlayAudioLimitsPerApp = value;
        }

        private void TextBoxPlayAudioLimitsPerUser_LostFocus(object sender, RoutedEventArgs e)
        {
            int value = AppSettings.GetPlayAudioLimits(this.TextBoxPlayAudioLimitsPerUser.Text, AppSettings.PLAY_AUDIO_LIMITS_PER_USER_DEFAULT);

            this.TextBoxPlayAudioLimitsPerUser.Text = value.ToString();

            this.serviceManager.PlayAudioLimitsPerUser = value;
        }

        private void TextBoxLocalPort_LostFocus(object sender, RoutedEventArgs e)
        {
            this.TextBoxLocalPort.Text = AppSettings.GetNetworkPort(this.TextBoxLocalPort.Text, AppSettings.LOCAL_PORT_DEFAULT).ToString();
        }

        private void ButtonTokenSetting_Click(object sender, RoutedEventArgs e)
        {
            ShowTokenInfoDialog();
        }

        private async void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.GridApplicationMain.IsEnabled = false;

                var audioDeviceItem = (this.ComboBoxAudioRenderDevices.SelectedItem as AudioDeviceItem);

                if (await InitAudioPlayer(audioDeviceItem))
                {
                    this.notification.ShowNotification(LocalizedInfo.MessageStartingService, NotificationLevel.Info);

                    SetAudioPlayerMasterVolume(this.audioPlayer);

                    var settings = App.Settings;

                    var config = new ServiceConfig
                    {
                        AudioItems         = (this.ComboBoxDataPresets.SelectedItem as DataPresetItem)?.DataPreset?.AudioItems,
                        AudioDataDirectory = this.audioDataDirectory,
                        AudioPlayer        = this.audioPlayer,
                        ForwardingPort     = AppSettings.GetNetworkPort(this.TextBoxLocalPort.Text, AppSettings.LOCAL_PORT_DEFAULT),
                    };

                    var info = await this.serviceManager.StartService(config);

                    if (info.IsRunning)
                    {
                        this.notification.ShowNotification(
                            String.Format(LocalizedInfo.MessagePatternServiceStarted, info.ValidityPeriod.TotalHours),
                            NotificationLevel.Success);

                        ShowUserWebPageInfo();

                        this.ComboBoxAudioRenderDevices.IsEnabled     = false;
                        this.ButtonReloadAudioRenderDevices.IsEnabled = false;

                        this.ComboBoxDataPresets.IsEnabled     = false;
                        this.ButtonReloadDataPresets.IsEnabled = false;

                        this.ButtonStart.IsEnabled    = false;
                        this.ButtonStop.IsEnabled     = true;
                        this.ButtonTestPlay.IsEnabled = false;

                        this.ButtonUserWebPage.IsEnabled = true;

                        this.TextBoxLocalPort.Text = info.TunnelInfo.ForwardingInfo.Port.ToString();
                    }
                    else
                    {
                        this.notification.ShowNotification(LocalizedInfo.MessageServiceStartFailed, NotificationLevel.Error, NOTIFICATION_DURATION_LONG);
                    }
                }
            }
            finally
            {
                this.GridApplicationMain.IsEnabled = true;
            }
        }

        /// <summary>
        /// Performs clicking ButtonStop.
        /// </summary>
        private void PerformButtonStopClick()
        {
            if (this.ButtonStop.IsEnabled)
            {
                this.ButtonStop.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        /// <summary>
        /// Performs stopping service.
        /// </summary>
        private void PerformStopService()
        {
            if (this.Dispatcher.CheckAccess())
            {
                PerformButtonStopClick();
            }
            else
            {
                this.Dispatcher.Invoke(
                    () => PerformButtonStopClick()
                    );
            }
        }

        private async void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.GridApplicationMain.IsEnabled = false;

                this.notification.ShowNotification(LocalizedInfo.MessageStoppingService, NotificationLevel.Info);

                await this.serviceManager.Stop();
                await DestroyAudioPlayer();
            }
            catch(Exception ex)
            {
                Log.Error(ex, "Error on stopping service.");
            }
            finally
            {
                this.userWebInfoWindow?.Hide();

                this.ButtonUserWebPage.IsEnabled = false;

                this.ButtonTestPlay.IsEnabled = true;
                this.ButtonStop.IsEnabled     = false;
                this.ButtonStart.IsEnabled    = true;

                this.ButtonReloadDataPresets.IsEnabled = true;
                this.ComboBoxDataPresets.IsEnabled     = true;

                this.ButtonReloadAudioRenderDevices.IsEnabled = true;
                this.ComboBoxAudioRenderDevices.IsEnabled     = true;

                this.GridApplicationMain.IsEnabled = true;

                this.notification.ShowNotification(LocalizedInfo.MessageServiceStopped, NotificationLevel.Info);
            }
        }


        private async Task StartTestPlay(AudioInfo[] audioInfoList)
        {
            using var audioManager = AudioManager.CreateAudioManager(audioInfoList, this.audioDataDirectory, this.notification);

            if(audioManager.AudioItems == null || audioManager.AudioItems.Count <= 0)
            {
                notification.ShowNotification(LocalizedInfo.MessageValidAudioFileNotFound, Notification.NotificationLevel.Error);

                return;
            }
            else
            {
                notification.ShowNotification(LocalizedInfo.MessageAudioTestPlay, Notification.NotificationLevel.Info);
            }

            var random     = new Random();
            var audioItems = audioManager.AudioItems;

            int count = audioItems.Count;

            for (int i = 0; i < 5; i++)
            {
                var audio = audioItems[random.Next(count)];
                this.audioPlayer.Play(audio.Data);
                
                notification.Notify(
                    String.Format(LocalizedInfo.MessagePatternPlaying, audio.Name), 
                    Notification.NotificationLevel.Info);

                await Task.Delay(1000);
            }
        }

        private async void ButtonTestPlay_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.GridApplicationMain.IsEnabled = false;

                var audioDeviceItem = (this.ComboBoxAudioRenderDevices.SelectedItem as AudioDeviceItem);

                if(await InitAudioPlayer(audioDeviceItem))
                {
                    this.GridApplicationMain.IsEnabled = true;

                    this.ComboBoxAudioRenderDevices.IsEnabled     = false;
                    this.ButtonReloadAudioRenderDevices.IsEnabled = false;

                    this.ButtonStart.IsEnabled    = false;
                    this.ButtonTestPlay.IsEnabled = false;

                    SetAudioPlayerMasterVolume(this.audioPlayer);

                    var audioItems = (this.ComboBoxDataPresets.SelectedItem as DataPresetItem)?.DataPreset?.AudioItems;

                    await Task.Run(
                        async () =>
                        {
                            await StartTestPlay(audioItems);
                        });

                    await DestroyAudioPlayer();
                }
            }
            finally
            {
                this.ButtonTestPlay.IsEnabled = true;
                this.ButtonStart.IsEnabled    = true;

                this.ButtonReloadAudioRenderDevices.IsEnabled = true;
                this.ComboBoxAudioRenderDevices.IsEnabled     = true;

                this.GridApplicationMain.IsEnabled = true;
            }
        }

        private void ShowUserWebPageInfo()
        {
            if (this.userWebInfoWindow == null)
            {
                this.userWebInfoWindow = new UserWebInfoWindow();
            }

            this.userWebInfoWindow.Reset(this.serviceManager?.SoundId);

            double top  = 0.0d;
            double left = 0.0d;

            if (this.WindowState == WindowState.Normal)
            {
                top  = this.Top;
                left = this.Left;
            }

            this.userWebInfoWindow.Top  = (top + (this.Height / 2)) - (this.userWebInfoWindow.Height / 2);
            this.userWebInfoWindow.Left = (left + (this.Width / 2)) - (this.userWebInfoWindow.Width  / 2);

            if (this.userWebInfoWindow.Visibility == Visibility.Visible)
            {
                this.userWebInfoWindow.Focus();
            }
            else
            {
                this.userWebInfoWindow.Show();
            }
        }

        private void ButtonUserWebPage_Click(object sender, RoutedEventArgs e)
        {
            ShowUserWebPageInfo();
        }

        private void ButtonInitialSetup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.GridApplicationMain.IsEnabled = false;

                var initialSetupWindow = new InitialSetupWindow
                {
                    Owner = this
                };

                initialSetupWindow.ShowDialog();
            }
            finally
            {
                this.GridApplicationMain.IsEnabled = true;
            }
        }
    }
}
