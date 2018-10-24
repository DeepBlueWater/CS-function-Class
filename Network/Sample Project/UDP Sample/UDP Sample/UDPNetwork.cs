using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Network
{
    class UDPNetworkEventArgs : EventArgs
    {
        public EndPoint remoteEP;
        public string Data;

        public UDPNetworkEventArgs(EndPoint _remoteEP, string _Data)
        {
            this.remoteEP = _remoteEP;
            this.Data = _Data;
        }
    }

    class UDPNetwork
    {
        public delegate void NetworkEventHandler(object sender, UDPNetworkEventArgs e);
        public event NetworkEventHandler Receiver;      //이벤트 정의

        private volatile Socket udpSocket;
        private volatile bool isRecieveStop = false;
        private Thread recieveThread;
        private int nRecievePort;
        private int nRecieveBufferSize = 4096;
        public int RecieveBufferSize
        {
            get
            {
                return nRecieveBufferSize;
            }
            set
            {
                nRecieveBufferSize = value;
            }
        }

        public UDPNetwork(int _RecievePort)
        {
            nRecievePort = _RecievePort;
        }

        public void Start()
        {
            isRecieveStop = false;
            recieveThread = new Thread(new ThreadStart(RecieveThread));
            recieveThread.IsBackground = true;
            recieveThread.Start();
        }

        public void Stop()
        {
            isRecieveStop = true;
            udpSocket.Shutdown(SocketShutdown.Both);
            udpSocket.Close();
            recieveThread.Join(1000);
        }

        private void RecieveThread()
        {
            try
            {
                udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                EndPoint localEP = new IPEndPoint(IPAddress.Any, nRecievePort);
                EndPoint remoteEP = new IPEndPoint(IPAddress.None, nRecievePort);
                udpSocket.Bind(localEP);

                byte[] receiveBuffer = new byte[nRecieveBufferSize];

                while (!isRecieveStop)
                {
                    int receivedSize = udpSocket.ReceiveFrom(receiveBuffer, ref remoteEP);

                    string str = Encoding.UTF8.GetString(receiveBuffer, 0, receivedSize);

                    if (Receiver != null)
                    {   
                        UDPNetworkEventArgs pArgs = new UDPNetworkEventArgs(remoteEP, str);
                        Receiver(this, pArgs);
                    }

                    Thread.Sleep(1);
                }
            }
            catch(SocketException se)
            {
                Console.WriteLine(se.Message);
            }
        }

        public void SendString(string _remoteIP, int _remotePort, string _Data)
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                IPAddress remoteIp = IPAddress.Parse(_remoteIP);
                IPEndPoint endPoint = new IPEndPoint(remoteIp, _remotePort);
                socket.Connect(endPoint);
                byte[] sBuffer = Encoding.UTF8.GetBytes(_Data);
                socket.Send(sBuffer, 0, sBuffer.Length, SocketFlags.None);
            }
        }
    }
}
