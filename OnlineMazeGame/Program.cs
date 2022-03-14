using System;
using System.Text;
using OnlineMazeGame;

class Home
{
    static void Main()
    {
        Console.Clear();
        Console.OutputEncoding = Encoding.Unicode;  // crucial
        Console.WriteLine("Server (S) or Client (C)");
        if (Console.ReadLine()?.ToUpper() == "S")
            Server.Start();
        else
            Client.Start();
    }
}