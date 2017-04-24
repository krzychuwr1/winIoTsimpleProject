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
using Windows.Media.SpeechRecognition;
using Windows.ApplicationModel;
using System.Diagnostics;
using Windows.Storage;
using Windows.UI.Core;
using System.Threading.Tasks;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RaspberryApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private SpeechRecognizer recognizer;

        private DeviceClient deviceClient;

        private const string SRGS_FILE = "Grammar\\grammar.xml";

        string iotHubUri = ""; // value from azure iot hub needed
        string deviceId = ""; // value from azure iot hub needed
        string deviceKey = ""; // value from azure iot hub needed

        public MainPage()
        {
            this.InitializeComponent();

            Unloaded += MainPage_Unloaded;

            initializeSpeechRecognizer();

            initializeAzureClient();
        }

        private void initializeAzureClient()
        {
            if(string.IsNullOrEmpty(iotHubUri) || string.IsNullOrEmpty(deviceId) || string.IsNullOrEmpty(deviceKey))
            {
                Debug.WriteLine("Azure key needed");
                return;
            }
            deviceClient =
                DeviceClient.Create(
                    iotHubUri,
                    AuthenticationMethodFactory.
                    CreateAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey),
                    TransportType.Amqp
                    );
        }

        private async void initializeSpeechRecognizer()
        {
            // Initialize recognizer
            recognizer = new SpeechRecognizer();

            // Set event handlers
            recognizer.StateChanged += RecognizerStateChanged;
            recognizer.ContinuousRecognitionSession.ResultGenerated += RecognizerResultGenerated;

            // Load Grammer file constraint
            string fileName = String.Format(SRGS_FILE);
            StorageFile grammarContentFile = await Package.Current.InstalledLocation.GetFileAsync(fileName);

            SpeechRecognitionGrammarFileConstraint grammarConstraint = new SpeechRecognitionGrammarFileConstraint(grammarContentFile);

            // Add to grammer constraint
            recognizer.Constraints.Add(grammarConstraint);

            // Compile grammer
            SpeechRecognitionCompilationResult compilationResult = await recognizer.CompileConstraintsAsync();

            Debug.WriteLine("Status: " + compilationResult.Status.ToString());

            // If successful, display the recognition result.
            if (compilationResult.Status == SpeechRecognitionResultStatus.Success)
            {
                Debug.WriteLine("Result: " + compilationResult.ToString());

                await recognizer.ContinuousRecognitionSession.StartAsync();
            }
            else
            {
                Debug.WriteLine("Status: " + compilationResult.Status);
            }
        }

        private async void RecognizerResultGenerated(SpeechContinuousRecognitionSession session, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            // Output debug strings
            Debug.WriteLine(args.Result.Status);
            Debug.WriteLine(args.Result.Text);
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                this.Message.Text = args.Result.Text;
            }
            );

            await sendStringToAzure(args.Result.Text);
        }

        private async Task sendStringToAzure(string text)
        {
            if(deviceClient != null)
            {
                var message = new Message(Encoding.ASCII.GetBytes(text));
                await deviceClient.SendEventAsync(message);
            }
            else
            {
                Debug.WriteLine("Azure DeviceClient has not been initialized");
            }
        }

        private void RecognizerStateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            Debug.WriteLine("Speech recognizer state: " + args.State.ToString());
        }

        private async void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            await recognizer.ContinuousRecognitionSession.StopAsync();
        }

        private async void Button_Click(object sender, RoutedEventArgs e) => await sendStringToAzure(Message.Text);
    }
}
