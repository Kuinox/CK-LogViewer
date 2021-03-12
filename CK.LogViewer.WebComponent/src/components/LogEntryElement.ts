import { LogEntry, SimpleLog } from "../LogType";
import { LogExceptionElement } from "./LogExceptionElement";

export class LogEntryElement extends HTMLElement {
    constructor() {
        super();
    }

    static create(log: SimpleLog) {
        const entry = new LogEntryElement();

        if (log.exception != null) {
            const exception = LogExceptionElement.create(log.exception);
            entry.appendChild(exception)
        } else {
            const span = document.createElement("span");
            span.innerHTML = log.text;
            entry.appendChild(span);

        }
        return entry;
    }
}

customElements.define('log-entry', LogEntryElement);

