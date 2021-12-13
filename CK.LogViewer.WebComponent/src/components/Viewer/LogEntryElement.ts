import { LogLevel, logLevelToString } from "../../backend/LogLevel";
import { ILogEntry } from "../../backend/ILogEntry";
import { LogExceptionElement } from "./LogExceptionElement";
import { CssClassManager } from "./CssClassManager";
import { LogLineBaseElement, LogViewerState, OnClickRulerCallback } from "./LogLineBaseElement";
import { ColorGenerator } from "../../helpers/colorGenerator";
import { LogType } from "../../backend/LogType";
import { ICloseGroup } from "../../backend/ICloseGroup";


export class LogEntryElement extends LogLineBaseElement {
    constructor(
        log: ILogEntry,
        cssClassManager: CssClassManager,
        colorGenerator: ColorGenerator,
        logviewerState: LogViewerState,
        onRulerClick: OnClickRulerCallback
    ) {
        super(log, cssClassManager, colorGenerator, logviewerState, onRulerClick);
        const logContent = document.createElement("div");
        logContent.classList.add("log-content");
        this.append(logContent);

        if (log.exception != null) {
            logContent.append(new LogExceptionElement(log.exception));
        }
        const span = document.createElement("span");
        span.className = "log-text";
        span.innerText = log.text ?? "";
        if(log.logType == LogType.CloseGroup) {
            span.innerText = (log as ICloseGroup).conclusions?.map(s=>s.text)?.join("\n") ?? "";
        }
        logContent.append(span);

        const logLevel = logLevelToString.get(log.logLevel & LogLevel.Mask);
        if (logLevel === undefined) throw Error("Invalid log level.");
        this.classList.add(logLevel);
    }
}

customElements.define('log-entry', LogEntryElement);
