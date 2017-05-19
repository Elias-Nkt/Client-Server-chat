namespace SocketServer
{
    class Program
    {     
        static void Main(string[] args)
        {
            Server serv = new Server();
            serv.StartServer();
        }
    }
}