import { GroupLog } from "./GroupLog";
import { LogEntry } from "./LogEntry";

export async function getLogs(name:string): Promise<LogEntry[]> {
    const result = await fetch(`http://localhost:5000/api/LogViewer/${name}?depth=2`);
    if(!result.ok) throw new Error("Request response is not OK.");
    const json = await result.json();
    return json as LogEntry[];
}

export async function getGroupLogs(name: string, groupOffset: number): Promise<GroupLog>{
    const result = await fetch(`http://localhost:5000/api/LogViewer/${name}/group/${groupOffset}`);
    if(!result.ok) throw new Error("Request response is not OK.");
    const json = await result.json();
    return json as GroupLog;
}

export async function uploadLog(formData: FormData): Promise<string> {
    const result = await fetch("http://localhost:5000/api/LogViewer/", {
        method: "post",
        body: formData,
    });
    return await result.text();
}

