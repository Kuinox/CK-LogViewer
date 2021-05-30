import { CKExceptionData } from "./CKExceptionData";
import { LogLevel } from "./LogLevel";

export interface SimpleLog {
    isGroup: false,
    offset: number,
    logLevel: LogLevel,
    logTime: string,
    monitorId: string,
    text: string,
    tags: string,
    exception?: CKExceptionData
}
