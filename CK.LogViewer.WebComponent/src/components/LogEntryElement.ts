import { LogEntry, SimpleLog } from "../LogType";

export class LogEntryElement extends HTMLElement {
    constructor() {
        super();
    }

    static create(log: SimpleLog) {
        const entry = new LogEntryElement();
        const span = document.createElement("span");
        span.innerHTML = log.text;
        entry.appendChild(span);
        return entry;
    }
}

customElements.define('log-entry', LogEntryElement);

