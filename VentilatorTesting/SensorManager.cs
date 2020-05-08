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

        private float CurrPeep1;
        private float NextPeep1;
        private float CurrPeep2;
        private float NextPeep2;

        private int NextValueBecomesCurrValue;
        private int PollCounter;

        private float PrevVolume1;
        private float PrevVolume2;
        private float VolDeriv1;
        private float VolDeriv2;

        private float MaxVol1;
        private float MinVol1;
        private float MaxVol2;
        private float MinVol2;

        private float CurrMaxVol1;
        private float CurrMinVol1;
        private float CurrMaxVol2;
        private float CurrMinVol2;

        private bool InhaleExhale; // Inhaling = true
        private int LastChange1; // Poll counter value
        private int LastChange2; // Poll counter value
        private int LastDiff1;
        private int LastDiff2;

        private SensorManager(DeviceInformation i2cBus)
        {
            currTest = null;
            AccP1 = null;
            BarP1 = null;
            AccP2 = null;
            BarP2 = null;

            CurrPeep1 = 0;
            CurrPeep2 = 0;

            NextPeep1 = float.MaxValue;
            NextPeep2 = float.MaxValue;

            InhaleExhale = true;
            LastChange1 = -1;
            LastChange2 = -1;
            PrevVolume1 = 0;
            PrevVolume2 = 0;

            VolDeriv1 = 0;
            VolDeriv2 = 0;

            MaxVol1 = 0;
            MaxVol2 = 0;
            MinVol1 = 0;
            MinVol2 = 0;

            CurrMaxVol1 = float.MinValue;
            CurrMaxVol2 = float.MinValue;
            CurrMinVol1 = float.MaxValue;
            CurrMinVol2 = float.MaxValue;

            LastDiff1 = 1;
            LastDiff2 = 1;

            PollCounter = 0;

            NextValueBecomesCurrValue = (int)(30 / (1 / SensorConstants.ACCEL_POLL_FREQ));

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

            if (pressure1 < NextPeep1)
            {
                NextPeep1 = (float)pressure1;
            }

            if (pressure2 < NextPeep2)
            {
                NextPeep2 = (float)pressure2;
            }

            if (angle1 != null)
            {
                float volume = AngleToVolumeConversion * (float)angle1;
                float deriv = volume - PrevVolume1;
                PrevVolume1 = volume;
                VolDeriv1 = (VolDeriv1 + deriv) / 2;

                if (volume < CurrMinVol1)
                {
                    CurrMinVol1 = volume;
                }
                if (volume > CurrMaxVol1)
                {
                    CurrMaxVol1 = volume;
                }

                if (InhaleExhale) // Inhaling
                {
                    if (VolDeriv1 < -0.001)
                    {
                        InhaleExhale = false;
                        MaxVol1 = CurrMaxVol1;
                        CurrMinVol1 = float.MaxValue;
                        var lastChange = LastChange1;
                        LastChange1 = PollCounter;
                        var currDiff = PollCounter - lastChange; // Time to inhale
                        var ie = currDiff / ((float)LastDiff1);
                        LastDiff1 = currDiff;
                        (Application.Current as App)?.ComService?.SendIEUpdate(ie, Enums.Patient.A);
                        (Application.Current as App)?.ComService?.SendTVUpdate(MaxVol1 - MinVol1, Enums.Patient.A);
                    }
                }
                else // Exhaling
                {
                    if (VolDeriv1 > 0.001)
                    {
                        InhaleExhale = true;
                        MinVol1 = CurrMinVol1;
                        CurrMaxVol1 = float.MinValue;
                        var lastChange = LastChange1;
                        LastChange1 = PollCounter;
                        var currDiff = PollCounter - lastChange; // Time to inhale
                        var ie = ((float)LastDiff1) / currDiff;
                        LastDiff1 = currDiff;
                        (Application.Current as App)?.ComService?.SendIEUpdate(ie, Enums.Patient.A);
                        (Application.Current as App)?.ComService?.SendTVUpdate(MaxVol1 - MinVol1, Enums.Patient.A);
                    }
                }
            }

            if (angle2 != null)
            {
                float volume = AngleToVolumeConversion * (float)angle2;
                float deriv = volume - PrevVolume2;
                PrevVolume2 = volume;
                VolDeriv2 = (VolDeriv2 + deriv) / 2;

                if (volume < CurrMinVol2)
                {
                    CurrMinVol2 = volume;
                }
                if (volume > CurrMaxVol2)
                {
                    CurrMaxVol2 = volume;
                }

                if (InhaleExhale) // Inhaling
                {
                    if (VolDeriv2 < -0.001)
                    {
                        InhaleExhale = false;
                        MaxVol2 = CurrMaxVol2;
                        CurrMinVol2 = float.MaxValue;
                        var lastChange = LastChange2;
                        LastChange2 = PollCounter;
                        var currDiff = PollCounter - lastChange; // Time to inhale
                        var ie = currDiff / ((float)LastDiff2);
                        LastDiff2 = currDiff;
                        (Application.Current as App)?.ComService?.SendIEUpdate(ie, Enums.Patient.B);
                        (Application.Current as App)?.ComService?.SendTVUpdate(MaxVol2 - MinVol2, Enums.Patient.B);
                    }
                }
                else // Exhaling
                {
                    if (VolDeriv2 > 0.001)
                    {
                        InhaleExhale = true;
                        MinVol2 = CurrMinVol2;
                        CurrMaxVol2 = float.MinValue;
                        var lastChange = LastChange2;
                        LastChange2 = PollCounter;
                        var currDiff = PollCounter - lastChange; // Time to inhale
                        var ie = ((float)LastDiff2) / currDiff;
                        LastDiff2 = currDiff;
                        (Application.Current as App)?.ComService?.SendIEUpdate(ie, Enums.Patient.B);
                        (Application.Current as App)?.ComService?.SendTVUpdate(MaxVol2 - MinVol2, Enums.Patient.B);
                    }
                }
            }

            if (PollCounter % NextValueBecomesCurrValue == 0)
            {
                CurrPeep1 = NextPeep1;
                CurrPeep2 = NextPeep2;

                (Application.Current as App)?.ComService?.SendPeepUpdate(CurrPeep1, Enums.Patient.A);
                (Application.Current as App)?.ComService?.SendPeepUpdate(CurrPeep2, Enums.Patient.B);

                NextPeep1 = float.MaxValue;
                NextPeep2 = float.MaxValue;
            }

            PollCounter++;
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
