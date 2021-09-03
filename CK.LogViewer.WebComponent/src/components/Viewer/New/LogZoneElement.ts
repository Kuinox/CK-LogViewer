import { LogEntry } from "../../../backend/LogEntry";
import { CssClassManager } from "../CssClassManager";
import { LogEntryElement } from "../LogEntryElement";

export class LogZoneElement extends HTMLElement {
    previousLog: LogEntry | undefined;
    previousElement: LogEntryElement | undefined;
    appendLog(log: LogEntry, cssClassManager : CssClassManager, onRulerClick: (entry: LogEntryElement, groupOffset: number) => void): void {
        const element = new LogEntryElement(log, this.previousElement, cssClassManager, onRulerClick);
        this.previousLog = log;
        this.previousElement = element;
        this.appendChild(element);
    }
}

customElements.define('log-zone-elements', LogZoneElement);
