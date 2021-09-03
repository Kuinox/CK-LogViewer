using CK.Monitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.LogViewer
{
    public interface ILogViewerConfig
    {
        int LineCount( IMulticastLogEntry logEntry );
    }
}
