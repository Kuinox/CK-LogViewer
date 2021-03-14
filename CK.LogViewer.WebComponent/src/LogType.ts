export interface GroupLog {
    isGroup: true,
    openLog: SimpleLog,
    groupLogs: LogEntry[],
    closeLog: SimpleLog
}

export interface SimpleLog {
    isGroup: false,
    offset: number,
    logOffset: number,
    logLevel: number,
    logTime: string,
    monitorId: string,
    text: string,
    exception?: CKExceptionData
}

export interface CKExceptionData {
    stackTrace: string,
    typeException: string,
    message: string
}



export type LogEntry = GroupLog | SimpleLog;
