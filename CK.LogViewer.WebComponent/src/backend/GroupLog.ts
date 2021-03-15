import { GroupStats } from "./GroupStats";
import { LogEntry } from "./LogEntry";
import { SimpleLog } from "./SimpleLog";

export interface GroupLog {
    isGroup: true,
    openLog: SimpleLog,
    groupLogs: LogEntry[],
    groupStats: GroupStats,
    closeLog: SimpleLog
}
