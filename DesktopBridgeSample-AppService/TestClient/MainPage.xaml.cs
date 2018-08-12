using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TestClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        AppServiceConnection connection = null;
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if(connection == null)
            {
                btnConnect.IsEnabled = false;
                connection = new AppServiceConnection();
                connection.AppServiceName = "ContactAppService";
                connection.PackageFamilyName = "a902fecb-68df-48d2-b2db-6ca390dc3dfa_tqm640ggdzf82";
                var status = await connection.OpenAsync();
                btnConnect.IsEnabled = true;
                tbStatus.Text = "Status: " + status.ToString();
                if(status == AppServiceConnectionStatus.Success)
                {
                    tbStatus.Foreground = new SolidColorBrush(Colors.Green);
                    btnConnect.Content = "Disconnect";
                    btnQuery.IsEnabled = true;
                }
                else
                {
                    btnConnect.Content = "Cleanup";
                }
            }
            else
            {
                connection = null;
                tbStatus.Text = "Status: Closed";
                tbStatus.Foreground = new SolidColorBrush(Colors.Red);
                btnConnect.Content = "Connect";
                btnQuery.IsEnabled = false;
                tbName.Text = "";
                tbPhone.Text = "";
                tbState.Text = "";
                tbCity.Text = "";
            }
        }

        private async void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            ValueSet request = new ValueSet();
            request.Add("ID", tbId.Text);
            AppServiceResponse response = await connection.SendMessageAsync(request);
            if(response.Status == AppServiceResponseStatus.Success)
            {
                tbName.Text = "Name: " + response.Message["Name"].ToString();
                tbPhone.Text = "Phone: " + response.Message["Phone"].ToString();
                tbCity.Text = "City: " + response.Message["City"].ToString();
                tbState.Text = "State: " + response.Message["State"].ToString();
            }
        }
    }
}
