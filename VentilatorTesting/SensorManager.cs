using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VentilatorTesting.Devices;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using Windows.System.Threading;

namespace VentilatorTesting
{
    public class SensorManager : IDisposable
    {
        private Accelerometer AccP1; // Accelerometer for Patient 1
        private ThreadPoolTimer AccPoll; // Timer to poll accelerometer data

        private SensorManager(DeviceInformation i2cBus)
        {
            AccP1 = new Accelerometer(i2cBus, Enums.Patient.A);
            AccPoll = ThreadPoolTimer.CreatePeriodicTimer(PollAcelerometer,
                new TimeSpan(0, 0, 0, 0, (int)(1000 * SensorConstants.ACCEL_POLL_FREQ)));
        }

        private void PollAcelerometer(ThreadPoolTimer timer)
        {
            Debug.WriteLine("Poll timer tick");
            AccP1.GetAngle();
        }

        public static async Task<SensorManager> CreateSensorManager()
        {
            string i2cDeviceSelector = I2cDevice.GetDeviceSelector();
            IReadOnlyList<DeviceInformation> devices = await DeviceInformation.FindAllAsync(i2cDeviceSelector);
            if (devices.Count != 1)
            {
                Debug.WriteLine("Ambiguous/no i2c bus(device)es: " + devices.Count);
            }
            return new SensorManager(devices[0]);
        }

        public void Dispose()
        {
            if (AccP1 != null)
            {
                AccP1.Dispose();
                AccP1 = null;
            }
            if (AccPoll != null)
            {
                AccPoll.Cancel();
                AccPoll = null;
            }
        }
    }
}
