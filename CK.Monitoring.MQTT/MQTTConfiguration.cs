using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Monitoring.MQTT
{
    public class MQTTConfiguration : IHandlerConfiguration
    {
        public string ConnectionString { get; set; } = null!;
        public bool LaunchLogViewer { get; set; } = false;
        public IHandlerConfiguration Clone()
        {
            return new MQTTConfiguration()
            {
                ConnectionString = ConnectionString,
                LaunchLogViewer = LaunchLogViewer
            };
        }
    }
}
