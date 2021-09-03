import { LogType } from "./LogType";
import { LogEntry } from "./LogEntry";
import { GroupStats } from "./GroupStats";


export interface LogGroup extends LogEntry {
    logType: LogType.OpenGroup;
    stats: GroupStats;
}
