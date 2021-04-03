import { getGroupLogs } from "../../../backend/api";
import { GroupLog } from "../../../backend/GroupLog";
import { LogEntry } from "../../../backend/LogEntry";
import { LogLevel, logLevelToString } from "../../../backend/LogLevel";
import { createDiv, setChildOf } from "../../../helpers/domHelpers";
import { LoadingIcon } from "../../Common/LoadingIcon";
import { LogEntryElement } from "../LogEntryElement";
import { GroupList } from "./GroupList";
import { GroupSummary } from "./GroupSummary";

export class LogGroupElement extends HTMLElement {
    private contentDiv: HTMLElement;
    private contentDivChild: HTMLElement | undefined;
    private groupLog: GroupLog;
    public readonly isGroup = true;
    public isFolded: boolean;
    private filename;
    static fromLogEntry(log: LogEntry, filename: string): LogEntryElement | LogGroupElement {
        return log.isGroup ? new LogGroupElement(log, filename) : new LogEntryElement(log);
    }

    constructor(log: GroupLog, filename: string) {
        super();
        this.filename = filename;
        this.groupLog = log;
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
                new LogEntryElement(log.openLog, {
                    className: "open-log"
                }),
                this.contentDiv,
                new LogEntryElement(log.closeLog, {
                    className: "close-log"
                })
            ]
        }));
        this.isFolded = this.serverOmittedData || this.directChildHaveDataOmitted;
        this.displayExpand();
    }

    get directChildHaveDataOmitted(): boolean {
        return this.groupLog.groupLogs.some(s => s.isGroup && LogGroupElement.didServerOmittedData(s));
    }

    static didServerOmittedData(groupLog: GroupLog): boolean {
        let logCount = 0;
        for (const key in groupLog.stats) {
            if (Object.prototype.hasOwnProperty.call(groupLog.stats, key)) {
                const element = groupLog.stats[key];
                if (element === undefined) continue;
                logCount += element;
            }
        }
        return groupLog.groupLogs.length == 0 && logCount > 0;
    }

    get serverOmittedData(): boolean {
        return LogGroupElement.didServerOmittedData(this.groupLog);
    }


    private toggleExpand = (): void => {
        this.isFolded = !this.isFolded;
        this.displayExpand();
    };

    private async displayExpand(): Promise<void> {
        if (this.isFolded) {
            this.contentDivChild = setChildOf(this.contentDiv, new GroupSummary(this.groupLog.stats, this.toggleExpand), this.contentDivChild);
        } else {
            if (this.serverOmittedData) {
                this.contentDivChild = setChildOf(this.contentDiv, new LoadingIcon(), this.contentDivChild);
            } else {
                const newChild = new GroupList(this.groupLog.groupLogs, this.filename);
                this.contentDivChild = setChildOf(this.contentDiv, newChild, this.contentDivChild);
                if (newChild.containLazyInitChild) {
                    const newLogs = await getGroupLogs(this.filename, this.groupLog.openLog.id);
                    if (newLogs.length != newChild.childs.length) throw new Error("Invalid data");
                    for (let i = 0; i < newLogs.length; i++) {
                        const update = newLogs[i];
                        const old = newChild.childs[i];
                        if (update === undefined || old === undefined) throw new Error("bug.");
                        if (update.isGroup != old.isGroup) throw new Error("bug.");
                        if (old.isGroup && update.isGroup) {
                            old.updateContent(update.groupLogs);
                        }
                    }
                }
            }
        }
    }

    public updateContent(logs: LogEntry[]): void {
        this.groupLog.groupLogs = logs;
        this.displayExpand();
    }
}

customElements.define('log-group', LogGroupElement);
