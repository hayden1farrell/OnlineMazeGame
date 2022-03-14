using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace OnlineMazeGame;
using SimpleNet;

public class Client
{
    public static string data = "";
    public static string oppenent = "";
    public static char[,] displayMaze;
    public static int X = 0;
    public static int Y = 0;
    public static int enemyX = 0;
    public static int enemyY = 0;
    public static bool player1 = false;
    public static int size = 0;
    public static SimpleNet.Client? client = null;
    public static void Start()
    {
        Console.OutputEncoding = Encoding.Unicode;  // crucial
        Console.WriteLine("Client Start");
        string username = GetUsername();
        client = ConnectToServer();
        
        string message = "";
        bool playing = false;
        bool displaying = false;
        bool gameBegan = false;
        while (true)
        {
            if (playing == false && displaying == false && gameBegan == false)
            {
                Console.WriteLine("Do you wish to see all games Y/N");
                if (Console.ReadLine().ToLower() == "y")
                {
                    displaying = true;
                    client.Send("+Request Game Info");
                }
                else
                {
                    Console.WriteLine("Do you want to (C)reate or (J)oin a game");
                    if (Console.ReadLine().ToUpper() == "C")
                        CreateNewGame(client);
                    else
                        JoinGame(client, username);
                }
            }
            
            Message NM;
            client.Messages.TryDequeue(out NM);
            if (NM != null) // seeing if the server has sent a message
            {
                data = NM.Data.Substring(1, NM.Data.Length - 1);
                switch (NM.Data[0])
                {
                    case ']':
                        DisplayGameMenu(data);
                        displaying = false;
                        break;
                    case '>':
                        if (gameBegan)
                        {
                            GameJoined(data, username);
                        }
                        playing = true;
                        break;
                    case '!':
                        oppenent = data;
                        Thread GameHandler = new Thread(new ThreadStart(GameStarted));
                        GameHandler.Start();
                        gameBegan = true;
                        break;
                    case '^':
                        ParseMaze(data);
                        break;
                    case ':':
                        SetSpawn(data);
                        break;
                    case '(':
                        UpdateLocation(data);
                        break;
                    default:
                        break;
                }
            }

        }
    }

    private static void UpdateLocation(string data)
    {
        string[] split = data.Split(",");
        if (split[2] == client.CLID)
        {
            X = Int32.Parse(split[0]);
            Y = Int32.Parse(split[1]);
        }
        else
        {
            enemyX = Int32.Parse(split[0]);
            enemyY = Int32.Parse(split[1]);
        }
            
    }

    private static void SetSpawn(string s)
    {
        string[] data = s.Split(",");
        X = Int32.Parse(data[0]);
        Y = Int32.Parse(data[1]);

        if (X + Y < 10)
            player1 = true;
    }

    private static void GameStarted()
    {
        Console.WriteLine("Game has started"); 
        Console.WriteLine(oppenent);
        bool mazeGotten = false;

        while (true)
        {
            GetMovment(ref X, ref Y);
            client.Send($"?,{X},{Y}");
            int player1X = 0;
            int player1Y = 0;
            int player2X = 0;
            int player2Y = 0;
            if (player1)
            {
                player1X = X;
                player1Y = Y;
                player2X = enemyX;
                player2Y = enemyY;
            }
            else
            {
                player1X = enemyX;
                player1Y = enemyY;
                player2X = X;
                player2Y = Y;
            }

            ShowMaze(player1X,  player1Y,  player2X,  player2Y);
            
            System.Threading.Thread.Sleep(50);
        }
    }
    private static void GetMovment(ref int x, ref int y)
    {
        if (Console.KeyAvailable)
        {
            ConsoleKeyInfo cki = Console.ReadKey();
            switch (cki.Key)
            {
                case ConsoleKey.UpArrow:
                    x -= 1;
                    break;

                case ConsoleKey.DownArrow:
                    x += 1;
                    break;

                case ConsoleKey.LeftArrow:
                    y -= 1;
                    break;

                case ConsoleKey.RightArrow:
                    y += 1;
                    break;
            }
        }
    }

    private static void ShowMaze(int player1X, int player1Y, int player2X, int player2Y)
    {
        Console.Clear();
        for (int i = 0; i < size - 1; i++)
        {
            for (int j = 0; j < size - 1; j++)
            {
                if (i == player1X + 1 && j == player1Y + 1)
                    Console.BackgroundColor = ConsoleColor.Blue;
                else if (i == player2X + 1 && j == player2Y + 1)
                    Console.BackgroundColor = ConsoleColor.Red;
                else if (displayMaze[i, j] == '1')
                    Console.BackgroundColor = ConsoleColor.Black;
                else
                    Console.BackgroundColor = ConsoleColor.Gray;
                Console.Write("  ");
            }

            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine("");
        }
    }

    private static void ParseMaze(string maze)
    {
        string[] splitMaze = maze.Split(",");
        char[,] temp = new char[splitMaze.Length,splitMaze.Length];
        size = splitMaze.Length;
        for (int i = 0; i < splitMaze.Length; i++)
        {
            string currentLine = splitMaze[i];
            for (int j = 0; j < currentLine.Length; j++)
            {
                temp[i, j] = currentLine[j];
            }
        }

        displayMaze = temp;
    }

    private static void GameJoined(string data, string username)
    {
        Console.WriteLine("You have joined the game: " + username);
        Console.WriteLine("When a second player joins the game will start");
    }

    private static void CreateNewGame(SimpleNet.Client client)
    {
        client.Send("*Create Game");
    }

    private static void JoinGame(SimpleNet.Client client, string username)
    {
        Console.WriteLine("Enter game ID");
        int id = Int32.Parse(Console.ReadLine());
        string JoinMessage = $"-,{id},{client.CLID},{username}";
        client.Send(JoinMessage);
    }

    private static void DisplayGameMenu(string data)
    {
        string[] games = data.Split("/]");

        foreach (var game in games)
        {
            string[] gameData = game.Split(",");
            Console.WriteLine($"Game ID: {gameData[0]}, Player Count: {gameData[1].Replace("/","")}");
        }
        Console.WriteLine(" ");
    }
    

    static SimpleNet.Client ConnectToServer()
    {
        (string ip, int port) = GetServerDetails();
        try
        {
            SimpleNet.Client MyClient = new SimpleNet.Client(ip, port);
            return MyClient;
        }
        catch(Exception e)
        {
            Console.WriteLine("Server did not allow connection check port, please retry");
            Console.WriteLine(e);
            ConnectToServer();
        }

        return null;
    }

    private static (string, int) GetServerDetails()
    {
        //Console.Write("Enter server IP: ");
        string IP = "::1";
        int port = 3333;
        return (IP, port);
    }

    private static string GetUsername()
    {
        Console.Write("Enter username: ");
        return Console.ReadLine();
    }
}