import { LogEntry } from "../../../backend/LogEntry";
import { LogEntryElement } from "../LogEntryElement";

export class LogZoneElement extends HTMLElement {
    previousLog: LogEntry | undefined;
    previousElement: LogEntryElement | undefined;
    appendLog(log: LogEntry): void {
        const element = new LogEntryElement(this, log, this.previousElement);
        this.previousLog = log;
        this.previousElement = element;
    }
}

customElements.define('log-zone-elements', LogZoneElement);
