import { GroupStats } from "./GroupStats";
import { LogEntry } from "./LogEntry";
import { SimpleLog } from "./SimpleLog";

export interface GroupLog {
    isGroup: true,
    openLog: SimpleLog,
    isFolded: boolean,
    groupLogs: LogEntry[],
    stats: GroupStats,
    closeLog: SimpleLog
}
