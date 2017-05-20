using System;
using System.Collections;
using System.Collections.Generic;
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

        Dictionary<string, Socket> clients = new Dictionary<string, Socket>();

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

                        clients.Add(name, r_client); // Добавили текущий серв и нейм в хеш

                        byte[] msg = Encoding.UTF8.GetBytes("Your Nickname: " + name);

                        MessageSender(r_client, msg);
                    }
                    else
                    {
                        data += Encoding.UTF8.GetString(bytes, 0, bytesRec);

                        byte[] msg = Encoding.UTF8.GetBytes(name + ": " + data);
                        Console.WriteLine ("Client Message: " + data);
                        //в личку написать т.е !Loloshka даров брат
                        //все что после ! знака - ник
                        // если нет чела в сети то пиши ошибку, лолошка оффлайн
                        // в хештэйбл записываем ники. валуе = 0 если офлайн или нет = 1 если тут 
                        // test message - pm? may be separate in function..
                        if (data[0] == '!')
                        {

                            string[] pmLogin = data.Split(new char[] { '!', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            int i = 0;
                            
                            foreach (KeyValuePair<string, Socket> pair in clients)
                            {
                                if (pair.Key == pmLogin[0] && pair.Value.Connected)
                                {
                                    MessageSender(pair.Value, msg);
                                    i = 1;
                                }
                                else if (pair.Key == pmLogin[0] && (pair.Value.Connected == false))
                                {
                                    byte[] smsaboutdisconnect = Encoding.UTF8.GetBytes(pmLogin[0] + " is disconnect");
                                    MessageSender(r_client, smsaboutdisconnect);
                                    i = 1;
                                }
                                
                            }

                            if (i == 0)
                            {
                                byte[] smsaboutlogin = Encoding.UTF8.GetBytes(pmLogin[0] + " is not register");
                                MessageSender(r_client, smsaboutlogin);
                            }
                        }
                        else
                        {
                            foreach (Socket s_client in clients.Values)
                            {
                                MessageSender(s_client, msg);
                            }
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
