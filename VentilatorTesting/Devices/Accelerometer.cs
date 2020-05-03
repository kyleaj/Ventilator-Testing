using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VentilatorTesting.Enums;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace VentilatorTesting.Devices
{
    class Accelerometer : IDisposable
    {
        private I2cDevice sensor;

        // Reference: https://github.com/adafruit/Adafruit_ADXL345/blob/master/Adafruit_ADXL345_U.cpp
        public Accelerometer(DeviceInformation deviceInformation, Patient patient)
        {
            Debug.WriteLine("Creating Accelerometer");
            if (deviceInformation == null)
            {
                throw new ArgumentNullException("Device Information cannot be null!");
            }
            short address;
            if (patient == Patient.A)
            {
                address = SensorConstants.ACCEL_ADDR_P_A;
            } else
            {
                address = SensorConstants.ACCEL_ADDR_P_B;
            }

            try
            {
                sensor = I2cDevice.FromIdAsync(deviceInformation.Id, 
                    new I2cConnectionSettings(address) { BusSpeed=I2cBusSpeed.StandardMode }).AsTask().GetAwaiter().GetResult();
            } catch(Exception e)
            {
                Debug.WriteLine("Could not open accelerometer");
                Debug.WriteLine(e);
                throw e;
            }
            InitializeSensor();
        }

        private void InitializeSensor()
        {
            Debug.WriteLine("Initializing sensor...");
            byte deviceId = GetDeviceID();
            Debug.WriteLine("Device id correct: " + (deviceId == 0xE5));
        }

        // Read 1 byte from register
        private byte ReadRegister(byte register)
        {
            sensor.Write(new byte[] { register });
            byte[] buffer = new byte[1];
            sensor.Read(buffer);
            return buffer[0];
        }

        public byte GetDeviceID()
        {
            return ReadRegister(SensorConstants.ACCEL_REG_DEV_ID);
        }

        public float? GetAngle()
        {
            var command = new byte[1];
            var humidityData = new byte[2];
            var temperatureData = new byte[2];

            

            return null;
        }

        public void Dispose()
        {
            if (sensor != null)
            {
                sensor.Dispose();
            }
            sensor = null;
        }
    }
}
