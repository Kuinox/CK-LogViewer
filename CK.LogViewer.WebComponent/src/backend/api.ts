import { LogEntry } from "LogType";

export async function  getLogs(): Promise<LogEntry[]> {
    const result = await fetch("http://localhost:5000/api/LogViewer?depth=2");
    if(!result.ok) throw new Error("Request response is not OK.");
    const json = await result.json();
    return json as LogEntry[];
}
