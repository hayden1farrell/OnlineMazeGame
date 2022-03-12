using SimpleNet;

namespace OnlineMazeGame;
using System.Net;

public class Server
{
    public struct GameInfo
    {
        public int PlayerCount;
        public bool open;
        public string[,] playerData;
    }
    
    public static Dictionary<int, GameInfo> games = new Dictionary<int, GameInfo>();
    public static void Start()
    {
        GameInfo test = new GameInfo();
        test.PlayerCount = 0;
        test.open = true;
        test.playerData = null;
        games.Add(0, test);

        Console.WriteLine("Server Start");
        SimpleNet.Server server = SiliconValley();
        Console.WriteLine($"Server has began IP");

        bool SeeMessages = true;
        while (true)
        {
            SimpleNet.Message NM;
            server.Messages.TryDequeue(out NM);
            if (NM != null)
            {
                switch (NM.Data[0])
                {
                    case '+':
                        SendGames(server, NM);
                        break;
                    case '*':
                        CreateGame(server, NM);
                        break;
                    case '-':
                        JoinGame(server, NM);
                        break;
                }
            }
        }
    }

    private static void JoinGame(SimpleNet.Server server, Message nm)
    {
        string[] reqData = nm.Data.Split(",");
        int gameID = Int32.Parse(reqData[2].ToString()) - 1;
        Console.Write(gameID);
        string clientID = reqData[1];

        if (games[gameID].open == true)
        {
            GameInfo info = games[gameID];
            info.PlayerCount += 1;
            string[,] playerData = new string[2,2];
            if (info.PlayerCount == 2)
            {
                info.open = false;
                playerData[0, 0] = info.playerData[0, 0];
                playerData[0, 1] = info.playerData[0, 1];
            }
            playerData[info.PlayerCount - 1, 0] = reqData[3];
            playerData[info.PlayerCount - 1, 1] = nm.clientID;

            games[gameID] = info;
            server.SendToClient(">Joined game successfully", nm.clientID);
        }
        else
            server.SendToClient("-Game is full", nm.clientID);
    }

    private static void CreateGame(SimpleNet.Server server, Message nm)
    {
        int gameID = games.Count;
        GameInfo gameinfo = new GameInfo();
        gameinfo.PlayerCount = 0;
        gameinfo.open = true;
        gameinfo.playerData = null;
        games.Add(gameID, gameinfo);
    }

    private static void SendGames(SimpleNet.Server server, SimpleNet.Message NM)
    {
        string message = "";
        if (games.Count == 0)
            message = "]No games have been created";
        else
        {
            foreach (var game in games)
            {
                message += "]" + game.Key.ToString() + "," + game.Value.PlayerCount + "/";
            }
        }
        
        server.SendToClient(message, NM.clientID);
    }

    private static SimpleNet.Server? SiliconValley()
    {
        bool success = false;
        string machine = Dns.GetHostName();
        IPAddress[] localIPs = Dns.GetHostAddresses(machine);
        
        DisplayIPS(localIPs);
        
        Console.Write("Select number: ");
        string ip = localIPs[Convert.ToInt32(Console.ReadLine()) - 1].ToString();
        int port = 3333;
        
        SimpleNet.Server myServer = new SimpleNet.Server(ip, port, ref success);

        if (success == true)
            return myServer;
        else
        {
            Console.WriteLine("Failed to connect - Retry");
            SiliconValley();
        }

        return null;
    }

    static void DisplayIPS(IPAddress[] localIPs)
    {
        Console.WriteLine("IP addresses available on this machine...  ::1 means localhost");
        int IPCount = 1;
        foreach (IPAddress item in localIPs)
        {
            Console.WriteLine($"{IPCount}- {item}");
            IPCount++;
        }
    }
}