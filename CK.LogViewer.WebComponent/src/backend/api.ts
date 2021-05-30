import { LogEntry } from "./LogEntry";

export class Api {
    filename: string;
    constructor(filename: string) {
        this.filename = filename;
    }

    async getLogs(abortSignal?: AbortSignal): Promise<LogEntry[]> {
        const result = await fetch(`api/LogViewer/${this.filename}?depth=2`, { signal: abortSignal });
        if (!result.ok) throw new Error("Request response is not OK.");
        const json = await result.json();
        return json as LogEntry[];
    }

    async getGroupLogs(groupOffset: number, abortSignal?: AbortSignal): Promise<LogEntry[]> {
        if(groupOffset === undefined ) throw Error("Invalid argument.");
        const result = await fetch(`api/LogViewer/${this.filename}?groupOffset=${groupOffset}`, { signal: abortSignal });
        if (!result.ok) throw new Error("Request response is not OK.");
        const json = await result.json();
        return json as LogEntry[];
    }


}



export async function uploadLog(formData: FormData): Promise<string> {
    const result = await fetch(`api/LogViewer/`, {
        method: "post",
        body: formData,
    });
    return await result.text();
}
