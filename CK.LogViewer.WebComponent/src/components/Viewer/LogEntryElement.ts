import { LogLevel, logLevelToString } from "../../backend/LogLevel";
import { ILogEntry } from "../../backend/ILogEntry";
import { LogExceptionElement } from "./LogExceptionElement";
import { CssClassManager } from "./CssClassManager";
import { LogLineBaseElement, OnClickRulerCallback } from "./LogLineBaseElement";


export class LogEntryElement extends LogLineBaseElement {
    constructor(
        log: ILogEntry,
        cssClassManager: CssClassManager,
        onRulerClick: OnClickRulerCallback
    ) {
        super(log, cssClassManager, onRulerClick);
        const logContent = document.createElement("div");
        logContent.classList.add("log-content");
        this.append(logContent);

        if (log.exception != null) {
            logContent.append(new LogExceptionElement(log.exception));
        }
        const span = document.createElement("span");
        span.className = "log-text";
        span.innerHTML = log.text ?? "";
        logContent.append(span);

        const logLevel = logLevelToString.get(log.logLevel & LogLevel.Mask);
        if (logLevel === undefined) throw Error("Invalid log level.");
        this.classList.add(logLevel);
    }
}

customElements.define('log-entry', LogEntryElement);
