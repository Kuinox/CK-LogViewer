import { LogEntry } from "../../backend/LogEntry";
import { LogGroupElement } from "./LogGroupElement";

export class GroupList extends HTMLElement {
    public containLazyInitChild = false;
    constructor(logs: LogEntry[], filename: string) {
        super();
        this.append(...logs.map(a => {
            const ret = LogGroupElement.fromLogEntry(a, filename);
            if (ret.isGroup && (ret as LogGroupElement).serverOmittedData) {
                this.containLazyInitChild = true;
            }
            return ret;
        }
        ));
    }
}

customElements.define("group-list", GroupList);
