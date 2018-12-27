using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volcano.Engine
{
    interface IStatus
    {
        event EventHandler<EngineStatus> OnStatus;
    }
}
