import { getLogs } from "../backend/api";
import { LogGroupElement } from "./GroupElement/LogGroupElement";

export class LogViewer extends HTMLElement {

    async connectedCallback(): Promise<void> {
        this.innerHTML = `<h1>Loading...</h1>`;
        const logs = await getLogs();
        this.innerHTML = "";
        const groups = document.createElement("div");
        for (let i = 0; i < logs.length; i++) {
            const curr = logs[i];
            groups.appendChild(LogGroupElement.fromLogEntry(curr));
        }
        this.appendChild(groups);
    }
}

customElements.define('log-viewer', LogViewer);
