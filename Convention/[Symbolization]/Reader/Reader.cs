using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convention.Symbolization.Internal
{
    internal class Reader
    {
        private ToolFile FileCore;

        public Reader(string file)
        {
            FileCore = new(file);
            if (FileCore.Exists() == false)
                throw new FileNotFoundException($"{file} not exists");
            FileCore.Open(FileMode.Open);
        }

        public SymbolizationContext BuildContext()
        {
            SymbolizationContext context = new();
            using StreamReader reader = new(FileCore.OriginControlStream);
            for (; reader.EndOfStream==false;)
            {
                var line = reader.ReadLine();
                    }
        }
    }
}
