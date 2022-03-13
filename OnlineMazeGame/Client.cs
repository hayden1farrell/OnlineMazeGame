using System.ComponentModel.DataAnnotations;

namespace OnlineMazeGame;
using SimpleNet;

public class Client
{
    public static string data = "";
    public static string oppenent = "";
    public static void Start()
    {
        Console.WriteLine("Client Start");
        string username = GetUsername();
        SimpleNet.Client client = ConnectToServer();
        
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
                    default:
                        break;
                }
            }

        }
    }
    private static void GameStarted()
    {
        Console.WriteLine("Game has started"); 
        Console.WriteLine(oppenent);
        bool mazeGotten = false;

        while (true)
        {
            System.Threading.Thread.Sleep(1000/100);
        }
    }

    private static void ShowMaze(char[,] displayMaze)
    {
        for (int i = 0; i < displayMaze.GetLength(0) - 1; i++)
        {
            for (int j = 0; j < displayMaze.GetLength(1) - 1; j++)
            {
                if (i == 1 && j == 1)
                    Console.BackgroundColor = ConsoleColor.Blue;
                else if (i == displayMaze.GetLength(1) - 3 && j == displayMaze.GetLength(1) - 3)
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
        char[,] displayMaze = new char[splitMaze.Length,splitMaze.Length];
        for (int i = 0; i < splitMaze.Length; i++)
        {
            string currentLine = splitMaze[i];
            for (int j = 0; j < currentLine.Length; j++)
            {
                displayMaze[i, j] = currentLine[j];
            }
        }

        ShowMaze(displayMaze);
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