import { GroupLog } from "../backend/GroupLog";
import { LogEntry } from "../backend/LogEntry";
import { SimpleLog } from "../backend/SimpleLog";
import { createDiv } from "../helpers/domHelpers";
import { LogEntryElement } from "./LogEntryElement";

export class LogGroupElement extends HTMLElement {
    private collapsed;
    private contentDiv: HTMLElement;
    constructor(collapseDiv: HTMLElement, contentDiv: HTMLElement, collapsed: boolean) {
        super();
        this.collapsed = collapsed;
        collapseDiv.addEventListener("click", this.toggleExpand);
        this.contentDiv = contentDiv;
    }

    toggleExpand = (): void => {
        this.collapsed = !this.collapsed;
        if (this.collapsed) {
            this.contentDiv.style.setProperty("display", "none");
        } else {
            this.contentDiv.style.removeProperty("display");
        }
    };
    private static fromLogEntry(log: LogEntry) {
        return log.isGroup ? this.create(log) : LogEntryElement.create(log);
    }

    static create(log: GroupLog): HTMLElement {
        const list = createDiv({
            childNodes: log.groupLogs.map(this.fromLogEntry)
        });
        const contentDiv = createDiv({
            className: "group-content",
            childNodes: [LogEntryElement.create(log.openLog, {
                className: "open-log"
            }),
                list,
            LogEntryElement.create(log.closeLog, {
                className: "close-log"
            })
            ]
        });
        const collapseDiv = createDiv({
            className: "group-tab",
            childNodes: [
                createDiv(),
                createDiv({
                    className: "group-ruler"
                })
            ]
        });
        let logCount = 0;
        for (const key in log.groupStats) {
            if (Object.prototype.hasOwnProperty.call(log.groupStats, key)) {
                const element = log.groupStats[key];
                if(element === undefined) continue;
                logCount += element;
            }
        }

        const isCollapsed = logCount > 0 && log.groupLogs.length === 0;
        const group = new LogGroupElement(collapseDiv, list, isCollapsed);
        group.append(collapseDiv);
        group.appendChild(contentDiv);
        return group;
    }
}

customElements.define('log-group', LogGroupElement);
