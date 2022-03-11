using System;
using OnlineMazeGame;

class Home
{
    static void Main()
    {
        Console.WriteLine("Server (S) or Client (C)");
        if (Console.ReadLine().ToUpper() == "S")
            Server.Start();
        else
            Client.Start();
    }
}