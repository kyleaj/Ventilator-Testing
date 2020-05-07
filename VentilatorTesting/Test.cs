using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VentilatorTesting
{
    public class Test<T>
    {
        public string Name;
        DateTime Created;
        public FlushableList<T> Data;
    }
}
