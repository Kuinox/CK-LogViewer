import { LogEntry } from "../../../backend/LogEntry";
import { LogEntryElement } from "../LogEntryElement";
import { LogGroupElement } from "./LogGroupElement";

export class GroupList extends HTMLElement {
    public containLazyInitChild = false;
    public childs: (LogGroupElement | LogEntryElement)[];
    constructor(logs: LogEntry[], filename: string) {
        super();
        this.childs = logs.map(a => {
            const ret = LogGroupElement.fromLogEntry(a, filename);
            if (ret.isGroup && (ret as LogGroupElement).serverOmittedData) {
                this.containLazyInitChild = true;
            }
            return ret;
        });
        this.append(...this.childs);
    }
}

customElements.define("group-list", GroupList);
