using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace X2DisplayTest
{
    public class IntegratingSphere : IDevice
    {
        public IntegratingSphere(IDevice fixture, string portname)
        {
            ReadProfile();
            this.fixture = fixture as Fixture;
            power = new DCPower3005(portname);
            this.portName = portName;
        }

        private Fixture fixture;
        private DCPower3005 power;

        private string portName;
        public string PortName
        {
            get
            {
                return portName;
            }
            set
            {
                if (value == null || !Regex.IsMatch(value, @"^COM\d{1,3}$"))
                    portName = "";
                else
                    portName = value;
            }
        }

        private int voltage;
        public int Voltage
        {
            get { return voltage; }
        }

        private int current;
        public int Current
        {
            get { return current; }
        }

        public void Lighten()
        {
            this.Lighten(5000, 1500);
        }

        public void Lighten(int voltage, int current)
        {
            power.SetControlValue(voltage, true);
            power.SetControlValue(current, false);
            power.SetOutputStatus(true);
            this.voltage = voltage;
            this.current = current;
        }

        public void Lightoff()
        {
            Lighten(0, 0);
            power.SetOutputStatus(false);
        }

        public void MoveTestPos()
        {
            fixture.IntegratingSphereUp();
            System.Threading.Thread.Sleep(100);
            power.SetControlValue(5500, true);
            power.SetControlValue(1540, false);
            power.SetOutputStatus(true);
            voltage = 5500;
            current = 1540;
        }

        public void MoveReadyPos()
        {
            power.SetOutputStatus(false);
            power.SetControlValue(0, true);
            power.SetControlValue(0, false);
            voltage = 0;
            current = 0;
            fixture.IntegratingSphereDown();            
        }

        /// <summary>
        /// unit mA/mV
        /// </summary>
        /// <param name="voltage"></param>
        /// <param name="current"></param>
        public void ChangeTestParam(int voltage, int current)
        {
            power.SetControlValue(voltage, true);
            power.SetControlValue(current, false);
            power.SetOutputStatus(true);
            this.voltage = voltage;
            this.current = current;
        }


        protected override void ReadProfile()
        {
            try
            {
                string segName = this.GetType().Name;
                this.PortName = fileHandle.ReadString(segName, "portname");
                this.voltage = fileHandle.ReadInt(segName, "voltage");
                this.current = fileHandle.ReadInt(segName, "current");
            }
            catch
            {
                this.PortName = "";
                this.voltage = this.current = 0;
                this.WriteProfile();
            }
        }

        protected override void WriteProfile()
        {
            string segName = this.GetType().Name;
            fileHandle.WriteString(segName, "portname", this.PortName);
            fileHandle.WriteDouble(segName, "voltage", this.voltage);
            fileHandle.WriteDouble(segName, "current", this.current);
        }

        public override System.Windows.Forms.Control DeviceConfigPanel
        {
            get
            {
                if (this.panel == null)
                {
                    this.panel = new IntegratingSpherePanel(this);
                }

                return this.panel;
            }
            protected set
            {
                this.panel = value;
            }
        }
    }
}
