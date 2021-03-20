import { getLogs } from "../backend/api";
import { LogEntryElement } from "./LogEntryElement";
import { LogGroupElement } from "./LogGroupElement";

export class LogViewer extends HTMLElement {

    constructor() {
        super();
    }

    async connectedCallback(): Promise<void> {

        if (window.location.hash.length > 1) {
            const name = window.location.hash.substring(2);
            const logs = await getLogs(name);
            this.render(logs);
        }

    }

    render(logs: any) {
        const groups = document.createElement("div");
        for (let i = 0; i < logs.length; i++) {
            const curr = logs[i];
            groups.appendChild(curr.isGroup ? LogGroupElement.create(curr) : LogEntryElement.create(curr));
        }
        this.appendChild(groups);
    }
}

customElements.define('log-viewer', LogViewer);
