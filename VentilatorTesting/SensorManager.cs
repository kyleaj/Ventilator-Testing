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
            AccP1 = null;
            BarP1 = null;
            AccP2 = null;
            BarP2 = null;

            try
            {
                AccP1 = new Accelerometer(i2cBus, Enums.Patient.A);
            } catch (Exception e)
            {
                Debug.WriteLine("Could not create accelerometer instance");
                Debug.WriteLine(e);
            }

            try
            {
                AccP2 = new Accelerometer(i2cBus, Enums.Patient.B);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Could not create accelerometer instance");
                Debug.WriteLine(e);
            }

            try
            {
                BarP1 = new Barometer(i2cBus, Enums.Patient.A);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Could not create accelerometer instance");
                Debug.WriteLine(e);
            }

            try
            {
                BarP2 = new Barometer(i2cBus, Enums.Patient.B);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Could not create accelerometer instance");
                Debug.WriteLine(e);
            }

            SensePoll = ThreadPoolTimer.CreatePeriodicTimer(PollSensors,
                new TimeSpan(0, 0, 0, 0, (int)(1000 * 1 / SensorConstants.ACCEL_POLL_FREQ)));
            
            AngleToVolumeConversion = 0.5F/90; // Calculate later
        }

        private void PollSensors(ThreadPoolTimer timer)
        {
            float? angle1 = AccP1?.GetAngle();
            float? pressure1 = BarP1?.GetPressure();

            float? angle2 = AccP2?.GetAngle();
            float? pressure2 = BarP2?.GetPressure();

            if (currTest != null)
            {
                if (angle1 == null)
                {
                    // Alert user
                }
                else
                {
                    float volume = AngleToVolumeConversion * (float)angle1;
                    currTest.VolumeDataP1.Add(volume);
                }
                if (angle2 == null)
                {
                    // Alert user
                }
                else
                {
                    float volume = AngleToVolumeConversion * (float)angle2;
                    currTest.VolumeDataP2.Add(volume);
                }
                if (pressure1 == null)
                {
                    // Alert user
                }
                else
                {
                    currTest.PressureDataP1.Add((float)pressure1);
                }
                if (pressure2 == null)
                {
                    // Alert user
                }
                else
                {
                    currTest.PressureDataP2.Add((float)pressure2);
                }
            }

            if ((Application.Current as App).ComService != null)
            {
                angle1 = angle1 == null ? 0 : angle1;
                (Application.Current as App).ComService.SendVolumeUpdate(AngleToVolumeConversion * (float)angle1, Enums.Patient.A);
                angle2 = angle2 == null ? 0 : angle2;
                (Application.Current as App).ComService.SendVolumeUpdate(AngleToVolumeConversion * (float)angle2, Enums.Patient.B);
                pressure1 = pressure1 == null ? 0 : pressure1;
                (Application.Current as App).ComService.SendPressureUpdate((float)pressure1, Enums.Patient.A);
                pressure2 = pressure2 == null ? 0 : pressure2;
                (Application.Current as App).ComService.SendPressureUpdate((float)pressure2, Enums.Patient.B);
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
            await test.PressureDataP1.FlushList();
            await test.VolumeDataP1.FlushList();
            await test.PressureDataP2.FlushList();
            await test.VolumeDataP2.FlushList();
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
