import { LogEntry } from "../../../backend/LogEntry";
import { LogLevel, logLevelToString } from "../../../backend/LogLevel";

export class LogMetadata extends HTMLElement {
    constructor(logEntry: LogEntry) {
        super();
        const logTime = document.createElement("span");
        logTime.classList.add(logLevelToString.get(logEntry.logLevel & LogLevel.Mask)!);
        logTime.innerText = logEntry.logTime;
        this.append(logTime);
        //TODO: display traits, and other infos.
    }
}

// if (log.tags != null) {

//     const toolTip = document.createElement("span");
//     toolTip.classList.add("tags-tooltip");
//     toolTip.classList.add("small-badge");
//     toolTip.innerHTML = log.tags.split(" ").length.toString() + " tags";

//     const toolTipContent = document.createElement("span");
//     toolTipContent.classList.add("tags-tooltip-content");
//     toolTipContent.innerHTML = log.tags;
//     leftContent.appendChild(toolTip);
//     leftContent.appendChild(toolTipContent);
// }

customElements.define('log-metadata', LogMetadata);
