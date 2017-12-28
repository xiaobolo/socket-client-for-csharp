using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SocketClientForCShape.network
{
    class TCPClient
    {
        public Socket socket;
        private IPEndPoint endPoint;
        private SocketAsyncEventArgs connectSAEA;
        private SocketAsyncEventArgs disconnectSAEA;
        private SocketAsyncEventArgs sendSAEA;
        private Thread sendThread;

        //事件注册
        public delegate void Handler0();
        public delegate void Handler1(byte[] bytes);
        public event Handler0 OnConnectSucceed; 
        public event Handler0 OnConnectFailed;
        public event Handler0 OnSended;
        public event Handler1 OnReceived;
        public event Handler0 OnDisconnected;


        public byte[] data { get; set; }

        public TCPClient(byte[] data)
        {
            this.data = data;
        }

        /// <summary>
        /// 开始发送数据
        /// </summary>
        public void StartSend()
        {
            while (socket != null && socket.Connected)
            {
                Send(data);
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// 连接到服务端
        /// </summary>
        /// <param name="ip">主机地址</param>
        /// <param name="port">端口号</param>
        public void Connent(string ip, int port)
        {
            if (socket==null || !socket.Connected)
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress iPAddress = IPAddress.Parse(ip);
                endPoint = new IPEndPoint(iPAddress, port);
                connectSAEA = new SocketAsyncEventArgs { RemoteEndPoint = endPoint };
                sendThread = new Thread(new ThreadStart(StartSend));
                connectSAEA.Completed += OnConnectCompleted;
                socket.ConnectAsync(connectSAEA);
            }
        }

        /// <summary>
        /// 连接完成时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnConnectCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                OnConnectFailed?.Invoke();
                return;
            }
            Socket socket = sender as Socket;
            string iPRemote = socket.RemoteEndPoint.ToString();

            OnConnectSucceed?.Invoke();
            sendThread.Start();

            SocketAsyncEventArgs receiveSAEA = new SocketAsyncEventArgs();
            byte[] receiveBuffer = new byte[1024 * 4];
            receiveSAEA.SetBuffer(receiveBuffer, 0, receiveBuffer.Length);
            receiveSAEA.Completed += OnReceiveCompleted;
            receiveSAEA.RemoteEndPoint = endPoint;
            socket.ReceiveAsync(receiveSAEA);
        }

        /// <summary>
        /// 接收完成时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.OperationAborted) return;

            Socket socket = sender as Socket;

            if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
            {
                string ipAdress = socket.RemoteEndPoint.ToString();
                int lengthBuffer = e.BytesTransferred;
                byte[] receiveBuffer = e.Buffer;
                byte[] buffer = new byte[lengthBuffer];
                Buffer.BlockCopy(receiveBuffer, 0, buffer, 0, lengthBuffer);

                OnReceived?.Invoke(buffer);
                socket.ReceiveAsync(e);
            }
            else if (e.SocketError == SocketError.ConnectionReset && e.BytesTransferred == 0)
            {
                OnDisconnected?.Invoke();
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="bytes">byte数组</param>
        public void Send(byte[] bytes)
        {
            if (sendSAEA == null)
            {
                sendSAEA = new SocketAsyncEventArgs();
                sendSAEA.Completed += OnSendCompleted;
            }

            sendSAEA.SetBuffer(bytes, 0, bytes.Length);
            if (socket != null)
            {
                socket.SendAsync(sendSAEA);
            }
        }

        /// <summary>
        /// 发送完成时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success) return;
            Socket socket = sender as Socket;
            byte[] sendBuffer = e.Buffer;

            OnSended?.Invoke();
        }

        /// <summary>
        /// 与服务器断开连接
        /// </summary>
        public void DisConnect()
        {
            if (socket != null && socket.Connected)
            {
                try
                {
                    disconnectSAEA = new SocketAsyncEventArgs();
                    disconnectSAEA.Completed += OnDisconnectCompleted;
                    socket.DisconnectAsync(disconnectSAEA);
                }
                catch (SocketException se)
                {
                }
                finally
                {
                    socket.Close();
                    socket = null;
                }
            }
        }

        /// <summary>
        /// 断开完成时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnDisconnectCompleted(object sender, SocketAsyncEventArgs e)
        {
            OnDisconnected?.Invoke();
        }
    }
}