using System;
using System.Windows;
using SocketClientForCShape.network;

namespace SocketClientForCShape
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        TCPClient client;
        byte[] data;

        public MainWindow()
        {
            InitializeComponent();
            data = new byte[] { 0x01, 0x02, 0xdf, 10, 100, 254 };
            client = new TCPClient(data);
            client.OnConnectSucceed += OnConnectSucceed;
            client.OnConnectFailed += OnConnectFailed;
            client.OnSended += OnSended;
            client.OnReceived += OnReceived;
            client.OnDisconnected += OnDisconnected;
        }

        private void OnDisconnected()
        {
            Console.WriteLine("Is OnDisconnected.");
        }

        private void OnReceived(byte[] bytes)
        {
            Console.WriteLine("Is OnReceived. " + bytes);
        }

        private void OnSended()
        {
            Console.WriteLine("Is OnSended.");
        }

        private void OnConnectSucceed()
        {
            Console.WriteLine("Is OnConnectSucceed.");
        }

        private void OnConnectFailed()
        {
            Console.WriteLine("Is OnConnectFailed.");
        }

        private void btnOnClick(object sender, RoutedEventArgs e)
        {
            if (btn_connent == sender)
            {
                Console.WriteLine("Connent is clicked.");
                client.Connent("127.0.0.1", 12345);
            }
            else if (btn_disconnent == sender)
            {
                Console.WriteLine("Disconnent is clicked.");
                client.DisConnect();
            }
            else if (btn_send == sender)
            {
                Console.WriteLine("Send is clicked.");
                data = new byte[] { 0x01, 0x02, 0xdf, 10, 100, 254, 0x02, 0xdf, 10, 100, 254 };
                client.data = data;
            }
        }
    }
}
