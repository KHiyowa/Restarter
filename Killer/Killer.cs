using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restarter.Killer
{
    internal interface Killer
    {
        public void Kill(Process process);
    }
}
