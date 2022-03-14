using SimpleNet;

namespace OnlineMazeGame;
using System.Net;

public class Server
{
    public struct GameInfo
    {
        public int PlayerCount;
        public bool open;
        public string player1Name;
        public string player2Name;
        public string player1ID;
        public string player2ID;
        public Game gameHandler;
    }
    
    public static Dictionary<int, GameInfo> games = new Dictionary<int, GameInfo>();
    public static void Start()
    {
        Console.Clear();
        Console.WriteLine("Server Start");
        SimpleNet.Server? server = SiliconValley();
        Console.WriteLine($"Server has began");

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
                    case '?':
                        UpdateLocation(server, NM);
                        break;
                }
            }
        }
    }

    private static void UpdateLocation(SimpleNet.Server server, Message nm)
    {
        int gameCode = GetGame(nm);
        string[] data = nm.Data.Split(",");
        int x = Int32.Parse(data[1]);
        int y = Int32.Parse(data[2]);
        HandleInput(nm, x, y, games[gameCode].gameHandler);
        Console.WriteLine(x);
        
        server.SendToClient($"({x},{y},{nm.clientID}", games[gameCode].player1ID );
        server.SendToClient($"({x},{y},{nm.clientID}",  games[gameCode].player2ID);
    }

    private static void HandleInput(Message nm, int x, int t, Game gameHandler)
    {
        Console.WriteLine("HANDLE EXCEPTIONS");
    }

    private static int GetGame(Message nm)
    {
        foreach (var game in games)
        {
            if (game.Value.player1ID == nm.clientID || game.Value.player2ID == nm.clientID)
                return game.Key;
        }

        return 0;
    }

    private static void JoinGame(SimpleNet.Server server, Message nm)
    {
        string[] reqData = nm.Data.Split(",");
        int gameID = Int32.Parse(reqData[1].ToString());
        bool startGame = false;
        if (games[gameID].open == true)
        {
            server.SendToClient(">Joined game successfully", nm.clientID);
            GameInfo info = games[gameID];
            info.PlayerCount += 1;
            if (info.PlayerCount == 2)
            {
                info.open = false;
                info.player2Name = reqData[3];
                info.player2ID = nm.clientID;
                startGame = true;
            }
            else
            {
                info.player1Name = reqData[3];
                info.player1ID = nm.clientID;
            }
            games[gameID] = info;
            if(startGame == true)  SetUpGame(games[gameID], server);
        }
        else
            server.SendToClient("-Game is full", nm.clientID);
    }

    private static void SetUpGame(GameInfo info, SimpleNet.Server server)
    {
        server.SendToClient($"!A challenger has been found, there name is: {info.player2Name}", info.player1ID);
        server.SendToClient($"!You have challenged: {info.player1Name}", info.player2ID);
        
        info.gameHandler.NewGame(info, server);
        SendSpawn(server, info, info.gameHandler);
        SendMazeToClients(info.gameHandler, server, info);
    }

    private static void SendSpawn(SimpleNet.Server server, GameInfo info, Game gameHandler)
    {
        server.SendToClient(":1,1", info.player1ID);
        server.SendToClient($":{gameHandler.maze.GetLength(0)-3},{gameHandler.maze.GetLength(1)-3}", info.player2ID);
    }

    private static void SendMazeToClients(Game gameHandler, SimpleNet.Server server, GameInfo info)
    {
        string mazeString = "";
        for (int i = 0; i < gameHandler.maze.GetLength(0); i++)
        {
            for (int j = 0; j < gameHandler.maze.GetLength(1); j++)
            {
                mazeString += gameHandler.maze[i, j];
            }

            mazeString += ",";
        }

        string sendMsg = $"^{mazeString}";
        server.SendToClient(sendMsg, info.player1ID);
        server.SendToClient(sendMsg, info.player2ID);
    }

    private static void CreateGame(SimpleNet.Server server, Message nm)
    {
        int gameID = games.Count;
        GameInfo gameinfo = new GameInfo();
        Game GameHandler = new Game();
        gameinfo.PlayerCount = 0;
        gameinfo.open = true;
        gameinfo.player1Name = "";
        gameinfo.player2Name = "";
        gameinfo.player1ID = "";
        gameinfo.player2ID = "";
        gameinfo.gameHandler = GameHandler;
        
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