import { LogLevel, logLevelToString } from "../../backend/LogLevel";
import { LogEntry, LogType } from "../../backend/LogEntry";
import { LogExceptionElement } from "./LogExceptionElement";

export class LogEntryElement extends HTMLElement {
    public readonly isGroup = false;
    private readonly parentsLogLevel: {
        logLevel: LogLevel,
        offset: number
    }[];
    private previous: LogEntryElement | undefined;
    constructor(log: LogEntry, previous: LogEntryElement | undefined) {
        super();
        debugger;
        this.parentsLogLevel = log.parentsLogLevel;
        const groupParentCount = log.logType !== LogType.OpenGroup ? log.parentsLogLevel.length - 1 : log.parentsLogLevel.length;
        for (let i = 0; i < groupParentCount + 1; i++) {
            const parentRuler = previous?.parentsLogLevel[i];
            if (parentRuler === undefined || parentRuler.offset !== log.parentsLogLevel[i].offset) {
                const isLast = i == groupParentCount;
                const logLevelStr = logLevelToString.get(
                    (!isLast ?
                        log.parentsLogLevel[i].logLevel
                        : log.logLevel) & LogLevel.Mask
                );
                if (logLevelStr === undefined) throw new Error("Invalid Data: Unknown Log Level.");
                this.appendRuler(logLevelStr, isLast, log);
            } else {

            }
        }
        const logLevel = logLevelToString.get(log.logLevel & LogLevel.Mask);
        if (logLevel === undefined) throw Error("Invalid log level.");

        if (log.exception != null) {
            this.appendChild(new LogExceptionElement(log.exception));
        }
        const span = document.createElement("span");
        this.classList.add(logLevel);
        span.className = "log-text";
        span.innerHTML = log.text;
        this.appendChild(span);
    }

    private appendRuler(logLevelStr: string, isLast: boolean, log: LogEntry) {
        const tab = document.createElement("div");


        tab.classList.add("group-tab", logLevelStr);
        if (isLast) {
            switch (log.logType) {
                case LogType.CloseGroup:
                    tab.classList.add("group-tab-close");
                    break;
                case LogType.Line:
                    tab.classList.add("group-tab-line");
                    break;
                case LogType.OpenGroup:
                    tab.classList.add("group-tab-open");
                    break;
            }
        }
        tab.appendChild(document.createElement("div"));
        this.appendChild(tab);
    }

    private growRuler(rulerIndex: number) {

    }
}

customElements.define('log-entry', LogEntryElement);

