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
using Windows.UI.Xaml;

namespace VentilatorTesting
{
    public class SensorManager : IDisposable
    {
        private Accelerometer AccP1; // Accelerometer for Patient 1
        private Accelerometer AccP2; // Accelerometer for Patient 2
        private Barometer BarP1; // Accelerometer for Patient 1
        private Barometer BarP2; // Accelerometer for Patient 1
        private ThreadPoolTimer SensePoll; // Timer to poll accelerometer data
        private Test currTest;
        private float AngleToVolumeConversion;

        private SensorManager(DeviceInformation i2cBus)
        {
            currTest = null;
            AccP1 = new Accelerometer(i2cBus, Enums.Patient.A);
            BarP1 = new Barometer(i2cBus, Enums.Patient.A);

            SensePoll = ThreadPoolTimer.CreatePeriodicTimer(PollSensors,
                new TimeSpan(0, 0, 0, 0, (int)(1000 * 1 / SensorConstants.ACCEL_POLL_FREQ)));
            
            AngleToVolumeConversion = 1F/90; // Calculate later
        }

        private void PollSensors(ThreadPoolTimer timer)
        {
            float? angle = AccP1.GetAngle();
            if (currTest != null)
            {
                if (angle == null)
                {
                    // Alert user
                }
                else
                {
                    float volume = AngleToVolumeConversion * (float)angle;
                    currTest.VolumeData.Add(volume);
                }
            }

            float pressure = BarP1.GetPressure();
            Debug.WriteLine("Pressure: " + pressure);

            if ((Application.Current as App).ComService != null)
            {
                (Application.Current as App).ComService.SendVolumeUpdate(AngleToVolumeConversion * (float)angle, Enums.Patient.A);
            }
        }

        public bool StartTest(string testName, int durationSeconds)
        {
            if (currTest != null)
            {
                return false;
            }
            currTest = new Test(testName);
            ThreadPoolTimer.CreateTimer((timer) => { StopTest(currTest); }, new TimeSpan(0, 0, durationSeconds));
            return true;
        }

        public void StopTest()
        {
            StopTest(currTest);
        }

        public async void StopTest(Test test)
        {
            if (currTest == test)
            {
                currTest = null;
            }
            if (!test.Running) return;
            test.Running = false;
            await test.PressureData.FlushList();
            await test.VolumeData.FlushList();
            // Anything else to do, you think?
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
            if (BarP1 != null)
            {
                BarP1.Dispose();
                BarP1 = null;
            }
            if (AccP2 != null)
            {
                AccP2.Dispose();
                AccP2 = null;
            }
            if (BarP2 != null)
            {
                BarP2.Dispose();
                BarP2 = null;
            }
            if (SensePoll != null)
            {
                SensePoll.Cancel();
                SensePoll = null;
            }
        }
    }
}
