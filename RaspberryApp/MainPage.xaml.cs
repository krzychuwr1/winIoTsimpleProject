using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Azure.Devices.Client;
using System.Text;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RaspberryApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            string iotHubUri = ""; // value from azure iot hub needed
            string deviceId = ""; // value from azure iot hub needed
            string deviceKey = ""; // value from azure iot hub needed

            var deviceClient = 
                DeviceClient.Create(
                    iotHubUri,
                    AuthenticationMethodFactory.
                    CreateAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey),
                    TransportType.Amqp
                    );

            var str = Message.Text;
            var message = new Message(Encoding.ASCII.GetBytes(str));
            await deviceClient.SendEventAsync(message);        }   
    }
}
