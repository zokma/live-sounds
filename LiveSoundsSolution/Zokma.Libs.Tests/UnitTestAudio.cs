using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Xunit;
using Xunit.Abstractions;
using Zokma.Libs.Audio;

namespace Zokma.Libs.Tests
{
    public class UnitTestAudio
    {
        private readonly ITestOutputHelper output;

        public UnitTestAudio(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData("TestData/AudioFiles/GitExclude/Test01.mp3", true,  1.0f)]
        [InlineData("TestData/AudioFiles/GitExclude/Test01.mp3", true,  0.5f)]
        [InlineData("TestData/AudioFiles/GitExclude/Test01.mp3", false, 1.0f)]
        [InlineData("TestData/AudioFiles/GitExclude/Test01.mp3", false, 0.5f)]
        public void TestLoadAudioFiles(string path, bool isCached, float volume)
        {
            var data = AudioData.LoadAudio(Pathfinder.FindPathName(path), isCached, volume);

            Assert.Equal(isCached, data.IsCached);
            Assert.Equal(volume,   data.Volume);

            Assert.NotNull(data.WaveFormat);

            output.WriteLine("AverageBytesPerSecond: {0}", data.WaveFormat.AverageBytesPerSecond);
            output.WriteLine("BitsPerSample: {0}",         data.WaveFormat.BitsPerSample);
            output.WriteLine("BlockAlign: {0}",            data.WaveFormat.BlockAlign);
            output.WriteLine("Channels: {0}",              data.WaveFormat.Channels);
            output.WriteLine("Encoding: {0}",              data.WaveFormat.Encoding);
            output.WriteLine("ExtraSize: {0}",             data.WaveFormat.ExtraSize);
            output.WriteLine("SampleRate: {0}",            data.WaveFormat.SampleRate);
        }

        [Theory]
        [InlineData("TestData/AudioFiles/GitExclude/Test01.mp3", true,  100.0f, 1.0f)]
        [InlineData("TestData/AudioFiles/GitExclude/Test01.mp3", false, 100.0f, 1.0f)]
        [InlineData("TestData/AudioFiles/GitExclude/Test01.mp3", true,  -10.0f, 0.0f)]
        [InlineData("TestData/AudioFiles/GitExclude/Test01.mp3", false, -10.0f, 0.0f)]
        public void TestLoadAudioFilesVol(string path, bool isCached, float volume, float expectedVol)
        {
            var data = AudioData.LoadAudio(Pathfinder.FindPathName(path), isCached, volume);

            Assert.Equal(isCached,    data.IsCached);
            Assert.Equal(expectedVol, data.Volume);

            Assert.NotNull(data.WaveFormat);

        }

        [Theory]
        [InlineData("TestData/AudioFiles/InvalidAudio00.mp3", true,  1.0f)]
        [InlineData("TestData/AudioFiles/InvalidAudio00.mp3", false, 1.0f)]
        public void TestLoadInvalidAudioFiles(string path, bool isCached, float volume)
        {
            Assert.Throws<InvalidDataException>(
                () =>
                {
                    var data = AudioData.LoadAudio(Pathfinder.FindPathName(path), isCached, volume);
                });
        }

        [Theory]
        [InlineData("TestData/AudioFiles/InvalidAudio01.txt", true, 1.0f)]
        [InlineData("TestData/AudioFiles/InvalidAudio01.txt", false, 1.0f)]
        public void TestLoadAudioFilesUnsupportedExt(string path, bool isCached, float volume)
        {
            Assert.Throws<COMException>(
                () =>
                {
                    var data = AudioData.LoadAudio(Pathfinder.FindPathName(path), isCached, volume);
                });
        }

        [Theory]
        [InlineData("TestData/AudioFiles/NotFound.mp3", true,  1.0f)]
        [InlineData("TestData/AudioFiles/NotFound.mp3", false, 1.0f)]
        public void TestLoadAudioFilesNotFound(string path, bool isCached, float volume)
        {
            Assert.Throws<FileNotFoundException>(
                () =>
                {
                    var data = AudioData.LoadAudio(Pathfinder.FindPathName(path), isCached, volume);
                });
        }


        [Theory]
        [InlineData(AudioDeviceType.WASAPI)]
        [InlineData(AudioDeviceType.Wave)]
        [InlineData(AudioDeviceType.DirectSound)]
        [InlineData(AudioDeviceType.ASIO)]
        public void TestAudioDevices(AudioDeviceType deviceType)
        {
            var devices = AudioDevice.GetAudioDevices(AudioDataFlow.Render, deviceType);

            Assert.NotNull(devices);

            foreach (var item in devices)
            {
                Assert.NotNull(item.Id);
                Assert.NotNull(item.Guid.ToString());
                Assert.Equal(deviceType, item.DeviceType);
                Assert.Equal(AudioDataFlow.Render, item.DataFlow);
                Assert.NotNull(item.Name);
                Assert.NotNull(item.FriendlyName);

                output.WriteLine("Id: {0}",           item.Id);
                output.WriteLine("Guid: {0}",         item.Guid);
                output.WriteLine("DeviceType: {0}",   item.DeviceType);
                output.WriteLine("DataFlow: {0}",     item.DataFlow);
                output.WriteLine("Name: {0}",         item.Name);
                output.WriteLine("FriendlyName: {0}", item.FriendlyName);
                output.WriteLine("MMDevice: {0}",     (item.MMDevice != null));
                output.WriteLine("----");
            }
        }



    }
}
