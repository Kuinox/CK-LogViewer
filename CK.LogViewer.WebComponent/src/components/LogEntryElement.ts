import { LogLevel, logLevelToString } from "../backend/LogLevel";
import { SimpleLog } from "../backend/SimpleLog";
import { setElementOptions, ClassOptions } from "../helpers/domHelpers";
import { LogExceptionElement } from "./LogExceptionElement";

export class LogEntryElement extends HTMLElement {
    constructor() {
        super();
    }

    static create(log: SimpleLog, options?: ClassOptions): LogEntryElement {
        const entryElement = new LogEntryElement();
        setElementOptions(entryElement, options);
        const logLevel = logLevelToString.get(log.logLevel & LogLevel.Mask);
        if (logLevel === undefined) throw Error("Invalid log level.");
        const time = document.createElement("span");
        time.innerHTML = log.logTime;
        time.classList.add("log-timestamp");
        entryElement.appendChild(time);
        if (log.exception != null) {
            entryElement.appendChild(new LogExceptionElement(log.exception));
        }

        entryElement.classList.add(logLevel);
        const span = document.createElement("span");
        span.innerHTML = log.text;
        entryElement.appendChild(span);



        return entryElement;
    }
}

customElements.define('log-entry', LogEntryElement);

