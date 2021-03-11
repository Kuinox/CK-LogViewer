import { LogEntry, SimpleLog } from "../LogType";

export class LogEntryElement extends HTMLElement {
    static create(log: SimpleLog) {
        const entry = document.createElement("log-entry");
        const span = document.createElement("span");
        span.innerHTML = log.text;
        entry.appendChild(span);
        return entry;
    }
}

customElements.define('log-entry', LogEntryElement);

