using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Nodes;
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

            if (receivedData.StartsWith("GET"))
            {
                // 경로 추출
                string[] requestLines = receivedData.Split('\r', '\n');
                string[] requestParts = requestLines[0].Split(' ');
                if (requestParts.Length >= 2)
                {
                    string path = requestParts[1]; // "/path/to/resource"

                    // get으로 왔을시 응답
                    HandleGetRequestToRespons(obj.WorkingSocket, path);
                }
            }
            else if (receivedData.StartsWith("POST"))
            {
                int bodyStartIndex = receivedData.IndexOf("\r\n\r\n") + 4;
                string jsonBody = receivedData.Substring(bodyStartIndex);

                // post로 왔을시 응답
                HandlePostRequestToRespons(obj.WorkingSocket, jsonBody);
            }
            else
            {
                Debug.WriteLine("Unsupported request type");
            }
        }

        private void HandleGetRequestToRespons(Socket workingSocket, string requestPath)
        {
            string strSendData = "";
            System.Globalization.CultureInfo culInfo = new System.Globalization.CultureInfo("en-US");

            // http프로토콜로 보내기
            StringBuilder sb = new StringBuilder(100);
            sb.AppendLine("HTTP/1.1 200 ok");
            sb.AppendLine("date: " + DateTime.UtcNow.ToString("ddd, dd MMM yyy HH':'mm':'ss 'GMT'", culInfo));
            sb.AppendLine("server: test server");
            sb.AppendLine("content-type:text/html; charset=UTF-8");

            if (requestPath == "/some/resource")
            {
                strSendData = "요야유";
            }
            else if (requestPath == "/another/resource")
            {

            }
            else
            {
                strSendData = "요야유";
            }

            sb.AppendLine("Content-Length: " + (Encoding.UTF8.GetBytes(strSendData)).Length);
            sb.AppendLine();
            sb.AppendLine(strSendData);
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());

            try
            {
                // 데이터를 클라이언트에게 보냅니다.
                int bytesSent = workingSocket.Send(bytes);
                //Thread.Sleep(10);

                // 데이터 전송 성공 여부 확인
                if (bytesSent == bytes.Length)
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

        private void HandlePostRequestToRespons(Socket workingSocket, string jsonBody)
        {
            string strSendData = "";
            System.Globalization.CultureInfo culInfo = new System.Globalization.CultureInfo("en-US");

            // http프로토콜로 보내기
            StringBuilder sb = new StringBuilder(100);
            sb.AppendLine("HTTP/1.1 200 ok");
            sb.AppendLine("date: " + DateTime.UtcNow.ToString("ddd, dd MMM yyy HH':'mm':'ss 'GMT'", culInfo));
            sb.AppendLine("server: test server");
            sb.AppendLine("content-type:text/html; charset=UTF-8");

            JArray jsonArray = null;

            // JSON 파싱
            try
            {
                jsonArray = JArray.Parse(jsonBody);
                // JSON을 객체로 파싱하는 예시 (Newtonsoft.Json 라이브러리 사용)
                //YourJsonObject jsonObject = JsonConvert.DeserializeObject<YourJsonObject>(jsonBody);
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"JSON 파싱 오류: {ex.Message}");
                // JSON 파싱 오류 처리
            }

            JObject jsonObject1 = jsonArray[0].ToObject<JObject>();
            int id1 = jsonObject1["id"].ToObject<int>();
            string name1 = jsonObject1["name"].ToObject<string>();
            string email1 = jsonObject1["email"].ToObject<string>();
            strSendData = "POST통신";

            sb.AppendLine("Content-Length: " + (Encoding.UTF8.GetBytes(strSendData)).Length);
            sb.AppendLine();
            sb.AppendLine(strSendData);
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());

            try
            {
                // 데이터를 클라이언트에게 보냅니다.
                int bytesSent = workingSocket.Send(bytes);
                //Thread.Sleep(10);

                // 데이터 전송 성공 여부 확인
                if (bytesSent == bytes.Length)
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
