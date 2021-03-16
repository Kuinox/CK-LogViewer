import { GroupLog } from "../../backend/GroupLog";
import { GroupStats } from "../../backend/GroupStats";
import { LogEntry } from "../../backend/LogEntry";
import { createDiv, setChild as setChildOf } from "../../helpers/domHelpers";
import { LogEntryElement } from "../LogEntryElement";
import { GroupList } from "./GroupList";
import { GroupSummary } from "./GroupSummary";

export class LogGroupElement extends HTMLElement {
    private collapsed: boolean;
    private contentDiv: HTMLElement;
    private contentDivChild: HTMLElement | undefined;
    private serverOmittedData: boolean;
    private groupStats: GroupStats;
    private logs: LogEntry[];
    static fromLogEntry(log: LogEntry): LogEntryElement | LogGroupElement {
        return log.isGroup ? new LogGroupElement(log) : LogEntryElement.create(log);
    }

    constructor(log: GroupLog) {
        super();
        this.groupStats = log.stats;
        this.logs = log.groupLogs;
        this.contentDiv = createDiv();
        const collapseDiv = createDiv({
            className: "group-tab",
            childNodes: [
                createDiv(),
                createDiv({
                    className: "group-ruler"
                })
            ],
            onClick: this.toggleExpand
        });

        this.appendChild(collapseDiv);
        this.appendChild(createDiv({
            childNodes: [
                LogEntryElement.create(log.openLog, {
                    className: "open-log"
                }),
                this.contentDiv,
                LogEntryElement.create(log.closeLog, {
                    className: "close-log"
                })
            ]
        }));
        let logCount = 0;
        for (const key in log.stats) {
            if (Object.prototype.hasOwnProperty.call(log.stats, key)) {
                const element = log.stats[key];
                if (element === undefined) continue;
                logCount += element;
            }
        }
        this.serverOmittedData = log.groupLogs.length == 0 && logCount > 0;
        this.collapsed = this.serverOmittedData;
        this.displayExpand();
    }

    private toggleExpand = (): void => {
        this.collapsed = !this.collapsed;
        this.displayExpand();
    };

    private displayExpand(): void {
        const newChild = this.collapsed ? new GroupSummary(this.groupStats) : new GroupList(this.logs);
        this.contentDivChild = setChildOf(this.contentDiv, newChild, this.contentDivChild);
    }
}

customElements.define('log-group', LogGroupElement);
