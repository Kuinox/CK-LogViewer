import { ICKExceptionData } from "./ICKExceptionData";
import { LogLevel } from "./LogLevel";
import { LogType } from "./LogType";

export interface ILogEntry{
    logTime: string,
    monitorId: string,
    text?: string,
    tags: string,
    exception?: ICKExceptionData,
    offset: number,
    groupOffset: number,
    logLevel: LogLevel,
    parentsLogLevel: {
        logLevel: LogLevel;
        groupOffset: number;
    }[],
    logType: LogType
}
