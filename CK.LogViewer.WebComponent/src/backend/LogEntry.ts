import { LogEntryElement } from "../components/Viewer/LogEntryElement";
import { CKExceptionData } from "./CKExceptionData";
import { LogLevel } from "./LogLevel";
import { LogType } from "./LogType";

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
        groupOffset: number
    }[],
    logType: LogType
}

