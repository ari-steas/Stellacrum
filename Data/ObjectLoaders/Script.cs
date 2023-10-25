using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stellacrum.Data.ObjectLoaders
{
    internal class Script
    {
        public string Code;
        public string Name;

        public Script(string code, string name)
        {
            Code = code;
            Name = name;
        }
    }
}
