using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Windows;

namespace WPFSocketClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Socket mainSock;
        int m_port = 5001;
        public MainWindow()
        {
            InitializeComponent();

            Connect();
        }

        public void Connect()
        {
            mainSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress serverAddr = IPAddress.Parse("220.118.53.92");
            IPEndPoint clientEP = new IPEndPoint(serverAddr, m_port);
            mainSock.BeginConnect(clientEP, new AsyncCallback(ConnectCallback), mainSock);
        }

        public void Close()
        {
            if (mainSock != null)
            {
                mainSock.Close();
                mainSock.Dispose();
            }
        }

        void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndConnect(ar);
                AsyncObject obj = new AsyncObject(4096);
                obj.WorkingSocket = mainSock;
                mainSock.BeginReceive(obj.Buffer, 0, obj.BufferSize, 0, DataReceived, obj);
            }
            catch (Exception e)
            {
            }
        }

        void DataReceived(IAsyncResult ar)
        {
            AsyncObject obj = (AsyncObject)ar.AsyncState;

            int received = obj.WorkingSocket.EndReceive(ar);

            byte[] buffer = new byte[received];

            Array.Copy(obj.Buffer, 0, buffer, 0, received);

            string receivedData = Encoding.UTF8.GetString(buffer); // 바이트 배열을 문자열로 변환
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string str = "wqeqwejks";
            var data = Encoding.UTF8.GetBytes(str.ToString());
            mainSock.Send(data);
        }
    }
}
