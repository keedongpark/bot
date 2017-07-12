using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace loady
{
    /// <summary>
    /// 간단한 네트워크 연결. Agent 단위로 사용.
    /// 바이트 송수신만 담당하고 Agent에서 프로토콜 처리.  
    /// </summary>
    public class Session
    {
        Agent agent;
        string host;
        ushort port;
        Socket socket;
        const int recvBufferSize = 1024;
        byte[] recvBuffer = new byte[recvBufferSize];

        MemoryStream recvStream = new MemoryStream();

        volatile bool sending = false;
        MemoryStream sendStream1 = new MemoryStream();
        MemoryStream sendStream2 = new MemoryStream();
        MemoryStream accumulStream;
        MemoryStream sendStream;

        public Session(Agent agent)
        {
            Contract.Assert(agent != null);

            this.agent = agent;
        }

        public bool Connect(string host, ushort port)
        {
            Contract.Assert(socket == null);

            this.host = host;
            this.port = port;

            IPHostEntry ipHostInfo = Dns.GetHostEntry(host);
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.BeginConnect(remoteEP, new AsyncCallback(OnConnectCompleted), socket);

            return true;
        }

        public bool IsConnected()
        {
            return socket != null && socket.Connected;
        }

        public void Disconnect()
        {
            if (IsConnected())
            {
                socket.Close();
            }
        }

        public bool Send(byte[] payload)
        {
            accumulStream.Write(payload, 0, payload.Length);

            RequestSend();

            return true;
        }

        void RequestRecv()
        {
            try
            {
                socket.BeginReceive(
                    recvBuffer, 
                    0, 
                    recvBufferSize, 
                    0, 
                    new AsyncCallback(OnRecvCompleted), socket
                    );
            }
            catch (Exception ex)
            {
                agent.fail($"session error {ex}");

                Disconnect();
            }
        } 

        void RequestSend()
        {
            if ( sending )
            {
                return;
            }

            if (accumulStream.Length == 0)
            {
                return;
            }

            SwitchSendStream();

            try
            {
                var buf = sendStream.GetBuffer();

                // Begin sending the data to the remote device.
                socket.BeginSend(
                    buf,
                    0,
                    (int)sendStream.Position,
                    0,
                    new AsyncCallback(OnSendCompleted),
                    socket);
            }
            catch ( Exception ex)
            {
                agent.fail($"session error {ex}");

                Disconnect();
            }
        }

        void OnConnectCompleted(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndConnect(ar);
            }
            catch (Exception ex)
            {
                agent.fail($"session error {ex}");

                Disconnect();
            }
        }

        void OnRecvCompleted(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    recvStream.Write(recvBuffer, 0, bytesRead);

                    agent.OnRecv(recvStream);
                }

                RequestRecv();
            }
            catch (Exception ex)
            {
                agent.fail($"session error {ex}");

                Disconnect();
            }
        }

        void OnSendCompleted(IAsyncResult ar)
        {
            sending = false;

            try
            {
                Socket client = (Socket)ar.AsyncState;
                int bytesSent = client.EndSend(ar);

                // 확인 : bytesSent는 항상 전체 패킷에 해당하는가?

                // 처음으로 돌림
                sendStream.Seek(0, SeekOrigin.Begin);

                RequestSend();
            }
            catch (Exception ex)
            {
                agent.fail($"session error {ex}");

                // TODO: exception 종류에 따라 체크해야 하는 지 확인 
                // x2clr 코드 확인

                Disconnect();
            }
        }

        void SwitchSendStream()
        {
            if ( Object.ReferenceEquals(accumulStream, sendStream1) )
            {
                accumulStream = sendStream2;
                sendStream = sendStream1;
            }
            else
            {
                accumulStream = sendStream1;
                sendStream = sendStream2;
            }
        }
    }
}
