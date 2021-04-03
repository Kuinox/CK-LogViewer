import { getLogs } from "../../backend/api";
import { LogEntry } from "../../backend/LogEntry";
import { LogGroupElement } from "./LogGroup/LogGroupElement";

export class LogViewer extends HTMLElement {

    constructor() {
        super();
    }

    async connectedCallback(): Promise<void> {
        if (window.location.hash.length > 1) {
            const name = window.location.hash.substring(2);
            const logs = await getLogs(name);
            this.render(logs, name);
        }
    }

    render(logs: LogEntry[], filename: string): void {
        const groups = document.createElement("div");
        for (let i = 0; i < logs.length; i++) {
            const curr = logs[i];
            groups.appendChild(LogGroupElement.fromLogEntry(curr, filename));
        }
        this.appendChild(groups);
    }
}

customElements.define('log-viewer', LogViewer);
