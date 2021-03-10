import { getLogs } from "../api";
import { appendLogEntry } from "./LogEntry";

export class LogViewer extends HTMLElement {

    async connectedCallback() {
        this.innerHTML = `<h1>Loading...</h1>`
        const logs = await getLogs();
        const html = [];
        html.push("<log-list>");
        for (let i = 0; i < logs.length; i++) {
            const element = logs[i];
            appendLogEntry(html, element);

        }
        html.push("</log-list>");
        this.innerHTML = html.join("");
    }

}

customElements.define('log-viewer', LogViewer);
