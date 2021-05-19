using LiveSounds.Api;
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
using System.Windows.Shapes;

namespace LiveSounds
{
    /// <summary>
    /// Interaction logic for TokenSettingWindow.xaml
    /// </summary>
    public partial class TokenSettingWindow : Window
    {

        public TokenSettingWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Applies token setting.
        /// </summary>
        private void Apply()
        {
            try
            {
                string token = this.TextBoxTokenInfo.Text;

                if (token != null)
                {
                    token = token.Trim(new char[] { '"', ' ' });

                    App.Settings.Token = token;

                    if (ZokmaApi.CheckTokenValid(token))
                    {
                        this.DialogResult = true;
                    }
                }
            }
            finally
            {
                this.Close();
            }
        }

        /// <summary>
        /// Cancels token setting.
        /// </summary>
        private void Cancel()
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.TextBoxTokenInfo.Text = App.Settings.Token;
        }

        private void ButtonApply_Click(object sender, RoutedEventArgs e)
        {
            Apply();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Cancel();
        }
    }
}
