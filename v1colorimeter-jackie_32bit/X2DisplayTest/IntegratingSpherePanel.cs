using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;

namespace X2DisplayTest
{
    public partial class IntegratingSpherePanel : UserControl
    {
        public IntegratingSpherePanel(IntegratingSphere sphere)
        {
            InitializeComponent();
            this.sphere = sphere;
        }

        private IntegratingSphere sphere;

        private void DCPower3005Panel_Load(object sender, EventArgs e)
        {
            cbPort.Items.AddRange(SerialPort.GetPortNames());

            if (cbPort.Items.Contains(sphere.PortName)) {
                cbPort.SelectedText = sphere.PortName;
            }
            else {
                cbPort.SelectedIndex = 0;
            }

            tbVoltage.Text = sphere.Voltage.ToString();
            tbCurrent.Text = sphere.Current.ToString();
        }

        private void cbPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void btnGetValue_Click(object sender, EventArgs e)
        {
            int voltage = 0, current = 0;

            try {
                //dcpower.GetCurrentAndVoltage(ref voltage, ref current);
            }
            catch {
                voltage = int.MaxValue;
                current = int.MaxValue;
            }
            finally {
                lbCurrent.Text = current.ToString();
                lbVoltage.Text = voltage.ToString();
            }            
        }
    }
}
