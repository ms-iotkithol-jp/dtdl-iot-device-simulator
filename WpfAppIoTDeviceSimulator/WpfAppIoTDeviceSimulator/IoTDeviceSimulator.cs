using Microsoft.Azure.Devices.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppIoTDeviceSimulator
{
    class IoTDeviceSimulator
    {
        DeviceClient deviceClient;
        string connectionString;
        public string DeviceId { get; set; }
        public IoTDeviceSimulator(string cs)
        {
            connectionString = cs;
            var builder = IotHubConnectionStringBuilder.Create(cs);
            DeviceId = builder.DeviceId;
        }

        TSNodeRepository tsNodeRepository = new TSNodeRepository();
        public void Add(TelemetryPrameterSpec tps)
        {
            tsNodeRepository.Add(tps);
        }

        public Dictionary<string, TelemetryParameter> TelemetryParameters
        {
            get { return tsNodeRepository.TelemetryParameters; }
        }

        public async Task Connect()
        {
            if (deviceClient == null)
            {
                deviceClient = DeviceClient.CreateFromConnectionString(connectionString);
            }
            await deviceClient.OpenAsync();

        }

        public async Task Disconnect()
        {
            if (deviceClient != null)
            {
                await deviceClient.CloseAsync();
            }
        }

        public async Task<byte[]> Send()
        {
            tsNodeRepository.Update();
            var msg = tsNodeRepository.GetMessageJSON();
            var msgBytes = System.Text.Encoding.UTF8.GetBytes(msg);
            var iothubMsg = new Message(msgBytes);
            await deviceClient.SendEventAsync(iothubMsg);
            return msgBytes;
        }
    }
}
