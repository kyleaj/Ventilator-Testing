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
            Debug.WriteLine(ReadRegister(sensor, SensorConstants.PRESS_ID_REG).ToString("x"));
            for (byte i = 0; i <= 0x2d; i++)
            {
                Debug.WriteLine("Reg: " + i.ToString("x"));
                Debug.WriteLine(ReadRegister(sensor, i).ToString("x"));
            }

            //return null;

            WriteRegister(sensor, SensorConstants.PRESS_MODE_REG, SensorConstants.PRESS_MODE_VAL);
            WriteRegister(sensor, SensorConstants.PRESS_EVENT_SET_REG, SensorConstants.PRESS_EVENT_SET_VAL);
            WriteRegister(sensor, SensorConstants.PRESS_MODE_REG, SensorConstants.PRESS_MODE_MEAS_VAL);

            Debug.WriteLine(ReadRegister(sensor, SensorConstants.PRESS_ID_REG).ToString("x"));

            Task.Run(async () =>
            {
                while (true)
                {
                    byte status = ReadRegister(sensor, 0x00);
                    if ((status & 0x04) == 0x04)
                    {
                        int pressure = Read32bitRegister(sensor, 0x01);
                        pressure = pressure >> 6;
                        Debug.WriteLine("Pressure: " + pressure);
                        for (byte i = 0; i <= 0x2d; i++)
                        {
                            Debug.WriteLine("Reg: " + i.ToString("x"));
                            Debug.WriteLine(ReadRegister(sensor, i).ToString("x"));
                        }
                    }
                    await Task.Delay(250);
                }
            });

            return null;
        }
    }
}
