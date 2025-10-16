using Convention.RScript;
using System;

public class Program
{
    static class Test
    {
        public static object Func(object x)
        {
            Console.WriteLine(x);
            return x;
        }
    }

    static void Main(string[] args)
    {
        RScriptEngine engine = new();
        RScriptImportClass import = new()
        {
            typeof(Math),
            typeof(ExpressionMath),
            typeof(Test)
        };
        var result = engine.Run(@"
int i= 2;
int count = 0;
label(test);
goto(true,func1);
Func(i);
goto(100>i,test);

namespace(func1)
{
    i = Pow(i,2);
    count = count + 1;
    Func(count);
}
", import);
        Console.WriteLine($"Script executed successfully. Result: {result["i"].data}");
    }
}