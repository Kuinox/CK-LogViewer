export interface GroupLog {
    isGroup: true,
    openLog: SimpleLog,
    groupLogs: LogEntry[],
    closeLog: SimpleLog
}

export type SimpleLog = {
    offset: number,
    logOffset: number,
    logLevel: number,
    logTime: string,
    monitorId: string,
    isGroup: false,
    text: string
}

export type LogEntry = GroupLog | SimpleLog;
