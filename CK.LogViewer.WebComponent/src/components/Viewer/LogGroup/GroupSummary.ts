// import { GroupStats } from "../../../backend/GroupStats";
// import { LogLevel } from "../../../backend/LogLevel";

// export class GroupSummary extends HTMLElement { //TODO: @Hugo I was lazy, I let you fix this tricky typing :D.
//     constructor(stats: GroupStats, onClick: () => void) {
//         super();
//         this.append(...
//             Object.keys(LogLevel)
//                 .filter(key => isNaN(Number(key))) //filter log level names only.
//                 .reverse() // reverse, so it start with the highest log level.
//                 .filter((key: any) => stats[key] !== undefined)
//                 .map((s: string & any) => GroupSummary.createBadge(s, stats[s]!))
//         );
//         this.addEventListener("click", onClick);
//     }

//     private static createBadge(level: string, qty: number): HTMLElement {
//         const span = document.createElement("span");
//         span.classList.add("badge");
//         span.classList.add(level.toLowerCase());
//         span.innerHTML = qty + " " + level;
//         return span;
//     }
// }

// customElements.define("group-summary", GroupSummary);
