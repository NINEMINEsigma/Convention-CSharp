using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Convention;
using Convention.EasySave;

public class Program
{
    class Vector2
    {
        public double x, y;
    }
    class Vector2X2
    {
        public double x, y;
        public Vector2 next;
    }

    static void Main(string[] args)
    {
        Directory.CreateDirectory(PlatformIndicator.PersistentDataPath);
        Console.WriteLine(PlatformIndicator.PersistentDataPath);
        EasySave.Save("key", new Vector2X2()
        {
            x = 10,
            y = 20,
            next = new()
            {
                x = 30,
                y = 40
            }
        }, "test.json");
    }
}