import { ILogEntry } from "./ILogEntry";

export class Api {
    filename: string;
    constructor(filename: string) {
        this.filename = filename;
    }

    async getLogs(abortSignal?: AbortSignal): Promise<ILogEntry[]> {
        const result = await fetch(`api/LogViewer/${this.filename}`, { signal: abortSignal });
        if (!result.ok) throw new Error("Request response is not OK.");
        const json = await result.json();
        return json as ILogEntry[];
    }

    async getGroupLogs(groupOffset: number, abortSignal?: AbortSignal): Promise<ILogEntry[]> {
        if(groupOffset === undefined ) throw Error("Invalid argument.");
        const result = await fetch(`api/LogViewer/${this.filename}?groupOffset=${groupOffset}`, { signal: abortSignal });
        if (!result.ok) throw new Error("Request response is not OK.");
        const json = await result.json();
        return json as ILogEntry[];
    }


}



export async function uploadLog(formData: FormData): Promise<string> {
    const result = await fetch(`api/LogViewer/`, {
        method: "post",
        body: formData,
    });
    return await result.text();
}
