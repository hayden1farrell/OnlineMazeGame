using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace SimpleNet
{
    /// <summary>
    /// This is the message class - it consists of a string of data and the ID of the client sending the data
    /// </summary>
    public class Message
    {
        public string clientID;
        public string Data;

        public Message(string CID, string contents)
        {
            clientID = CID;
            Data = contents;
        }
    }

    public class Server
    {
        //this records all currently connected clients
        public Dictionary<string, ServerClient> Clients = new Dictionary<string, ServerClient>();
        //message queue of all data transmitted by clients
        public ConcurrentQueue<Message> Messages = new ConcurrentQueue<Message>(); //thread-safe queue
        public int Port = 0;
        public string IP = "";

        private TcpListener server;

        /// <summary>
        /// This instantiates a server
        /// </summary>
        /// <param name="IPV4Address">The IP Version 4 address as a string e.g. "127.0.0.1"</param>
        /// <param name="port">The port number</param>
        /// <param name="started">a boolean valuw that will tell you whether the server has successfully started</param>
        public Server(string IPV4Address, int port, ref bool started)
        {
            try
            {
                IPAddress localAddr = IPAddress.Parse(IPV4Address);

                server = new TcpListener(localAddr, port);


                server.Start();

                // Start listening thread for client requests.
                Task listener = new Task(Listen);
                listener.Start();
                started = true;
                Port = port;
                IP = IPV4Address;
            }
            catch
            {
                started = false;
            }
        }

        /// <summary>
        /// this is the listen thread that will spot new clients
        /// if a new client is joining it will instantiate a new ServerClient object to listen out for that client on a separate thread
        /// 
        /// --------------- --------------     ---------
        /// |             -|serverClient||----| Client |
        /// |                            |    ----------
        /// |  server     -|serverClient||----| Client |
        /// |                            |    ----------
        /// |             -|serverClient||----| Client |
        /// ---------------              |    ----------
        /// 
        /// </summary>
        public async void Listen()
        {
            int clientID = 0;

            while (true)
            {
                //new client is requesting to join
                TcpClient clientSocket = server.AcceptTcpClient();
                if (clientSocket != null)
                {
                    clientID++;
                    Console.WriteLine("Connection request - allocated ID Number :" + clientID);
                    ServerClient Client = new ServerClient();
                    // this starts a dedicated listener thread for this client
                    Client.startClient(clientSocket, clientID.ToString(), this);

                    //respond to client issuing their ID number as a message e.g "ID=3#3$
                    NetworkStream ns = Client.clientSocket.GetStream();
                    Byte[] sendBytes = Encoding.ASCII.GetBytes("ID=" + clientID.ToString() + "#" + Client.clNo + "$");
                    ns.Write(sendBytes, 0, sendBytes.Length);
                    ns.Flush();
                    //add new client to dictionary of clients connected id is client ID
                    Clients.Add(Client.clNo, Client);
                    BroadcastClientConnectionStatus(Client);

                }
            }

        }
        /// <summary>
        /// This will send a message to all connected clients
        /// messages will be transmitted in the form
        /// e.g. "HELLO#3$"
        /// HELLO = contents
        /// # seperater
        /// 3 = ID of client who sent the message to be broadcast
        /// $ indicates end of message
        /// </summary>
        /// <param name="M"></param>
        public void broadcastToClients(Message M)
        {
            //Broadcast message to clients
            //
            foreach (KeyValuePair<string, ServerClient> C in Clients)
            {
                NetworkStream networkStream = C.Value.clientSocket.GetStream();
                Byte[] sendBytes = Encoding.ASCII.GetBytes(M.Data + "#" + M.clientID.ToString() + "$");
                networkStream.Write(sendBytes, 0, sendBytes.Length);
                networkStream.Flush();

            }
        }
        /// <summary>
        /// when a new client joins their ID is sent to all other clients so they are aware of their presence
        /// if a client leaves the other clients will be made aware of this too.
        /// </summary>
        /// <param name="NewC"></param>
        public void BroadcastClientConnectionStatus(ServerClient ThisClient, bool joining = true)
        {
            //Broadcast message to clients
            //message will be in the form "NEW CLIENT JOINED#3$" where 3 is the ID of the new client
            foreach (KeyValuePair<string, ServerClient> C in Clients)
            {

                NetworkStream networkStream = C.Value.clientSocket.GetStream();

                Byte[] sendBytes = Encoding.ASCII.GetBytes("NEW CLIENT JOINED#" + ThisClient.clNo.ToString() + "$");

                if (!joining)
                {
                    sendBytes = Encoding.ASCII.GetBytes("CLIENT LEFT#" + ThisClient.clNo.ToString() + "$");
                }
                networkStream.Write(sendBytes, 0, sendBytes.Length);
                networkStream.Flush();
            }
        }
        /// <summary>
        /// will broadcast a comma separated list of all client IDs currently connected to server
        /// </summary>
        public void BroadcastConnectedClients()
        {
            StringBuilder clients = new StringBuilder();
            //build comma separated list
            foreach (KeyValuePair<string, ServerClient> C in Clients)
            {

                clients.Append(C.Key);
                clients.Append(',');
            }
            //transmit to each connected client
            foreach (KeyValuePair<string, ServerClient> C in Clients)
            {

                NetworkStream networkStream = C.Value.clientSocket.GetStream();


                Byte[] sendBytes = Encoding.ASCII.GetBytes("CLIENT LIST#" + clients.ToString() + "$");


                networkStream.Write(sendBytes, 0, sendBytes.Length);
                networkStream.Flush();
            }
        }




        /// <summary>
        /// This will remove a client from the server
        /// </summary>
        /// <param name="C"></param>
        public void dropClient(ServerClient C)
        {
            Clients.Remove(C.clNo);
            BroadcastClientConnectionStatus(C, false);
            Console.WriteLine("Connection lost Client :" + C.clNo);
        }

    }



    /// <summary>
    /// Class to handle each client interaction separately on the server
    /// will run its own Listen thread to manage messages from clients that will be added to the server message queue
    /// </summary>

    public class ServerClient
    {
        public TcpClient clientSocket;
        public string clNo;
        Server S;

        public void startClient(TcpClient inClientSocket, string clineNo, Server s)
        {
            S = s;
            clientSocket = inClientSocket;
            clNo = clineNo;
            Thread ctThread = new Thread(listen);
            ctThread.Start();
        }

        private void listen()
        {
            byte[] bytesFrom = new byte[65536];
            string dataFromClient = null;

            while (true)
            {
                try
                {
                    NetworkStream networkStream = clientSocket.GetStream();
                    networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                    dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                    dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$") + 1);
                    string IDofClient = dataFromClient.Substring(dataFromClient.IndexOf("#") + 1, dataFromClient.IndexOf("$") - (dataFromClient.IndexOf("#") + 1));
                    Message M = new Message(IDofClient, dataFromClient.Substring(0, dataFromClient.IndexOf("#")));
                    S.Messages.Enqueue(M);
                    networkStream.Flush();
                }
                catch { break; }


            }
            clientSocket.Close();
            //client has dropped connection so remove from client list
            S.dropClient(this);


        }
    }

    /// <summary>
    /// The client class
    /// This will open a connection to the server on a separate machine
    /// it will then run a listening thread for any data from the servewr
    /// You will need to know the server IP and PORT NUMBER
    /// </summary>
    public class Client
    {
        public TcpClient clientSocket;
        public NetworkStream serverStream;
        public ConcurrentQueue<Message> Messages = new ConcurrentQueue<Message>(); //thread-safe queue
        public string CLID;

        /// <summary>
        /// This instantiates a new client
        /// It will handshake with the server and receive its client ID number from the server
        /// </summary>
        public Client(string serverIP, int serverPort)
        {
            clientSocket = new TcpClient(serverIP, serverPort);
            clientSocket.Connect(serverIP, serverPort);
            //receive ID
            Byte[] bytesFrom = new byte[65536];
            string DataFromServer = string.Empty;
            NetworkStream networkStream = clientSocket.GetStream();
            //it will need to receive a message beginning "ID" from the server (this is the message telling it its client ID number)
            while (!DataFromServer.StartsWith("ID"))
            {
                networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                networkStream.Flush();
                DataFromServer = System.Text.Encoding.ASCII.GetString(bytesFrom);
            }
            CLID = DataFromServer.Substring(DataFromServer.IndexOf("#") + 1, DataFromServer.IndexOf("$") - (DataFromServer.IndexOf("#") + 1));
            networkStream.Flush();
            //start listener thread
            Thread ctThread = new Thread(Listen);
            ctThread.Start();
        }

        /// <summary>
        /// This will send a string message to the server which will then broadcast to all other connected clients
        /// </summary>
        /// <param name="Message"></param>
        /// <returns></returns>
        public bool Send(string Message)
        {
            try
            {
                serverStream = clientSocket.GetStream();
                byte[] outStream = System.Text.Encoding.ASCII.GetBytes(Message + "#" + CLID + "$");
                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Flush();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// This is the listen thread that will pick up any messages from the server
        /// </summary>
        private void Listen()
        {
            while (true)
            {
                serverStream = clientSocket.GetStream();
                int buffSize = 0;
                byte[] inStream = new byte[65536];
                buffSize = clientSocket.ReceiveBufferSize;
                serverStream.Read(inStream, 0, buffSize);
                string dataFromServer = System.Text.Encoding.ASCII.GetString(inStream);
                dataFromServer = dataFromServer.Substring(0, dataFromServer.IndexOf("$") + 1);
                string IDofClient = dataFromServer.Substring(dataFromServer.IndexOf("#") + 1, dataFromServer.IndexOf("$") - (dataFromServer.IndexOf("#") + 1));
                Message M = new Message(IDofClient, dataFromServer.Substring(0, dataFromServer.IndexOf("#")));
                Messages.Enqueue(M);
            }
        }

    }
}
