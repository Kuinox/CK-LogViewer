export async function  getLogs(): Promise<LogEntry[]> {
    const result = await fetch("http://localhost:5000/LogViewer/");
    const json = await result.json();
    return json as LogEntry[];
}
