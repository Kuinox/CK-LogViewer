import { ILogEntry } from "../../backend/ILogEntry";
import { LogLevel, logLevelToString } from "../../backend/LogLevel";
import { LogType } from "../../backend/LogType";
import { CssClassManager } from "./CssClassManager";

const groupTabClassName = "group-tab";
const rulerGroupClassNamePart = "ruler-group";
const monitorClass = "monitor";
export type OnClickRulerCallback = (groupOffset: number) => void;

export class LogLineBaseElement extends HTMLElement {
    public logData: ILogEntry;
    private cssClassManager: CssClassManager;
    public constructor(
        log: ILogEntry,
        cssClassManager: CssClassManager,
        onRulerClick: OnClickRulerCallback
    ) {
        super();
        this.logData = log;
        this.cssClassManager = cssClassManager;
        this.classList.add(this.monitorClass);
        const metadataContainer = document.createElement("div");
        this.append(metadataContainer);
        const date = document.createElement("span");
        date.classList.add("metadata-date");
        date.innerHTML = log.logTime.padEnd(32, " ");
        metadataContainer.append(date);
        const slider = document.createElement("div");
        slider.classList.add("slider");
        slider.addEventListener("mousedown", this.onSliderMouseDown);
        this.setMargin(10);
        this.append(slider);
        for (let i = 0; i < log.parentsLogLevel.length - 1; i++) {
            this.appendRuler(log.parentsLogLevel[i].logLevel, log.parentsLogLevel[i].groupOffset, onRulerClick, undefined);
        }
        if (log.parentsLogLevel.length >= 1) {
            if (log.logType === LogType.CloseGroup) {
                this.appendRuler(log.parentsLogLevel[log.parentsLogLevel.length - 1].logLevel, log.parentsLogLevel[log.parentsLogLevel.length - 1].groupOffset, onRulerClick, "ruler-close");
            } else {
                this.appendRuler(log.parentsLogLevel[log.parentsLogLevel.length - 1].logLevel, log.parentsLogLevel[log.parentsLogLevel.length - 1].groupOffset, onRulerClick, undefined);
            }
        }
        if (log.logType === LogType.OpenGroup) {
            this.appendRuler(log.logLevel, log.offset, onRulerClick, "ruler-open");
        }
    }

    initialMouseX = 0;
    initialMargin = 0;
    onSliderMouseDown = (ev: MouseEvent): void => {
        document.addEventListener("mousemove", this.onSliderMove);
        document.addEventListener("mouseup", this.onMouseUp);
        this.initialMouseX = ev.x;
        this.initialMargin = this.currentMargin;
    };

    onMouseUp = (): void => {
        document.removeEventListener("mouseup", this.onMouseUp);
        document.removeEventListener("mousemove", this.onSliderMove);
    };

    onSliderMove = (ev: MouseEvent): void => {
        const diff = ev.x - this.initialMouseX;
        const newPos = this.initialMargin + diff;
        this.setMargin(newPos);
    };
    private get sliderClassName(): string {
        return  "slider" + this.monitorClass;
    }
    currentMargin = 0;
    setMargin(margin: number): void {
        this.currentMargin = margin;
        this.cssClassManager.requireClass(this.sliderClassName, `.${this.monitorClass} .slider {
            margin-left: ${margin}px;
        }` );
    }

    public static runOnGroup(groupOffset: number, delegate: (entry: LogLineBaseElement) => void): void {
        Array.prototype.slice.call<HTMLCollectionOf<Element>, [], Element[]>(
            document.getElementsByClassName(LogLineBaseElement.getRulerClassByOffset(groupOffset))
        ).map<LogLineBaseElement>(s => s.parentElement as LogLineBaseElement).forEach(delegate);
    }

    private get monitorClass() {
        return monitorClass + this.logData.monitorId;
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
        onRulerClick: OnClickRulerCallback,
        classToApply: string | undefined
    ) {
        const tabContainer = document.createElement("div");
        const ruleName = LogLineBaseElement.getRulerClassByOffset(groupOffset);
        tabContainer.classList.add(groupTabClassName, ruleName);
        this.append(tabContainer);
        const rule = "." + ruleName + "{background-color: rgba(255, 255, 255, 0.1);}";
        tabContainer.onmouseenter = () => this.cssClassManager.requireClass(ruleName, rule);
        tabContainer.onmouseleave = () => this.cssClassManager.releaseClass(ruleName);
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
