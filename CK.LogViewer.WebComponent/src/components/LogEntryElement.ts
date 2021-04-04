import { LogLevel, logLevelToString } from "../backend/LogLevel";
import { SimpleLog } from "../backend/SimpleLog";
import { setElementOptions, ClassOptions } from "../helpers/domHelpers";
import { LogExceptionElement } from "./LogExceptionElement";

export class LogEntryElement extends HTMLElement {
    public readonly isGroup = false;

    constructor(log: SimpleLog, options?: ClassOptions) {
        super();
        setElementOptions(this, options);
        const logLevel = logLevelToString.get(log.logLevel & LogLevel.Mask);
        if (logLevel === undefined) throw Error("Invalid log level.");
        const leftContent = document.createElement("div");
        leftContent.classList.add("left-content");

        const time = document.createElement("span");
        time.innerHTML = log.logTime;
        leftContent.appendChild(time);

        const monitor = document.createElement("span");
        monitor.classList.add("monitorId");
        monitor.innerHTML = "#" + log.monitorId;
        leftContent.appendChild(monitor);

        if (log.tags != null) {

            const toolTip = document.createElement("span");
            toolTip.classList.add("tags-tooltip");
            toolTip.classList.add("small-badge");
            toolTip.innerHTML = log.tags.split(" ").length.toString() + " tags";

            const toolTipContent = document.createElement("span");
            toolTipContent.classList.add("tags-tooltip-content");
            toolTipContent.innerHTML = log.tags;
            leftContent.appendChild(toolTip);
            leftContent.appendChild(toolTipContent);
        }



        this.appendChild(leftContent);
        if (log.exception != null) {
            this.appendChild(new LogExceptionElement(log.exception));
        }

        this.classList.add(logLevel);
        const span = document.createElement("span");
        span.className = "log-text";
        span.innerHTML = log.text;
        this.appendChild(span);
    }
}

customElements.define('log-entry', LogEntryElement);

