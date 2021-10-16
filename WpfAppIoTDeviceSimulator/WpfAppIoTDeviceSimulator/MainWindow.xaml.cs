using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.DigitalTwins.Parser;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using System.Windows.Threading;

namespace WpfAppIoTDeviceSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            rand = new Random(DateTime.Now.Millisecond);
            this.Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            lbTPs.ItemsSource = sensorParameters;

            var cd = Directory.GetCurrentDirectory();
            var di = new DirectoryInfo(cd + "\\models\\pnp");
            var modelJson = new List<string>();
            foreach (var f in di.GetFiles())
            {
                ShowLog($"Add DTDLFile: {f.Name} ");
                using (var fs = File.OpenRead(f.FullName))
                {
                    using (var reader = new StreamReader(fs))
                    {
                        modelJson.Add(reader.ReadToEnd());
                    }
                }
            }
            deviceDTDLParser = new DeviceDTDLParser();
            try
            {
                ShowLog("Parsing...");
                await deviceDTDLParser.ParseDTDLModel(modelJson);
                foreach(var tpSpec in deviceDTDLParser.TelemetryParameterSpecs)
                {
                    tsNodeRepository.Add(tpSpec);
                    ShowLog($"Parsed - {tpSpec.Name}");
                }
#if test
                tsNodeRepository.Update();
                var json = tsNodeRepository.GetJSON();
#endif
                foreach (var key in tsNodeRepository.TelemetryParameters.Keys)
                {
                    var tp = tsNodeRepository.TelemetryParameters[key];
                    if (!tp.IsTimestamp())
                    {
                        sensorParameters.Add(tp);
                    }
                }

            }

            catch(Exception ex)
            {
                ShowLog(ex.Message);
            }
            ShowLog("PnP definition parse done.");
        }

        DeviceDTDLParser deviceDTDLParser;
        TSNodeRepository tsNodeRepository = new TSNodeRepository();
        ObservableCollection<TelemetryParameter> sensorParameters = new ObservableCollection<TelemetryParameter>();
               void ShowLog(string log)
        {
            var sb = new StringBuilder();
            var writer = new StringWriter(sb);
            writer.WriteLine($"{DateTime.Now.ToString("yyyyMMdd-HHmmss")}: {log}");
            writer.Write(tbLog.Text);
            tbLog.Text = sb.ToString();
        }

        DeviceClient deviceClient;
        Random rand;
        string deviceId;

        private async void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbIoTHubCS.Text))
            {
                MessageBox.Show("Please set IoT Hub Device Connection String!");
                return;
            }
            deviceClient = DeviceClient.CreateFromConnectionString(tbIoTHubCS.Text);
            await deviceClient.OpenAsync();
            var csBuilder = IotHubConnectionStringBuilder.Create(tbIoTHubCS.Text);
            deviceId = csBuilder.DeviceId;
            ShowLog($"Connected to IoT Hub as {deviceId}");
            buttonSendingControl.IsEnabled = true;
        }

        double currentValue = 0.0f;


        DispatcherTimer timer = null;
        private void buttonSendingControl_Click(object sender, RoutedEventArgs e)
        {
            if (timer == null)
            {
                timer = new DispatcherTimer();
                timer.Tick += Timer_Tick;
            }
            if (timer.IsEnabled)
            {
                timer.Stop();
                ShowLog("Stop sending");
                buttonSendingControl.Content = "Send Start";
            }
            else
            {
                timer.Interval = TimeSpan.FromMilliseconds(int.Parse(tbSendInterval.Text));
                timer.Start();
                ShowLog("Start sending");
                buttonSendingControl.Content = "Send Stop";
            }
        }

        private async void Timer_Tick(object sender, EventArgs e)
        {
            tsNodeRepository.Update();
            var msg = tsNodeRepository.GetMessageJSON();
            var msgBytes = System.Text.Encoding.UTF8.GetBytes(msg);
            var iothubMsg = new Message(msgBytes);
            await deviceClient.SendEventAsync(iothubMsg);
            ShowLog($"Message Send - {msg},{msgBytes.Length} bytes");
        }

        private void lbTPs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbTPs.SelectedItem is DeltaPropTelemetryParameter)
            {
                var pane = new DeltaPropPan((DeltaPropTelemetryParameter)lbTPs.SelectedItem);
                pane.Show();
            }
            else if (lbTPs.SelectedItem is CircularTelemetryParameter)
            {
                var pane = new CircularPane((CircularTelemetryParameter)lbTPs.SelectedItem);
                pane.Show();
            }
            else if (lbTPs.SelectedItem is DiscreteTelemetryParameter)
            {
                var pane = new DiscretePane((DiscreteTelemetryParameter)lbTPs.SelectedItem);
                pane.Show();
            }
            else if (lbTPs.SelectedItem is EnumTelemetryParameter)
            {
                var pane = new EnumPane((EnumTelemetryParameter)lbTPs.SelectedItem);
                pane.Show();
            }

        }

        TSITypeGenerator typeGenerator;
        private void buttonGenTSIType_Click(object sender, RoutedEventArgs e)
        {
            string id = Guid.NewGuid().ToString();
            typeGenerator = new TSITypeGenerator(id, deviceDTDLParser.Id);
            string gen = typeGenerator.GenerateTypeDef(deviceDTDLParser.TelemetryParameterSpecs);
            var fileWindow = new TSIModelFile(gen);
            fileWindow.Show();

        }

        private void buttonGenTSIInstance_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
