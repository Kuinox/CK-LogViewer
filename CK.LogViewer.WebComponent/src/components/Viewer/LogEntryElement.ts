import { LogLevel, logLevelToString } from "../../backend/LogLevel";
import { LogEntry } from "../../backend/LogEntry";
import { LogType } from "../../backend/LogType";
import { LogExceptionElement } from "./LogExceptionElement";
import { CssClassManager } from "./CssClassManager";

const groupTabClassName = "group-tab";
const rulerGroupClassNamePart = "ruler-group";
export class LogEntryElement extends HTMLElement {
    // private rulerData: {
    //     logLevel: LogLevel;
    //     groupOffset: number;
    //     element: HTMLElement;
    // }[];
    private previous: LogEntryElement | undefined;
    private next: LogEntryElement | undefined;
    public logData: LogEntry;
    constructor(log: LogEntry, previous: LogEntryElement | undefined, cssClassManager: CssClassManager, onRulerClick: (entry: LogEntryElement, groupOffset: number) => void) {
        super();
        this.previous = previous;
        if (this.previous !== undefined) {
            this.previous.next = this;
        }
        for (let i = 0; i < log.parentsLogLevel.length - 1; i++) {
            this.appendRuler(log.parentsLogLevel[i].logLevel, log.parentsLogLevel[i].groupOffset, cssClassManager, onRulerClick, undefined);
        }
        if (log.parentsLogLevel.length > 1) {
            if (log.logType === LogType.CloseGroup) {
                this.appendRuler(log.parentsLogLevel[log.parentsLogLevel.length - 1].logLevel, log.parentsLogLevel[log.parentsLogLevel.length - 1].groupOffset, cssClassManager, onRulerClick, "ruler-close");
            } else {
                this.appendRuler(log.parentsLogLevel[log.parentsLogLevel.length - 1].logLevel, log.parentsLogLevel[log.parentsLogLevel.length - 1].groupOffset, cssClassManager, onRulerClick, undefined);
            }
        }
        if (log.logType === LogType.OpenGroup) {
            this.appendRuler(log.logLevel, log.offset, cssClassManager, onRulerClick, "ruler-open");
        }
        this.updateRulers(true);
        this.logData = log;
        const logContent = document.createElement("div");
        logContent.classList.add("log-content");
        this.append(logContent);

        if (log.exception != null) {
            logContent.append(new LogExceptionElement(log.exception));
        }
        const span = document.createElement("span");
        span.className = "log-text";
        span.innerHTML = log.text;
        logContent.append(span);

        const logLevel = logLevelToString.get(log.logLevel & LogLevel.Mask);
        if (logLevel === undefined) throw Error("Invalid log level.");
        this.classList.add(logLevel);
        this.updateRulers(true);
    }

    /**
     *
     * @returns OpenGroup element.
     */
    public static runOnGroup(groupOffset: number, delegate: (entry: LogEntryElement) => void): void {
        Array.prototype.slice.call<HTMLCollectionOf<Element>, [], Element[]>(
            document.getElementsByClassName(LogEntryElement.getRulerClassByOffset(groupOffset))
        ).map<LogEntryElement>(s => s.parentElement as LogEntryElement).forEach(delegate);
    }

    private static getRulerClassByOffset(groupOffset: number) {
        return rulerGroupClassNamePart + groupOffset;
    }
    private static getOffsetByRulerClass(rulerClass: string) {
        return Number.parseInt(rulerClass.slice(rulerGroupClassNamePart.length));
    }

    private static isOffsetClass(className: string) {
        return className.startsWith(rulerGroupClassNamePart);
    }

    private static getOffsetFromRuler(ruler: Element) {
        let className: string;
        ruler.classList.forEach(s => {
            if (this.isOffsetClass(s)) {
                className = s;
            }
        });
        return this.getOffsetByRulerClass(className!);
    }

    private appendRuler(
        logLevel: LogLevel,
        groupOffset: number,
        cssClassManager: CssClassManager,
        onRulerClick: (entry: LogEntryElement, groupOffset: number) => void,
        classToApply: string | undefined
    ) {
        const tabContainer = document.createElement("div");
        const ruleName = LogEntryElement.getRulerClassByOffset(groupOffset);
        tabContainer.classList.add(groupTabClassName, ruleName);
        this.append(tabContainer);
        const rule = "." + ruleName + "{background-color: rgba(255, 255, 255, 0.1);}";
        tabContainer.onmouseenter = () => cssClassManager.requireClass(ruleName, rule);
        tabContainer.onmouseleave = () => cssClassManager.releaseClass(ruleName);
        tabContainer.onclick = () => onRulerClick(this, groupOffset);
        const tab = document.createElement("div");
        const logLevelStr = logLevelToString.get(logLevel & LogLevel.Mask);
        if (logLevelStr === undefined) throw new Error("Invalid Data: Unknown Log Level.");
        tab.classList.add(logLevelStr, "ruler-unconnected");
        if (classToApply !== undefined) {
            tab.classList.add(classToApply);
        }
        tabContainer.appendChild(tab);
    }
    private getRulers() {
        return Array.prototype.slice.call<HTMLCollectionOf<Element>, [], Element[]>(this.getElementsByClassName(groupTabClassName));
    }
    private updateRulers(propagate: boolean) {
        const rulers = this.getRulers();
        const previousRulers = this.previous?.getRulers();
        const nextRulers = this.next?.getRulers();
        if(this.previous === undefined) {
            debugger;
        }
        for (let i = 0; i < rulers.length; i++) {
            const current = rulers[i];
            const currentOffset = LogEntryElement.getOffsetFromRuler(current);
            const connectedTop = previousRulers?.find(s => LogEntryElement.getOffsetFromRuler(s) === currentOffset);
            const connectedBot = nextRulers?.find(s => LogEntryElement.getOffsetFromRuler(s) === currentOffset);
            current.classList.remove("ruler-unconnected", "ruler-unconnected-top", "ruler-unconnected-bottom");
            if (!connectedBot && !connectedTop) {
                current.classList.add("ruler-unconnected");
            } else if (!connectedBot && connectedTop) {
                current.classList.add("ruler-unconnected-bot");
            } else if (connectedBot && !connectedTop
                && !(i === rulers.length - 1 && this.logData.logType === LogType.OpenGroup) //not an open group log.
            ) {
                current.classList.add("ruler-unconnected-top");
            }
        }
        if (propagate) {
            this.previous?.updateRulers(false);
            this.next?.updateRulers(false);
        }
    }
}

customElements.define('log-entry', LogEntryElement);
