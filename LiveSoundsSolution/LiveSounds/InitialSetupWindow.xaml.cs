using LiveSounds.Localization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Zokma.Libs;
using Zokma.Libs.Logging;

namespace LiveSounds
{
    /// <summary>
    /// Interaction logic for InitialSetupWindow.xaml
    /// </summary>
    public partial class InitialSetupWindow : Window
    {
        /// <summary>
        /// ngrok Web site.
        /// </summary>
        private const string NGROK_WEB_SITE = "https://ngrok.com/";

        /// <summary>
        /// Tools Directory name.
        /// </summary>
        private const string TOOLS_DIRECTORY_NAME = "Tools";

        /// <summary>
        /// Regex to get ngrok auth token.
        /// </summary>
        private const string REGEX_TO_GET_NGROK_AUTH_TOKEN = "(^|(ngrok authtoken ))(?<AUTHTOKEN>[0-9a-zA-Z_-]{16,100})";

        /// <summary>
        /// ngrok exe name.
        /// </summary>
        internal const string NGROK_EXE_NAME = "ngrok.exe";

        /// <summary>
        /// Tools Directory.
        /// </summary>
        internal static readonly Pathfinder TOOLS_DIRECTORY;

        static InitialSetupWindow()
        {
            TOOLS_DIRECTORY = App.UserDirectory.GetSubPathfinder(TOOLS_DIRECTORY_NAME);
        }

        public InitialSetupWindow()
        {
            InitializeComponent();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonOpenNgrokWeb_Click(object sender, RoutedEventArgs e)
        {
            var info = new ProcessStartInfo()
            {
                FileName = NGROK_WEB_SITE,
                UseShellExecute = true,
            };

            using var process = Process.Start(info);
        }

        private void ButtonOpenToolsDirectory_Click(object sender, RoutedEventArgs e)
        {
            if(!Directory.Exists(TOOLS_DIRECTORY.BaseDirectory))
            {
                Directory.CreateDirectory(TOOLS_DIRECTORY.BaseDirectory);
            }

            var info = new ProcessStartInfo()
            {
                FileName = "EXPLORER.EXE",
                Arguments = $"\"{TOOLS_DIRECTORY.BaseDirectory}\"",
                UseShellExecute = false,
            };

            using var process = Process.Start(info);
        }

        private void SetupNgrokAuthToken(string authtoken)
        {
            var info = new ProcessStartInfo()
            {
                FileName = TOOLS_DIRECTORY.FindPathName(NGROK_EXE_NAME),
                Arguments = $"authtoken {authtoken}",
                UseShellExecute = false,
            };

            using var process = Process.Start(info);
        }

        private void ButtonApply_Click(object sender, RoutedEventArgs e)
        {
            bool result = false;

            try
            {
                string authtoken = this.TextBoxNgrokAuthToken.Text;

                if (!String.IsNullOrWhiteSpace(authtoken))
                {
                    authtoken = authtoken.Trim();

                    var regex = new Regex(REGEX_TO_GET_NGROK_AUTH_TOKEN, RegexOptions.None, TimeSpan.FromMilliseconds(AppSettings.REGEX_TIMEOUT_NORMAL_IN_MILLISECONDS));

                    var match = regex.Match(authtoken);

                    if (match.Success)
                    {
                        authtoken = match.Groups["AUTHTOKEN"].Value;

                        SetupNgrokAuthToken(authtoken);

                        result = true;
                    }
                }
            }
            catch(RegexMatchTimeoutException rmte)
            {
                Log.Error(rmte, "Error on finding ngrok authtoken.");
            }
            catch(InvalidOperationException ioe)
            {
                Log.Error(ioe, "Error on setting up ngrok authtoken.");
            }

            string resultText;
            Brush  resultTextBrush;

            if(result)
            {
                resultText      = LocalizedInfo.TextBlockSetupNgrokAuthTokenSucceeded;
                resultTextBrush = new SolidColorBrush(Colors.RoyalBlue);
            }
            else
            {
                resultText      = LocalizedInfo.TextBlockSetupNgrokAuthTokenFailed;
                resultTextBrush = new SolidColorBrush(Colors.OrangeRed);
            }

            this.TextBlockSetupNgrokAuthTokenResult.Text       = resultText;
            this.TextBlockSetupNgrokAuthTokenResult.Foreground = resultTextBrush;
        }
    }
}
