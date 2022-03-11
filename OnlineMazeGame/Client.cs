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
        while (true)
        {
            if (Console.KeyAvailable)
            {
                message = Console.ReadLine();
                client.Send(message);
            }
            Message NM;
            client.Messages.TryDequeue(out NM);
            if (NM != null)
                Console.WriteLine("Message= " + NM.Data + " from " + NM.clientID);

        }
    }

    static SimpleNet.Client ConnectToServer()
    {
        (string ip, int port) = GetServerDetails();
        try
        {
            SimpleNet.Client MyClient = new SimpleNet.Client(ip, port);
            Console.WriteLine("My ID = " + MyClient.CLID);
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