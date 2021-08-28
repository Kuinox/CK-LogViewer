import { LogLevel, logLevelToString } from "../../backend/LogLevel";
import { LogEntry, LogType } from "../../backend/LogEntry";
import { LogExceptionElement } from "./LogExceptionElement";

export class LogEntryElement extends HTMLElement {
    public readonly isGroup = false;
    private rulerData: {
        logLevel: LogLevel;
        groupOffset: number;
    }[];
    private previous: LogEntryElement | undefined;
    private rulers: HTMLDivElement[] = [];
    constructor(elementToAppend: HTMLElement, log: LogEntry, previous: LogEntryElement | undefined) {
        super();
        elementToAppend.append(this);
        this.rulerData = log.parentsLogLevel;
        if (log.logType === LogType.OpenGroup) {
            this.rulerData.push({
                logLevel: log.logLevel,
                groupOffset: log.offset
            });
        }
        this.previous = previous;
        const groupParentCount = log.parentsLogLevel.length;
        const span = document.createElement("span");
        span.className = "log-text";
        span.innerHTML = log.text;
        this.prepend(span);
        if (log.exception != null) {
            this.prepend(new LogExceptionElement(log.exception));
        }
        const spacer = document.createElement("span");
        spacer.style.width = (groupParentCount * 20) + "px";
        this.prepend(spacer);
        const logLevel = logLevelToString.get(log.logLevel & LogLevel.Mask);
        if (logLevel === undefined) throw Error("Invalid log level.");
        this.classList.add(logLevel);

        for (let i = groupParentCount - 1; i >= 0; i--) {
            const parentRuler = previous?.rulerData[i];
            if (parentRuler === undefined || parentRuler.groupOffset !== log.parentsLogLevel[i]?.groupOffset) {
                const isLast = i == groupParentCount - 1;
                const logLevelStr = logLevelToString.get(
                    (!isLast ?
                        log.parentsLogLevel[i].logLevel
                        : log.logLevel) & LogLevel.Mask
                );
                if (logLevelStr === undefined) throw new Error("Invalid Data: Unknown Log Level.");
                this.prependRuler(logLevelStr);
            } else {
                this.growRuler(i, this.clientHeight);
            }
        }





    }

    private prependRuler(logLevelStr: string) {
        const tabContainer = document.createElement("div");
        tabContainer.classList.add("group-tab", logLevelStr);
        const tab = document.createElement("div");
        tabContainer.style.height = this.offsetHeight + "px";
        tabContainer.appendChild(tab);
        this.rulers.push(tabContainer);
        this.prepend(tabContainer);
    }

    private growRuler(rulerIndex: number, pixelCount: number) {
        const targetedGroupOffset = this.rulerData[rulerIndex].groupOffset;
        const previousGroupOffset = this.previous?.rulerData[rulerIndex]?.groupOffset;
        if (previousGroupOffset === undefined || targetedGroupOffset != previousGroupOffset) {
            this.doGrowRuler(rulerIndex, pixelCount);
        } else {
            this.previous?.growRuler(rulerIndex, pixelCount);
        }
    }

    private doGrowRuler(rulerIndex: number, pixelCount: number) {
        debugger;
        const heightString = this.rulers[rulerIndex].style.height;
        const height = Number.parseInt(heightString.slice(0, heightString.indexOf("px")));
        this.rulers[rulerIndex].style.height = (height + pixelCount) + "px";
    }
}

customElements.define('log-entry', LogEntryElement);

