import { LogLevel, logLevelToString } from "../backend/LogLevel";
import { SimpleLog } from "../backend/SimpleLog";
import { setElementOptions, ClassOptions } from "../helpers/domHelpers";
import { LogExceptionElement } from "./LogExceptionElement";

export class LogEntryElement extends HTMLElement {
    public readonly isGroup = false;

    constructor(log: SimpleLog, options?: ClassOptions) {
        super();
        setElementOptions(this, options);
        const logLevel = logLevelToString.get(log.logLevel & LogLevel.Mask);
        if (logLevel === undefined) throw Error("Invalid log level.");
        const time = document.createElement("span");
        time.innerHTML = log.logTime;
        time.classList.add("log-timestamp");
        this.appendChild(time);
        if (log.exception != null) {
            this.appendChild(new LogExceptionElement(log.exception));
        }

        this.classList.add(logLevel);
        const span = document.createElement("span");
        span.className="log-text";
        span.innerHTML = log.text;
        this.appendChild(span);
    }
}

customElements.define('log-entry', LogEntryElement);

