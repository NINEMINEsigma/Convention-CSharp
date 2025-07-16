using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Convention;
using Convention.EasySave;
using Convention.Symbolization;
using Convention.Test;

public class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Convention-CSharp 测试程序");
        Console.WriteLine("==========================");
        
        // 运行文件功能测试
        FileTest.RunTests();
        
        Console.WriteLine("\n按任意键退出...");
        Console.ReadKey();
    }
}