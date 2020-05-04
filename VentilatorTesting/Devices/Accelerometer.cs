using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
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
            if (deviceId != 0xE5)
            {
                throw new Exception("Doesn't look like the correct device is connected at the expected I2C address...");
            }

            // Set range to +/- 2g for max precision
            byte format = ReadRegister(SensorConstants.ACCEL_REG_DEV_ID);

            // Update the data rate
            format = (byte)(format & ~0x0F);
            format = (byte)(format | SensorConstants.ACCEL_RANGE_2_G);

            // Make sure that the FULL-RES bit is enabled for range scaling
            format = (byte)(format | 0x08);
            WriteRegister(SensorConstants.ACCEL_REG_DATA_FORMAT, format);

            // Enable measurements
            WriteRegister(SensorConstants.ACCEL_REG_POWER_CTL, 0x08);
        }

        // Read 1 byte from register
        private byte ReadRegister(byte register)
        {
            sensor.Write(new byte[] { register });
            byte[] buffer = new byte[1];
            sensor.Read(buffer);
            return buffer[0];
        }

        private short Read16bitRegister(byte register)
        {
            sensor.Write(new byte[] { register });
            byte[] buffer = new byte[2];
            sensor.Read(buffer);
            Debug.WriteLine(String.Join(", ", buffer));
            return (short)(buffer[0] | (buffer[1] << 8));
        }

        private void WriteRegister(byte register, byte data)
        {
            sensor.Write(new byte[] { register, data });
        }

        public byte GetDeviceID()
        {
            return ReadRegister(SensorConstants.ACCEL_REG_DEV_ID);
        }

        public float? GetAngle()
        {
            Debug.WriteLine("Reading accelerations...");

            try
            {
                float x_accel = Read16bitRegister(SensorConstants.ACCEL_X_REG)
                    * SensorConstants.ACCEL_MG2G;

                float y_accel = Read16bitRegister(SensorConstants.ACCEL_Y_REG)
                    * SensorConstants.ACCEL_MG2G;

                float z_accel = Read16bitRegister(SensorConstants.ACCEL_Z_REG)
                    * SensorConstants.ACCEL_MG2G;

                Debug.WriteLine("X: " + x_accel);
                Debug.WriteLine("Y: " + y_accel);
                Debug.WriteLine("Z: " + z_accel);

                // Calculate angle of vector from vertical (the z direction)
                Vector3 accel = new Vector3(x_accel, y_accel, z_accel);
                Vector3 vert = new Vector3(0, 0, -1);

                float angle = (float)Math.Acos(Vector3.Dot(accel, vert) / (accel.Length() * vert.Length()));

                Debug.WriteLine("Angle: " + angle);
                return angle;
            } catch (Exception e)
            {
                Debug.WriteLine("Error reading accelerometer data");
                Debug.WriteLine(e);
                return null;
            }

            return null;
        }

        public void Dispose()
        {
            // Sleep sensor
            WriteRegister(SensorConstants.ACCEL_REG_POWER_CTL, 0x00);
            if (sensor != null)
            {
                sensor.Dispose();
            }
            sensor = null;
        }
    }
}
