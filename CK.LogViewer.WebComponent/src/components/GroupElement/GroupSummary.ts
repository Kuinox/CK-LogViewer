import { GroupStats } from "../../backend/GroupStats";
import { LogLevel } from "../../backend/LogLevel";

export class GroupSummary extends HTMLElement { //TODO: @Hugo I was lazy, I let you fix this tricky typing :D.
    constructor(stats: GroupStats) {
        super();
        this.append(...
            Object.keys(LogLevel)
                .filter(key => isNaN(Number(key))) //filter log level names only.
                .reverse() // reverse, so it start with the highest log level.
                .filter((key: any) => stats[key] !== undefined)
                .map((s: string & any) => GroupSummary.createBadge(s, stats[s]!))
        );
    }



    private static createBadge(level: string, qty: number): HTMLElement {
        const span = document.createElement("span");
        span.innerHTML = level+": "+qty+", ";
        return span;
    }
}

customElements.define("group-summary", GroupSummary);
