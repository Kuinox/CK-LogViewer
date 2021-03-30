import { CKExceptionData } from "./CKExceptionData";
import { LogLevel } from "./LogLevel";

export interface SimpleLog {
    isGroup: false,
    id: number,
    logOffset: number,
    logLevel: LogLevel,
    logTime: string,
    monitorId: string,
    text: string,
    exception?: CKExceptionData
}
