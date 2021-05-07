using LiveSounds.Localization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LiveSounds
{
    /// <summary>
    /// Interaction logic for UserWebInfoWindow.xaml
    /// </summary>
    public partial class UserWebInfoWindow : Window
    {
        /// <summary>
        /// Interval for clipboard.
        /// </summary>
        private static readonly TimeSpan CLIPBOARD_INTERVAL = TimeSpan.FromSeconds(10.0f);
        
        /// <summary>
        /// Resouce uri.
        /// </summary>
        private string resouceUri;

        /// <summary>
        /// Timer for clipboard.
        /// </summary>
        private DispatcherTimer clipboardTimer;

        public UserWebInfoWindow()
        {
            InitializeComponent();

            this.clipboardTimer = new DispatcherTimer() {
                Interval  = CLIPBOARD_INTERVAL,
                IsEnabled = false,
            };

            this.clipboardTimer.Tick += ClipboardTimer_Tick;

        }

        private void ClipboardTimer_Tick(object sender, EventArgs e)
        {
            this.clipboardTimer.Stop();

            this.ButtonCopyToClipboard.Content = LocalizedInfo.ButtonCopyURLToClipboard;
            this.ButtonCopyToClipboard.IsEnabled = true;
        }

        public void Reset(string resouceId)
        {
            this.Dispatcher.Invoke(
                () =>
                {
                    this.clipboardTimer.Stop();

                    this.ButtonCopyToClipboard.Content = LocalizedInfo.ButtonCopyURLToClipboard;

                    if(String.IsNullOrWhiteSpace(resouceId))
                    {
                        this.resouceUri = null;

                        this.ButtonOpenUserWeb.IsEnabled     = false;
                        this.ButtonCopyToClipboard.IsEnabled = false;
                    }
                    else
                    {
                        var uri = new Uri(String.Format(AppSettings.ZOKMA_WEB_APP_URI_PATTERN, Uri.EscapeDataString(resouceId)));

                        this.resouceUri = uri.AbsoluteUri;

                        this.ButtonOpenUserWeb.IsEnabled     = true;
                        this.ButtonCopyToClipboard.IsEnabled = true;
                    }

                });
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void ColorZoneTitle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void ButtonOpenUserWeb_Click(object sender, RoutedEventArgs e)
        {
            // This check will be verbose, but check with my belief.
            if(this.resouceUri != null && this.resouceUri.StartsWith(AppSettings.ZOKMA_URI_STARTS_WITH))
            {
                using var proc = Process.Start(new ProcessStartInfo(this.resouceUri) { UseShellExecute = true });
            }
        }

        private void ButtonCopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            // This check will be verbose, but check with my belief.
            if (this.resouceUri != null && this.resouceUri.StartsWith(AppSettings.ZOKMA_URI_STARTS_WITH))
            {
                Clipboard.SetText(this.resouceUri);

                this.ButtonCopyToClipboard.IsEnabled = false;
                this.ButtonCopyToClipboard.Content = LocalizedInfo.ButtonURLCopiedToClipboard;

                this.clipboardTimer.Start();
            }
        }
    }
}
