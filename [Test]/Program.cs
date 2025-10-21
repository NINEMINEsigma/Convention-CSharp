using Convention;
using Convention.EasySave;
using Convention.RScript;
using System;
using System.IO;

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

        
        var result = engine.Compile(@"
int i= 2;
int count = 0;
label(test);
goto(true,func1);
Func(i);
goto(100>i,test);

goto(context.ExistNamespace(""x""),end);
namespace(x)
{
    Func(""xxx"");
}

namespace(func1)
{
    i = Pow(i,2);
    count = count + 1;
    Func(count);
}

label(end);
", import);
        var data = RScriptSerializer.SerializeClass(result);
        var file = new ToolFile("F:\\test_after_run.dat");
        file.SaveAsBinary(data);
        data = file.LoadAsBinary();
        engine.Run(RScriptSerializer.DeserializeClass(data), import);
        return;
        var data2 = RScriptSerializer.SerializeClass(engine.GetCompileResultFromCurrent());
        var file2 = new ToolFile("F:\\test_after_run.dat");
        file2.SaveAsBinary(data2);
    }
}