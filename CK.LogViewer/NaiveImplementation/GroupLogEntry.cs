using System;
using System.Collections.Generic;
using System.Text;

namespace CK.LogViewer.NaiveImplementation
{
    public class GroupLogEntry
    {
        public SimpleLogEntry OpenLog { get; set; }

        public IList<SimpleLogEntry> GroupLogs { get; set; }

        public SimpleLogEntry CloseLog { get; set; }
    }
}
