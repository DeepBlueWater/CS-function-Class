using System;
using System.Windows.Forms;

namespace UDP_Sample
{
    public partial class FormMain : Form
    {
        Network.UDPNetwork udpNetwork;

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, System.EventArgs e)
        {
            udpNetwork = new Network.UDPNetwork(4001);
            udpNetwork.Receiver += UdpNetwork_Receiver;
            udpNetwork.Start();

            udpNetwork.SendString("192.168.0.231", 4001, "TEST Send Message");
        }

        private void UdpNetwork_Receiver(object sender, Network.UDPNetworkEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            udpNetwork.Stop();
        }
    }
}
