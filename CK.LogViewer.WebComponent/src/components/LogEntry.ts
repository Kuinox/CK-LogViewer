import { LogEntry, SimpleLog } from "../LogType";

export function appendLogEntry(html: string[], logEntry: LogEntry) {
    if (logEntry.isGroup == false) {
        html.push("<log-entry>");
        html.push(logEntry.text);
        html.push("</log-entry>");
    } else {
        appendSimpleLog(html, logEntry.openLog);
        html.push("<log-list>");
        const arr = logEntry.groupLogs;
        for (let i = 0; i < arr.length; i++) {
            const element = logEntry.groupLogs[i];
            appendLogEntry(html, element);
        }
        html.push("</log-list>");
        appendSimpleLog(html, logEntry.closeLog);
    }
    return html.join();
}

function appendSimpleLog(html: string[], logEntry: SimpleLog) {
    html.push("<log-entry>");
    html.push(logEntry.text);
    html.push("</log-entry>");
}

