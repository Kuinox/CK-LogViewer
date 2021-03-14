import { getLogs } from "../backend/api";
import { LogEntryElement } from "./LogEntryElement";
import { LogGroupElement } from "./LogGroupElement";

export class LogViewer extends HTMLElement {

    async connectedCallback(): Promise<void> {
        this.innerHTML = `<h1>Loading...</h1>`;
        const logs = await getLogs();
        this.innerHTML = "";
        const groups = document.createElement("div");
        for (let i = 0; i < logs.length; i++) {
            const curr = logs[i];
            groups.appendChild(curr.isGroup ? LogGroupElement.create(curr) : LogEntryElement.create(curr));
        }
        this.appendChild(groups);
    }
}

customElements.define('log-viewer', LogViewer);
