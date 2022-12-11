using OpcLabs.EasyOpc.UA;
using OpcLabs.EasyOpc.UA.OperationModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void _tryRead()
        {
            try
            {
                this.Text = "...";
                UAAttributeData attributeData = null;
                UAEndpointDescriptor endpointDescriptor = "opc.tcp://localhost:49320";
                var client = new EasyUAClient();
                try
                {
                    attributeData = client.Read(endpointDescriptor, "ns=2;s=Channel1.BIENTAN.ap_suat");
                    this.Text = Convert.ToString(attributeData.Value);
                }
                catch (UAException uaException)
                {
                    var msg = uaException.GetBaseException().Message;
                    Debug.WriteLine($"*** Failure: {msg}");
                    this.Text = msg;
                }

                // Display results
                Debug.WriteLine($"Value: {attributeData?.Value}");
                Debug.WriteLine($"ServerTimestamp: {attributeData?.ServerTimestamp}");
                Debug.WriteLine($"SourceTimestamp: {attributeData?.SourceTimestamp}");
                Debug.WriteLine($"StatusCode: {attributeData?.StatusCode}");
            }
            catch (Exception ex)
            {
                this.Text = ex.Message;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            _tryRead();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                int value = Convert.ToInt32(textBox1.Text);
                UAEndpointDescriptor endpointDescriptor = "opc.tcp://127.0.0.1:49320";
                var client = new EasyUAClient();

                var data = client.WriteValue(endpointDescriptor, "ns=2;s=Channel1.BIENTAN.ap_suat", value);
                Console.WriteLine(data);
                MessageBox.Show("Success");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
