using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Runtime;


namespace Word_Counter.parser
{
    abstract class AbstractFileParser
    {   
        public abstract Task<Dictionary<string, int>> parse(System.IO.Stream s, Form parent, System.Threading.CancellationToken cancellationToken);   
    }
}
