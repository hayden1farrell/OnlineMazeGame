﻿namespace OnlineMazeGame;
using System.Net;

public class Server
{
    struct GameInfo
    {
        public int PlayerCount = 0;
        public bool open = false;
        public string password = "";
        
    }

    private Dictionary<int, GameInfo> games = new Dictionary<int, GameInfo>();
    public static void Start()
    {
        Console.WriteLine("Server Start");
        SimpleNet.Server server = SiliconValley();
        Console.WriteLine($"Server has began IP: {server.IP} ,Port: {server.Port}");
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