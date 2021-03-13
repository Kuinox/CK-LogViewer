import { getLogs } from "../api";
import { LogEntryElement } from "./LogEntryElement";
import { LogGroupElement } from "./LogGroupElement";

export class LogViewer extends HTMLElement {

    constructor() {
        super();
        document.addEventListener("pass-log-data", (event) => {
            this.innerHTML = "";
            const eventAny = event as any;
            const { logs } = eventAny.detail;
            const groups = document.createElement("div");
            for (let i = 0; i < logs.length; i++) {
                const curr = logs[i];
                groups.appendChild(curr.isGroup ? LogGroupElement.create(curr) : LogEntryElement.create(curr));
            }
            this.appendChild(groups);
        });
    }
}

customElements.define('log-viewer', LogViewer);
