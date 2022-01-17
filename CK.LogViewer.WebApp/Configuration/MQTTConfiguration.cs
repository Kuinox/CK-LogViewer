using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.LogViewer.WebApp.Configuration
{
    public class MQTTConfiguration
    {
        public string ConnectionString { get; set; } = null!;
        public int LogBufferSize { get; set; } = 500;
    }
}
