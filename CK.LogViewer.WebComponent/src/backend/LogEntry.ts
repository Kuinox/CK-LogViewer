import { CKExceptionData } from "./CKExceptionData";
import { LogLevel } from "./LogLevel";

export interface LogEntry {
    offset: number,
    groupOffset: number,
    logLevel: LogLevel,
    logTime: string,
    monitorId: string,
    text: string,
    tags: string,
    exception?: CKExceptionData,
    parentsLogLevel: {
        logLevel: LogLevel,
        offset: number
    }[],
    logType: LogType
}

export enum LogType {
    Line = 1,
    OpenGroup = 2,
    CloseGroup = 3
}
