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
using Zokma.Libs;
using Zokma.Libs.Audio;

namespace TestAudioPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private class AudioDeviceItem
        {
            public AudioDevice AudioDevice { get; set; }

            public override string ToString()
            {
                return this.AudioDevice.FriendlyName;
            }
        }

        private AudioPlayer player;

        private AudioData audioData1;
        private AudioData audioData2;


        public MainWindow()
        {
            InitializeComponent();
        }

        private void SetAudioDevices()
        {
            if(this.AudioDeviceSelect == null)
            {
                return;
            }

            var selected = this.AudioDeviceTypeSelect.SelectedValue as ListBoxItem;

            AudioDeviceType type;

            if (!Enum.TryParse(selected.Content as string, true, out type))
            {
                type = Zokma.Libs.Audio.AudioDeviceType.WASAPI;
            }

            var devices = AudioDevice.GetAudioRenderDevices(type);

            this.AudioDeviceSelect.Items.Clear();
            foreach (var item in devices)
            {
                this.AudioDeviceSelect.Items.Add(new AudioDeviceItem { AudioDevice = item });
            }

            this.AudioDeviceSelect.SelectedIndex = 0;
        }

        private void AudioDeviceType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetAudioDevices();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.audioData1 = AudioData.LoadAudio(Pathfinder.ApplicationRoot.FindPathName("TestData/GitExclude/Test01.mp3"), AudioPlayer.DefaultWaveFormat);
            this.audioData2 = AudioData.LoadAudio(Pathfinder.ApplicationRoot.FindPathName("TestData/GitExclude/Test02.mp3"), AudioPlayer.DefaultWaveFormat);
            SetAudioDevices();
        }

        private void MasterVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.player != null)
            {
                this.player.MasterVolume = (float)(this.MasterVolume.Value / 100.0f);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.player?.Dispose();
        }


        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if(this.player == null)
            {
                return;
            }

            AudioData data;

            if (sender == this.Play1Button)
            {
                data = this.audioData1;
            }
            else
            {
                data = this.audioData2;
            }

            var mode = this.LoopAudio.IsChecked.Value ? PlaybackMode.Loop : PlaybackMode.Once;

            this.player?.Play(data, mode);
        }

        private void InitButton_Click(object sender, RoutedEventArgs e)
        {
            this.player?.Dispose();

            this.player = new AudioPlayer((this.AudioDeviceSelect.SelectedItem as AudioDeviceItem).AudioDevice);
            this.player.MasterVolume = (float)(this.MasterVolume.Value / 100.0f);
            this.player.Start();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            this.player?.Stop();
        }
    }
}
