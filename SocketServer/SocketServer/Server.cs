using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SocketServer
{
    class Server
    {
        Socket sListener;

        bool isServerRunning;

        public Hashtable clients = new Hashtable();

        int port = 11000;

        IPEndPoint Point;

        public void StopServer()
        {
            isServerRunning = false;
            sListener.Close();
        }

        public void StartServer()
        {
            Thread th = new Thread(delegate ()
            {
                isServerRunning = true;

                IPHostEntry ipHost = Dns.GetHostEntry("localhost");
                IPAddress ipAddr = ipHost.AddressList[0];
                Point = new IPEndPoint(ipAddr, port);

                sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                sListener.Bind(Point);
                sListener.Listen(10);

                Console.WriteLine("Ожидаем соединение через порт {0}", port);

                SocketAccepter();
                });
                th.Start();
        }

        public void SocketAccepter()
        {
            while (isServerRunning)
            {
                Socket client = sListener.Accept();

                clients.Add(client, "");

                Console.WriteLine("Клиент добавлен");

                Thread thh = new Thread(delegate ()
                {
                    MessageReceiver(client);
                });
                thh.Start();
            }
        }

        public void MessageReceiver(Socket r_client)
        {

            string name = "0";

            while (isServerRunning)
            {
                try
                {
                    Console.WriteLine("Обрабатываем запрос\n");

                    byte[] bytes = new byte[1024];

                    int bytesRec = r_client.Receive(bytes);

                    string data = null;
                    if (name == "0")
                    {
                        name = Encoding.UTF8.GetString(bytes, 0, bytesRec);

                        byte[] msg = Encoding.UTF8.GetBytes("Your Nickname: " + name);

                        MessageSender(r_client, msg);
                    }
                    else
                    {
                        data += name + ": " + Encoding.UTF8.GetString(bytes, 0, bytesRec);

                        byte[] msg = Encoding.UTF8.GetBytes(data);
                        Console.WriteLine ("Client Message: " + data);
                        //в личку написать т.е !Loloshka даров брат
                        //все что после ! знака - ник
                        // если нет чела в сети то пиши ошибку, лолошка оффлайн
                        // 
                        foreach (Socket s_client in clients.Keys)
                        {
                            MessageSender(s_client, msg);
                        }
                    }
                }
                catch (Exception ex)
                {
                    isServerRunning = false;
                    Console.WriteLine("Po4emy NeReSpEcT" + ex.ToString());
                }
            }
        }

        public void MessageSender(Socket s_client, byte[] bytes)
        {
            try
            {
                s_client.Send(bytes);
            }
            catch (Exception ex)
            {
                isServerRunning = false;
                Console.WriteLine("Po4emy NeReSpEcT" + ex.ToString());
            }
        }
    }
}
