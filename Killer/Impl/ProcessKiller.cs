using System.Diagnostics;

namespace Restarter.Killer.Impl
{
    internal class ProcessKiller : Killer
    {
        public void Kill(Process process)
        {
            process.Kill();
        }
    }
}
