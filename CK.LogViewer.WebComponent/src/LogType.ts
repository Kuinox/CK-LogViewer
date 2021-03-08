export interface GroupLog {
    isGroup: true,
    openLog: SimpleLog,
    groupLogs: LogEntry[],
    closeLog: SimpleLog
}

export type SimpleLog = {
    isGroup: false,
    text: string
}

export type LogEntry = GroupLog | SimpleLog;
