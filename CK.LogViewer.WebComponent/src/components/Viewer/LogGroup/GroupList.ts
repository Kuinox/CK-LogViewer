import { Api } from "../../../backend/api";
import { LogEntry } from "../../../backend/LogEntry";
import { LogEntryElement } from "../LogEntryElement";
import { LogGroupElement } from "./LogGroupElement";

export class GroupList extends HTMLElement {
    public containLazyInitChild = false;
    public childs: (LogGroupElement | LogEntryElement)[];
    constructor(logs: LogEntry[], api: Api) {
        super();
        this.childs = logs.map(a => {
            const ret = LogGroupElement.fromLogEntry(a, api);
            if (ret.isGroup && (ret as LogGroupElement).serverOmittedData) {
                this.containLazyInitChild = true;
            }
            return ret;
        });
        this.append(...this.childs);
    }
}

customElements.define("group-list", GroupList);
