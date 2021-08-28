import { LogEntry } from "../../../backend/LogEntry";
import { LogEntryElement } from "../LogEntryElement";

export class LogZoneElement extends HTMLElement {
    previousLog: LogEntry | undefined;
    appendLog(log: LogEntry):void {
        this.appendChild(new LogEntryElement(log));
        this.previousLog = log;
    }
}

customElements.define('log-zone-elements', LogZoneElement);
