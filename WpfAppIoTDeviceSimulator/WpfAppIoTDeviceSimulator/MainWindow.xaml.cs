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

            }

            catch(Exception ex)
            {
                ShowLog(ex.Message);
            }
            ShowLog("PnP definition parse done.");
        }

        DeviceDTDLParser deviceDTDLParser;
        ObservableCollection<TelemetryParameter> sensorParameters = new ObservableCollection<TelemetryParameter>();
               void ShowLog(string log)
        {
            var sb = new StringBuilder();
            var writer = new StringWriter(sb);
            writer.WriteLine($"{DateTime.Now.ToString("yyyyMMdd-HHmmss")}: {log}");
            writer.Write(tbLog.Text);
            tbLog.Text = sb.ToString();
        }

        IoTDeviceSimulator deviceSimulator;
        private async void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbIoTHubCS.Text))
            {
                MessageBox.Show("Please set IoT Hub Device Connection String!");
                return;
            }
            deviceSimulator = new IoTDeviceSimulator(tbIoTHubCS.Text);
            await deviceSimulator.Connect();
            
            ShowLog($"Connected to IoT Hub as {deviceSimulator.DeviceId}");

            foreach (var tpSpec in deviceDTDLParser.TelemetryParameterSpecs)
            {
                deviceSimulator.Add(tpSpec);
                ShowLog($"Parsed - {tpSpec.Name}");
            }
#if test
                tsNodeRepository.Update();
                var json = tsNodeRepository.GetJSON();
#endif
            foreach (var key in deviceSimulator.TelemetryParameters.Keys)
            {
                var tp = deviceSimulator.TelemetryParameters[key];
                if (!tp.IsTimestamp())
                {
                    sensorParameters.Add(tp);
                }
            }


            buttonSendingControl.IsEnabled = true;
        }


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
            var msgContent = await deviceSimulator.Send();
            ShowLog($"Message Send - {System.Text.Encoding.UTF8.GetString(msgContent)},{msgContent.Length} bytes");
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
