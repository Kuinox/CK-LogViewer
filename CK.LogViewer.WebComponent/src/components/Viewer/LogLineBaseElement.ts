import { ILogEntry } from "../../backend/ILogEntry";
import { LogLevel, logLevelToString } from "../../backend/LogLevel";
import { LogType } from "../../backend/LogType";
import { CssClassManager } from "./CssClassManager";
import { ColorGenerator } from "../../helpers/colorGenerator";

const groupTabClassName = "group-tab";
const rulerGroupClassNamePart = "ruler-group";
const monitorClass = "monitor";
export type OnClickRulerCallback = (groupOffset: number) => void;
const minMargin = 10;

export class LogViewerState {
    rulersOffset: {
        [key: string]: number
    } = {};

    rulersColors: {
        [key: string]: string
    } = {};
}

export class LogLineBaseElement extends HTMLElement {
    static _constructor = (function () {
        document.addEventListener("click", LogLineBaseElement.rulerClicked);
    }());
    static rulerClicked(ev: MouseEvent): void {
        const ruler = LogLineBaseElement.tryGetRulerFromDocumentEvent(ev);
        if (ruler === undefined) return;
        const rulerOffset = LogLineBaseElement.getOffsetFromRuler(ruler);
        (ruler.parentElement as LogLineBaseElement).onRulerClick(rulerOffset);
        ev.preventDefault();
    }

    public logData: ILogEntry;
    private colorGenerator: ColorGenerator;
    private cssClassManager: CssClassManager;
    private logviewerState: LogViewerState;
    private onRulerClick: OnClickRulerCallback;
    public constructor(
        log: ILogEntry,
        cssClassManager: CssClassManager,
        colorGenerator: ColorGenerator,
        logviewerState: LogViewerState,
        onRulerClick: OnClickRulerCallback
    ) {
        super();
        this.onRulerClick = onRulerClick;
        this.logData = log;
        this.colorGenerator = colorGenerator;
        this.cssClassManager = cssClassManager;
        this.logviewerState = logviewerState;
        this.classList.add(this.monitorClass);
        const date = document.createElement("span");
        date.classList.add("metadata-date");
        date.innerHTML = log.logTime.padEnd(32, " ");
        this.append(date);
        const slider = document.createElement("div");
        slider.classList.add("slider");
        slider.addEventListener("mousedown", this.onSliderMouseDown);
        slider.appendChild(document.createElement("div"));
        this.setMargin(minMargin);
        this.append(slider);
        for (let i = 0; i < log.parentsLogLevel.length - (log.logType == LogType.CloseGroup ? 1 : 0); i++) {
            this.appendRuler(log.parentsLogLevel[i].logLevel, log.parentsLogLevel[i].groupOffset, undefined);
        }
        if (log.logType === LogType.CloseGroup) {
            this.appendRuler(log.logLevel, log.groupOffset, "ruler-close");
        }
        if (log.logType === LogType.OpenGroup) {
            this.appendRuler(log.logLevel, log.offset, "ruler-open");
        }
    }

    initialMouseX = 0;
    initialMargin = 0;
    previousCursor = "";
    onSliderMouseDown = (ev: MouseEvent): void => {
        ev.preventDefault(); // avoid selecting text.
        document.addEventListener("mousemove", this.onSliderMove);
        document.addEventListener("mouseup", this.onMouseUp);
        this.initialMouseX = ev.x;
        const initialValue = this.logviewerState.rulersOffset[this.sliderClassName];
        this.initialMargin = initialValue ?? minMargin;
        this.previousCursor = document.body.style.cursor;
        document.body.style.cursor = "col-resize";
    };

    currentMargin = 0;
    setMargin = (margin: number): void => {
        if (margin < minMargin) margin = minMargin;
        this.logviewerState.rulersOffset[this.sliderClassName] = margin;
        this.currentMargin = margin;
        let rulerColor = this.logviewerState.rulersColors[this.sliderClassName];
        if (rulerColor === undefined) {
            rulerColor = this.colorGenerator.getUniqueColor();
            this.logviewerState.rulersColors[this.sliderClassName] = rulerColor;
        }

        this.cssClassManager.requireClass(this.sliderClassName, `.${this.monitorClass} .slider {
    margin-left: ${margin}px;

}
.${this.monitorClass} .slider div {
    /*border-color: ${rulerColor};*/
    box-shadow: -3px 0px 3px ${rulerColor};
}

        ` );
    };

    onMouseUp = (): void => {
        document.removeEventListener("mouseup", this.onMouseUp);
        document.body.style.cursor = this.previousCursor;
        document.removeEventListener("mousemove", this.onSliderMove);
    };

    onSliderMove = (ev: MouseEvent): void => {
        ev.preventDefault(); // avoid changing cursor.
        const diff = ev.x - this.initialMouseX;
        const newPos = this.initialMargin + diff;
        this.setMargin(newPos);
    };
    private get sliderClassName(): string {
        return "slider" + this.monitorClass;
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
    self = this;
    private appendRuler(
        logLevel: LogLevel,
        groupOffset: number,
        classToApply: string | undefined
    ) {
        const tabContainer = document.createElement("div");
        const ruleName = LogLineBaseElement.getRulerClassByOffset(groupOffset);
        tabContainer.classList.add(groupTabClassName, ruleName);
        this.append(tabContainer);
        const rule = "." + ruleName + "{background-color: rgba(255, 255, 255, 0.1);}";
        tabContainer.onmouseenter = () => this.cssClassManager.requireClass(ruleName, rule);
        tabContainer.onmouseleave = () => this.cssClassManager.releaseClass(ruleName);
        const tab = document.createElement("div");
        const logLevelStr = logLevelToString.get(logLevel & LogLevel.Mask);
        if (logLevelStr === undefined) throw new Error("Invalid Data: Unknown Log Level.");
        tab.classList.add(logLevelStr, "ruler");
        if (classToApply !== undefined) {
            tab.classList.add(classToApply);
        }
        tabContainer.appendChild(tab);
    }



    static tryGetRulerFromDocumentEvent(ev: MouseEvent): HTMLElement | undefined {
        if (!(ev.target instanceof HTMLElement)) return;
        const clickedDiv = ev.target as HTMLElement;
        if (!clickedDiv.className.includes("ruler")) return;
        const isGroup = clickedDiv.className.includes(rulerGroupClassNamePart);
        return isGroup ? clickedDiv : clickedDiv.parentElement!;
    }
    private getRulers() {
        return Array.prototype.slice.call<HTMLCollectionOf<Element>, [], Element[]>(this.getElementsByClassName(groupTabClassName));
    }
}
