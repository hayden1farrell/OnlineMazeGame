using System.ComponentModel.DataAnnotations;

namespace OnlineMazeGame;
using SimpleNet;

public class Client
{
    public static void Start()
    {
        Console.WriteLine("Client Start");
        string username = GetUsername();
        SimpleNet.Client client = ConnectToServer();
        
        string message = "";
        bool playing = false;
        bool displaying = false;
        while (true)
        {
            if (playing == false && displaying == false)
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
                string data = NM.Data.Substring(1, NM.Data.Length - 1);
                switch (NM.Data[0])
                {
                    case ']':
                        DisplayGameMenu(data);
                        displaying = false;
                        break;
                    case '>':
                        GameJoined(data, username);
                        playing = true;
                        break;
                    default:
                        break;
                }
            }

        }
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
        Console.Write("Enter server IP: ");
        string IP = Console.ReadLine();
        Console.Write("Enter server Port: ");
        int port = Convert.ToInt32(Console.ReadLine());
        return (IP, port);
    }

    private static string GetUsername()
    {
        Console.Write("Enter username: ");
        return Console.ReadLine();
    }
}