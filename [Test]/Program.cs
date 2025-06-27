using System;
using System.Collections.Generic;
using System.Threading;
using Convention;

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