import { getGroupLogs } from "../../backend/api";
import { GroupLog } from "../../backend/GroupLog";
import { LogEntry } from "../../backend/LogEntry";
import { LogLevel, logLevelToString } from "../../backend/LogLevel";
import { createDiv, setChildOf } from "../../helpers/domHelpers";
import { LoadingIcon } from "../LoadingIcon";
import { LogEntryElement } from "../LogEntryElement";
import { GroupList } from "./GroupList";
import { GroupSummary } from "./GroupSummary";

export class LogGroupElement extends HTMLElement {
    private contentDiv: HTMLElement;
    private contentDivChild: HTMLElement | undefined;
    private groupLog: GroupLog;
    public isGroup = true;
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
        this.groupLog.isFolded = log.isFolded || this.serverOmittedData;
        this.displayExpand();
    }

    get serverOmittedData(): boolean {
        let logCount = 0;
        for (const key in this.groupLog.stats) {
            if (Object.prototype.hasOwnProperty.call(this.groupLog.stats, key)) {
                const element = this.groupLog.stats[key];
                if (element === undefined) continue;
                logCount += element;
            }
        }
        return this.groupLog.groupLogs.length == 0 && logCount > 0;
    }


    private toggleExpand = (): void => {
        this.groupLog.isFolded = !this.groupLog.isFolded;
        this.displayExpand();
    };

    private displayExpand(): void {
        let newChild = undefined;
        if (this.groupLog.isFolded) {
            newChild = new GroupSummary(this.groupLog.stats, this.toggleExpand);
        } else {
            if (this.serverOmittedData) {
                newChild = new LoadingIcon();
            } else {
                newChild = new GroupList(this.groupLog.groupLogs, this.filename);
                if (newChild.containLazyInitChild) {
                    getGroupLogs(this.filename, this.groupLog.openLog.offset).then(
                        groupLogs => {
                            const oldGroup = this.groupLog;
                            this.groupLog = groupLogs;
                            LogGroupElement.setFolded(this.groupLog, oldGroup);
                            this.displayExpand();
                        }
                    );
                }
            }
        }
        this.contentDivChild = setChildOf(this.contentDiv, newChild, this.contentDivChild);
    }

    private static setFolded(newGroup: GroupLog, oldGroup: GroupLog | undefined) {
        for (let i = 0; i < newGroup.groupLogs.length; i++) {
            const old = oldGroup?.groupLogs[i];
            const newE = newGroup.groupLogs[i];
            if (newE.isGroup) {
                if (old === undefined) {
                    newE.isFolded = true;
                } else {
                    if (!old.isGroup) throw new Error("Log data has changed !");
                    newE.isFolded = old.isFolded;
                }
                LogGroupElement.setFolded(newE, old);
            }
        }
    }
}

customElements.define('log-group', LogGroupElement);
