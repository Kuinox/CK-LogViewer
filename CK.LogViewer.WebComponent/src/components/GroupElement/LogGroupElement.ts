import { GroupLog } from "../../backend/GroupLog";
import { GroupStats } from "../../backend/GroupStats";
import { LogEntry } from "../../backend/LogEntry";
import { LogLevel, logLevelToString } from "../../backend/LogLevel";
import { createDiv, setChildOf } from "../../helpers/domHelpers";
import { LoadingIcon } from "../LoadingIcon";
import { LogEntryElement } from "../LogEntryElement";
import { GroupList } from "./GroupList";
import { GroupSummary } from "./GroupSummary";

export class LogGroupElement extends HTMLElement {
    private folded: boolean;
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
                    classList: ["group-ruler", logLevelToString.get(log.openLog.logLevel & LogLevel.Mask)!]
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
        this.folded = log.isFolded || this.serverOmittedData;
        this.displayExpand();
    }

    private toggleExpand = (): void => {
        this.folded = !this.folded;
        this.displayExpand();
    };

    private displayExpand(): void {
        let newChild = undefined;
        if (this.folded) {
            newChild = new GroupSummary(this.groupStats, this.toggleExpand);
        } else {
            if (this.serverOmittedData) {
                newChild = new LoadingIcon();
            } else {
                newChild = new GroupList(this.logs);
            }
        }
        this.contentDivChild = setChildOf(this.contentDiv, newChild, this.contentDivChild);
    }
}

customElements.define('log-group', LogGroupElement);
