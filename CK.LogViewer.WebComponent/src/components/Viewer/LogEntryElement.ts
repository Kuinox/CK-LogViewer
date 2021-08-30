import { LogLevel, logLevelToString } from "../../backend/LogLevel";
import { LogEntry, LogType } from "../../backend/LogEntry";
import { LogExceptionElement } from "./LogExceptionElement";

export class LogEntryElement extends HTMLElement {
    private rulerData: {
        logLevel: LogLevel;
        groupOffset: number;
        rulerIndex?: number;
    }[];
    private previous: LogEntryElement | undefined;
    private rulers: { element: HTMLDivElement, offset: number }[] = [];
    private logContent: HTMLDivElement;
    private next: LogEntryElement | undefined;
    private logData: LogEntry;
    constructor(elementToAppend: HTMLElement, log: LogEntry, previous: LogEntryElement | undefined) {
        super();
        this.logData = log;
        elementToAppend.append(this);
        this.rulerData = log.parentsLogLevel;
        if (log.logType === LogType.OpenGroup) {
            this.rulerData.push({
                logLevel: log.logLevel,
                groupOffset: log.offset
            });
        }
        this.previous = previous;
        if (this.previous !== undefined) {
            this.previous.next = this;
        }
        const groupParentCount = log.parentsLogLevel.length;
        this.logContent = document.createElement("div");
        this.logContent.classList.add("log-content");
        this.logContent.style.setProperty("--spacing", groupParentCount.toString());

        this.prepend(this.logContent);
        const span = document.createElement("span");
        span.className = "log-text";
        span.innerHTML = log.text;
        this.logContent.prepend(span);
        if (log.exception != null) {
            this.logContent.prepend(new LogExceptionElement(log.exception));
        }
        const logLevel = logLevelToString.get(log.logLevel & LogLevel.Mask);
        if (logLevel === undefined) throw Error("Invalid log level.");
        this.classList.add(logLevel);
        if (groupParentCount > 0) {
            for (let i = 0; i < groupParentCount; i++) {
                const groupOffset = this.rulerData[i].groupOffset;
                const previousOffsetIndex = previous?.rulerData.map(s => s.groupOffset).indexOf(groupOffset) ?? -1;
                const parentRuler = previous?.rulerData[previousOffsetIndex];
                const logLevelStr = logLevelToString.get(log.parentsLogLevel[i].logLevel & LogLevel.Mask);
                if (logLevelStr === undefined) throw new Error("Invalid Data: Unknown Log Level.");
                if (parentRuler === undefined || parentRuler.groupOffset !== log.parentsLogLevel[i]?.groupOffset) {
                    this.appendRuler(logLevelStr, i, groupOffset);
                } else {
                    const divRect = this.getBoundingClientRect();
                    this.growRuler(groupOffset, divRect.height);
                }
            }
        }

    }

    toggle(element: HTMLElement): void {
        if (element.style.display == "none") {
            element.style.display = "flex";
        } else {
            element.style.display == "none";
        }
    }

    private collapseLogs(ruler: HTMLDivElement, primary: boolean): any {
        const thingToEdit = primary ? this.logContent : this;
        this.toggle(thingToEdit);
        if (thingToEdit.style.display == "none") {
            thingToEdit.style.display = "flex";

        } else {
            thingToEdit.style.display = "none";
        }
        if (!primary) {
            for (let i = 0; i < this.rulers.length; i++) {
                this.toggle(this.rulers[i].element);
            }
            return;
        }
        const offset = this.rulers.filter(s => s.element === ruler)[0].offset;
        let current = this.next;
        while (current !== undefined) {
            if (current.logData.parentsLogLevel.filter(s => s.groupOffset == offset)[0] === undefined) {
                return;
            }
            current.collapseLogs(ruler, false);
            current = current.next;
        }
    }

    private appendRuler(logLevelStr: string, rulerIndex: number, groupOffset: number) {
        const tabContainer = document.createElement("div");
        tabContainer.classList.add("group-tab");
        tabContainer.style.setProperty("--margin-spacing", rulerIndex.toString());
        this.prepend(tabContainer);
        const tab = document.createElement("div");
        tab.classList.add(logLevelStr);
        tab.style.height = 0 + "px";
        tabContainer.appendChild(tab);
        this.rulers.push(
            {
                element: tab,
                offset: groupOffset
            });
        tabContainer.onclick = () => this.collapseLogs(tab, true);
    }

    private growRuler(targetedGroupOffset: number, pixelCount: number) {
        const previousIndex = this.previous?.rulerData.map(s => s.groupOffset).indexOf(targetedGroupOffset) ?? -1;
        if (previousIndex === -1) {
            this.doGrowRuler(targetedGroupOffset, pixelCount);
            return;
        }
        const previousGroupOffset = this.previous?.rulerData[previousIndex]?.groupOffset;
        if (previousGroupOffset === undefined) throw new RangeError("Range error. You found a bug.");
        if (previousGroupOffset === undefined || targetedGroupOffset != previousGroupOffset) {
            this.doGrowRuler(targetedGroupOffset, pixelCount);
        } else {
            this.previous?.growRuler(targetedGroupOffset, pixelCount);
        }
    }

    private doGrowRuler(groupOffset: number, pixelCount: number) {
        const ruler = this.rulers.filter(s => s.offset == groupOffset)[0].element;
        const heightString = ruler.style.height;
        const height = Number.parseFloat(heightString.slice(0, heightString.indexOf("px")));
        ruler.style.height = (height + pixelCount) + "px";
    }
}

customElements.define('log-entry', LogEntryElement);

