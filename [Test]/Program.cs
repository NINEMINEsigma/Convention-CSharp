using System;
using System.Collections.Generic;
using System.Threading;
using Convention;

public class Program
{
    class Slot1
    {

    }
    class Slot2
    {

    }
    class Slot3
    {

    }
    class Slot4
    {

    }

    static void Main(string[] args)
    {
        var seed = new Random(2049);
        for (int i = 0; i < 100000; i++)
        {
            Convention.Architecture.InternalReset();
            var slot1 = new Slot1();
            var slot2 = new Slot2();
            var slot3 = new Slot3();
            var slot4 = new Slot4();
            List<Action> list = new() {
                ()=> Convention.Architecture.Register(slot1,()=>{ },typeof(Slot2),typeof(Slot3),typeof(Slot4)),
                ()=> Convention.Architecture.Register(slot2,()=>{ },typeof(Slot3),typeof(Slot4)),
                ()=> Convention.Architecture.Register(slot3,()=>{ },typeof(Slot4)),
                ()=> Convention.Architecture.Register(slot4,()=>{ })};
            list.Sort((x, y) => seed.Next() > seed.Next() ? 1 : -1);
            foreach (var item in list)
            {
                item();
            }
            Console.Write($"{i}\t\r"); 
        }
    }
}