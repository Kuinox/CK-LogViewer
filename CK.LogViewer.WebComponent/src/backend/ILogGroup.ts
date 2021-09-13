import { ILogEntry } from "./ILogEntry";
import { GroupStats } from "./GroupStats";


export interface ILogGroup extends ILogEntry {
    stats: GroupStats;
}
