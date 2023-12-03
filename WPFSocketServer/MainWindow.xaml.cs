using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace WPFSocketServer
{
    public partial class MainWindow : Window
    {
        public Socket mainSock;
        public List<Socket> connectedClients = new List<Socket>();
        int m_port = 5000;
        public MainWindow()
        {
            InitializeComponent();

            Start();
        }

        public void Start()
        {
            try
            {
                //소켓을 생성한다
                mainSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //목적지를 정해준다
                IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, m_port);
                //목적지와 묶어준다
                mainSock.Bind(serverEP);
                //받아들이기 시작한다 (최대 10개의 Client까지)
                mainSock.Listen(10);
                //연결시도가 감지되면 AcceptCallBack으로 이동하게 설정
                mainSock.BeginAccept(AcceptCallBack, null);
            }
            catch (Exception e)
            {
            }
        }

        void AcceptCallBack(IAsyncResult ar)
        {
            try
            {
                Socket client = mainSock.EndAccept(ar);
                AsyncObject obj = new AsyncObject(1920 * 1080 * 3);
                obj.WorkingSocket = client;
                connectedClients.Add(client);
                client.BeginReceive(obj.Buffer, 0, 1920 * 1080 * 3, 0, DataReceived, obj);

                mainSock.BeginAccept(AcceptCallBack, null);
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

            SendData(obj.WorkingSocket);
        }

        private void SendData(Socket workingSocket)
        {
            string str = "Hello World";
            var data = Encoding.UTF8.GetBytes(str.ToString());

            try
            {
                // 데이터를 클라이언트에게 보냅니다.
                int bytesSent = workingSocket.Send(data);

                // 데이터 전송 성공 여부 확인
                if (bytesSent == data.Length)
                {
                    Debug.WriteLine("데이터를 클라이언트에게 성공적으로 보냈습니다.");
                }
                else
                {
                    Debug.WriteLine("데이터 전송이 부분적으로 완료되었습니다.");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"데이터 전송 중 오류 발생: {e.Message}");
            }
        }

        public void Close()
        {
            //메인소켓 해제
            if (mainSock != null)
            {
                mainSock.Close();
                mainSock.Dispose();
            }

            //서버->어딘가를 향하는 소켓은 여러개가 있고 목적지가 모두 다릅니다.
            //이 소켓들은 connectedClients에 저장되어 있으므로 모두 해제해줍니다.
            foreach (Socket socket in connectedClients)
            {
                socket.Close();
                socket.Dispose();
            }
            connectedClients.Clear();
        }
    }
}
