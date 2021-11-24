import { ILogGroup } from "../../backend/ILogGroup";
import { LogLevel } from "../../backend/LogLevel";
import { ColorGenerator } from "../../helpers/colorGenerator";
import { CssClassManager } from "./CssClassManager";
import { LogLineBaseElement, LogViewerState, OnClickRulerCallback } from "./LogLineBaseElement";

export class GroupSummary extends LogLineBaseElement {
    private onBadgeClick: (sender: GroupSummary) => void;
    constructor(
        group: ILogGroup,
        cssClassManager: CssClassManager,
        colorGenerator: ColorGenerator,
        logviewerState: LogViewerState,
        onRulerClick: OnClickRulerCallback,
        onClick: (sender: GroupSummary) => void) {
        super(group, cssClassManager, colorGenerator, logviewerState, onRulerClick);
        this.onBadgeClick = onClick;
        this.append(...
            Object.keys(LogLevel)
                .filter(key => isNaN(Number(key))) //filter log level names only.
                .reverse() // reverse, so it start with the highest log level.
                .filter((key: any) => group.stats[key] !== undefined)
                .map((s: string & any) => GroupSummary.createBadge(s, group.stats[s]!))
        );
    }

    static _constructor = (function () {
        document.addEventListener("click", GroupSummary.badgeClicked);
    }());

    static badgeClicked(ev: MouseEvent): void {
        if (ev.target instanceof GroupSummary) {
            ev.target.onBadgeClick( ev.target);
        } else if (ev.target instanceof HTMLElement &&  ev.target.parentElement instanceof GroupSummary) {
                ev.target.parentElement.onBadgeClick(ev.target.parentElement);
        }
    }
    private static createBadge(level: string, qty: number): HTMLElement {
        const span = document.createElement("span");
        span.classList.add("badge");
        span.classList.add(level.toLowerCase());
        span.innerHTML = qty + " " + level;
        return span;
    }
}

customElements.define("group-summary", GroupSummary);
