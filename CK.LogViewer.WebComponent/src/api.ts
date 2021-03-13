import { LogEntry } from "./LogType";

export async function getLogs(): Promise<LogEntry[]> {
    const result = await fetch("http://localhost:5000/LogViewer/");
    const json = await result.json();
    return json as LogEntry[];
}

export async function uploadLog(formData: FormData): Promise<LogEntry[]> {
    const result = await fetch("http://localhost:5000/LogViewer/", {
        method: "post",
        body: formData,
    });
    const json = await result.json();
    return json as LogEntry[];
}
