import { LogLevel, logLevelToString } from "../../backend/LogLevel";
import { LogEntry } from "../../backend/LogEntry";
import { LogType } from "../../backend/LogType";
import { LogExceptionElement } from "./LogExceptionElement";
import { CssClassManager } from "./CssClassManager";
import { GroupSummary } from "./LogGroup/GroupSummary";
import { toggleHidden } from "../../helpers/domHelpers";

export class LogEntryElement extends HTMLElement {
    private rulerData: {
        logLevel: LogLevel;
        groupOffset: number;
        element: HTMLElement;
    }[];
    private previous: LogEntryElement | undefined;
    private next: LogEntryElement | undefined;
    public logData: LogEntry;
    constructor(log: LogEntry, previous: LogEntryElement | undefined, cssClassManager: CssClassManager, onRulerClick: (entry: LogEntryElement, groupOffset: number) => void) {
        super();
        this.rulerData = [];
        for (let i = 0; i < log.parentsLogLevel.length; i++) {
            this.appendRuler(log.parentsLogLevel[i].logLevel, log.parentsLogLevel[i].groupOffset, cssClassManager, onRulerClick);
        }
        if (log.logType === LogType.OpenGroup) {
            this.appendRuler(log.logLevel, log.offset, cssClassManager, onRulerClick);
            this.rulerData[this.rulerData.length - 1].element.classList.add("ruler-open");
        } else if (log.logType === LogType.CloseGroup) {
            this.rulerData[this.rulerData.length - 1].element.classList.add("ruler-close");
        }
        this.updateRulers(true);
        this.logData = log;
        this.previous = previous;
        if (this.previous !== undefined) {
            this.previous.next = this;
        }
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
    public runOnGroup(groupOffset: number, delegate: (entry: LogEntryElement) => void): LogEntryElement {
        if (!this.isSubGroupOf(groupOffset)) throw Error("This group doesn't have the specified ruler.");
        let openGroup: LogEntryElement | undefined;
        if (this.logData.offset === groupOffset) openGroup = this;
        delegate(this);
        let curr = this.previous;
        while (curr?.isSubGroupOf(groupOffset)) {
            delegate(curr);
            if (curr.logData.offset === groupOffset) openGroup = curr;
            curr = curr.previous;
        }
        curr = this.next;
        while (curr?.isSubGroupOf(groupOffset)) {
            delegate(curr);
            if (curr.logData.offset === groupOffset) openGroup = curr;
            curr = curr.next;
        }
        if (openGroup === undefined) throw new Error("This group doesn't have any open group.");
        return openGroup;
    }

    private isSubGroupOf(groupOffset: number): boolean {
        return this.rulerData.filter(s => s.groupOffset === groupOffset)[0] !== undefined;
    }
    private appendRuler(logLevel: LogLevel, groupOffset: number, cssClassManager: CssClassManager, onRulerClick: (entry: LogEntryElement, groupOffset: number) => void) {
        const tabContainer = document.createElement("div");
        const ruleName = "ruler-group" + groupOffset;
        tabContainer.classList.add("group-tab", ruleName);
        this.append(tabContainer);
        const rule = "." + ruleName + "{background-color: rgba(255, 255, 255, 0.1);}";
        tabContainer.onmouseenter = () => cssClassManager.requireClass(ruleName, rule);
        tabContainer.onmouseleave = () => cssClassManager.releaseClass(ruleName);
        tabContainer.onclick = () => onRulerClick(this, groupOffset);
        const tab = document.createElement("div");
        const logLevelStr = logLevelToString.get(logLevel & LogLevel.Mask);
        if (logLevelStr === undefined) throw new Error("Invalid Data: Unknown Log Level.");
        tab.classList.add(logLevelStr, "ruler-unconnected");
        tabContainer.appendChild(tab);
        this.rulerData.push(
            {
                element: tab,
                groupOffset: groupOffset,
                logLevel: logLevel
            }
        );
    }

    private updateRulers(propagate: boolean) {
        for (let i = 0; i < this.rulerData.length; i++) {
            const current = this.rulerData[i];
            const connectedTop = this.previous?.rulerData[i]?.groupOffset === current.groupOffset;
            const connectedBot = this.next?.rulerData[i]?.groupOffset === current.groupOffset;
            current.element.classList.remove("ruler-unconnected", "ruler-unconnected-top", "ruler-unconnected-bottom");
            if (!connectedBot && !connectedTop) {
                current.element.classList.add("ruler-unconnected");
            } else if (!connectedBot && connectedTop) {
                current.element.classList.add("ruler-unconnected-bot");
            } else if (connectedBot && !connectedTop
                && !(i === this.rulerData.length - 1 && this.logData.logType === LogType.OpenGroup) //not an open group log.
            ) {
                current.element.classList.add("ruler-unconnected-top");
            }
        }
        if (propagate) {
            this.previous?.updateRulers(false);
            this.next?.updateRulers(false);
        }
    }
}

customElements.define('log-entry', LogEntryElement);
