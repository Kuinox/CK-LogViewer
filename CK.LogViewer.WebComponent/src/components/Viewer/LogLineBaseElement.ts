import { ILogEntry } from "../../backend/ILogEntry";
import { LogLevel, logLevelToString } from "../../backend/LogLevel";
import { LogType } from "../../backend/LogType";
import { CssClassManager } from "./CssClassManager";

const groupTabClassName = "group-tab";
const rulerGroupClassNamePart = "ruler-group";
const monitorClass = "monitor-";
export type OnClickRulerCallback = (groupOffset: number) => void;

export class LogLineBaseElement extends HTMLElement {
    public logData: ILogEntry;

    public constructor(
        log: ILogEntry,
        cssClassManager: CssClassManager,
        onRulerClick: OnClickRulerCallback
    ) {
        super();
        this.classList.add(LogLineBaseElement.getMonitorClass(log.monitorId));
        const metadataContainer = document.createElement("div");
        this.append(metadataContainer);
        const date = document.createElement("span");
        date.classList.add("metadata-date");
        date.innerHTML = log.logTime.padEnd(32, " ");
        metadataContainer.append(date);
        this.logData = log;
        for (let i = 0; i < log.parentsLogLevel.length - 1; i++) {
            this.appendRuler(log.parentsLogLevel[i].logLevel, log.parentsLogLevel[i].groupOffset, cssClassManager, onRulerClick, undefined);
        }
        if (log.parentsLogLevel.length >= 1) {
            if (log.logType === LogType.CloseGroup) {
                this.appendRuler(log.parentsLogLevel[log.parentsLogLevel.length - 1].logLevel, log.parentsLogLevel[log.parentsLogLevel.length - 1].groupOffset, cssClassManager, onRulerClick, "ruler-close");
            } else {
                this.appendRuler(log.parentsLogLevel[log.parentsLogLevel.length - 1].logLevel, log.parentsLogLevel[log.parentsLogLevel.length - 1].groupOffset, cssClassManager, onRulerClick, undefined);
            }
        }
        if (log.logType === LogType.OpenGroup) {
            this.appendRuler(log.logLevel, log.offset, cssClassManager, onRulerClick, "ruler-open");
        }
    }

    public static runOnGroup(groupOffset: number, delegate: (entry: LogLineBaseElement) => void): void {
        Array.prototype.slice.call<HTMLCollectionOf<Element>, [], Element[]>(
            document.getElementsByClassName(LogLineBaseElement.getRulerClassByOffset(groupOffset))
        ).map<LogLineBaseElement>(s => s.parentElement as LogLineBaseElement).forEach(delegate);
    }

    private static getMonitorClass(monitorId: string) {
        return monitorClass + monitorId;
    }

    private static getRulerClassByOffset(groupOffset: number) {
        return rulerGroupClassNamePart + groupOffset;
    }
    private static getOffsetByRulerClass(rulerClass: string) {
        return Number.parseInt(rulerClass.slice(rulerGroupClassNamePart.length));
    }

    private static isOffsetClass(className: string) {
        return className.startsWith(rulerGroupClassNamePart);
    }

    private static getOffsetFromRuler(ruler: Element) {
        let className: string;
        ruler.classList.forEach(s => {
            if (this.isOffsetClass(s)) {
                className = s;
            }
        });
        return this.getOffsetByRulerClass(className!);
    }

    private appendRuler(
        logLevel: LogLevel,
        groupOffset: number,
        cssClassManager: CssClassManager,
        onRulerClick: OnClickRulerCallback,
        classToApply: string | undefined
    ) {
        const tabContainer = document.createElement("div");
        const ruleName = LogLineBaseElement.getRulerClassByOffset(groupOffset);
        tabContainer.classList.add(groupTabClassName, ruleName);
        this.append(tabContainer);
        const rule = "." + ruleName + "{background-color: rgba(255, 255, 255, 0.1);}";
        tabContainer.onmouseenter = () => cssClassManager.requireClass(ruleName, rule);
        tabContainer.onmouseleave = () => cssClassManager.releaseClass(ruleName);
        tabContainer.onclick = () => onRulerClick(groupOffset);
        const tab = document.createElement("div");
        const logLevelStr = logLevelToString.get(logLevel & LogLevel.Mask);
        if (logLevelStr === undefined) throw new Error("Invalid Data: Unknown Log Level.");
        tab.classList.add(logLevelStr, "ruler-unconnected");
        if (classToApply !== undefined) {
            tab.classList.add(classToApply);
        }
        tabContainer.appendChild(tab);
    }
    private getRulers() {
        return Array.prototype.slice.call<HTMLCollectionOf<Element>, [], Element[]>(this.getElementsByClassName(groupTabClassName));
    }
}
