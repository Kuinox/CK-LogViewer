import { LogEntry } from "../../backend/LogEntry";
import { LogGroupElement } from "./LogGroupElement";

export class GroupList extends HTMLElement {
    constructor(logs: LogEntry[]) {
        super();
        this.append(...logs.map(LogGroupElement.fromLogEntry));
    }
}

customElements.define("group-list", GroupList);
