import { getLogs } from "../api";
import { LogEntryElement } from "./LogEntryElement";
import { LogGroupElement } from "./LogGroupElement";

export class LogViewer extends HTMLElement {

    async connectedCallback() {
        this.innerHTML = `<h1>Loading...</h1>`
        const logs = await getLogs();
        this.innerHTML = "";
        for (let i = 0; i < logs.length; i++) {
            const element = logs[i];
            this.appendChild(element.isGroup ? LogGroupElement.create(element) : LogEntryElement.create(element));
        }
    }

}

customElements.define('log-viewer', LogViewer);
