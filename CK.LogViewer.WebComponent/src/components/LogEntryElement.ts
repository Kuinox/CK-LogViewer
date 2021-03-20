import { LogLevel, logLevelToString } from "../backend/LogLevel";
import { SimpleLog } from "../LogType";
import { LogExceptionElement } from "./LogExceptionElement";

export class LogEntryElement extends HTMLElement {
    constructor() {
        super();
    }

    static create(log: SimpleLog): LogEntryElement {
        const entryElement = new LogEntryElement();
        const logLevel = logLevelToString.get(log.logLevel & LogLevel.Mask);
        if (logLevel === undefined) throw Error("Invalid log level.");
        const time = document.createElement("span");
        time.innerHTML = log.logTime;
        time.classList.add("log-timestamp");
        entryElement.appendChild(time);
        if (log.exception != null) {
            const exception = LogExceptionElement.create(log.exception);
            entryElement.appendChild(exception);
        }

        entryElement.classList.add(logLevel);
        const span = document.createElement("span");
        span.innerHTML = log.text;
        entryElement.appendChild(span);



        return entryElement;
    }
}

customElements.define('log-entry', LogEntryElement);

