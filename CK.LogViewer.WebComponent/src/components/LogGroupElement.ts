import { GroupLog } from "../LogType";
import { LogEntryElement } from "./LogEntryElement";
export class LogGroupElement extends HTMLElement {
    private collapsed: boolean = false;
    private contentDiv: HTMLElement;
    constructor(collapseDiv: HTMLElement, contentDiv: HTMLElement) {
        super();
        collapseDiv.addEventListener("click", this.toggleExpand);
        this.contentDiv = contentDiv;
    }

    toggleExpand = () => {
        this.collapsed = !this.collapsed;
        if (this.collapsed) {
            this.contentDiv.style.setProperty("display", "none");
        } else {
            this.contentDiv.style.removeProperty("display");
        }
    }

    static create(log: GroupLog): HTMLElement {
        const contentDiv = document.createElement("div");
        contentDiv.className = "group-content";

        const open = LogEntryElement.create(log.openLog);
        open.className = "open-log";
        contentDiv.appendChild(open);
        const list = document.createElement("div");
        const collapseDiv = document.createElement("div");
        {
            collapseDiv.className = "group-tab";
            const leftCollapse = document.createElement("div");
            const rightCollapse = document.createElement("div");
            rightCollapse.className = "group-ruler";
            collapseDiv.appendChild(leftCollapse);
            collapseDiv.appendChild(rightCollapse);
        }
        contentDiv.appendChild(list);

        for (let i = 0; i < log.groupLogs.length; i++) {
            const element = log.groupLogs[i];
            list.appendChild(element.isGroup ? this.create(element) : LogEntryElement.create(element));
        }
        const close = LogEntryElement.create(log.openLog);
        close.className = "close-log";
        contentDiv.appendChild(close);
        const group = new LogGroupElement(collapseDiv, list);
        group.append(collapseDiv);
        group.appendChild(contentDiv);
        return group;
    }
}

customElements.define('log-group', LogGroupElement);
