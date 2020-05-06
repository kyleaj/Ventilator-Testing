using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;

namespace VentilatorTesting
{
    class Barometer : Sensor
    {
        private GpioPin Pin;

        private Barometer(GpioPin interruptPin)
        {
            Pin = interruptPin;
        }

        // Return true if the pressure is less than the Peep pressure desired
        public bool PeepViolated()
        {
            return Pin.Read() == GpioPinValue.High;
        }

        public static Tuple<Barometer, Barometer> GetBarometers(DeviceInformation deviceInformation, float peep)
        {
            Debug.WriteLine("Creating Barometer");
            if (deviceInformation == null)
            {
                throw new ArgumentNullException("Device Information cannot be null!");
            }

            I2cDevice sensor;
            try
            {
                sensor = I2cDevice.FromIdAsync(deviceInformation.Id,
                    new I2cConnectionSettings(
                        SensorConstants.PRESS_ADDR) { BusSpeed = I2cBusSpeed.StandardMode })
                        .AsTask().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Could not open barometer");
                Debug.WriteLine(e);
                throw e;
            }
            Debug.WriteLine("Writing to registers");
            WriteRegister(sensor, SensorConstants.PRESS_MODE_REG, SensorConstants.PRESS_MODE_VAL);
            WriteRegister(sensor, SensorConstants.PRESS_EVENT_SET_REG, SensorConstants.PRESS_EVENT_SET_VAL);
            WriteRegister(sensor, SensorConstants.PRESS_MODE_REG, SensorConstants.PRESS_MODE_MEAS_VAL);
            Debug.WriteLine("Wrote to registers");

            Task.Run(async () =>
            {
                Debug.WriteLine("Starting loop");
                while (true)
                {
                    Debug.WriteLine("Reading status");
                    byte status = ReadRegister(sensor, 0x00);
                    Debug.WriteLine("Status: " + status);
                    if ((status & 0x04) == 0x04)
                    {
                        int pressure = Read32bitRegister(sensor, 0x01);
                        pressure = pressure >> 4;
                        Debug.WriteLine(pressure);
                    }
                    await Task.Delay(250);
                }
            });

            return null;
        }
    }
}
