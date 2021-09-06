import { Api } from "../../backend/api";
import { LogEntry } from "../../backend/LogEntry";
import { LogGroup } from "../../backend/LogGroup";
import { LogType } from "../../backend/LogType";
import { isHidden, setHidden, toggleHidden } from "../../helpers/domHelpers";
import { LoadingIcon } from "../Common/LoadingIcon";
import { CssClassManager } from "./CssClassManager";
import { LogEntryElement } from "./LogEntryElement";
import { GroupSummary } from "./LogGroup/GroupSummary";
import { LogMetadata } from "./New/LogMetadataElement";
import { LogZoneElement } from "./New/LogZoneElement";
export class LogViewer extends HTMLElement { //TODO: hide this behind an object, so consumer dont see HTML methods.
    private loadIcon: LoadingIcon | undefined;
    private logZone!: LogZoneElement;
    private cssClassManager = new CssClassManager();
    constructor(displayLoading: boolean) {
        super();
        this.reset(displayLoading);
    }

    connectedCallback(): void {
        const hash = window.location.hash;
        if (hash.length > 1) {
            this.render(hash.slice(1));
        }
    }

    aborter: AbortController | undefined;
    sleep(ms: number): Promise<void> {
        return new Promise(resolve => setTimeout(resolve, ms));
    }

    async render(filename: string): Promise<void> { //TODO: move this out of the LogViewer component.
        const api = new Api(filename);
        this.aborter?.abort();
        const aborter = new AbortController();
        this.aborter = aborter;
        const logs = await api.getLogs(aborter.signal);
        if (this.aborter.signal.aborted) {
            return;
        }
        this.reset(true);
        for (let i = 0; i < logs.length; i++) {
            const curr = logs[i];
            this.appendEntry(curr, this.cssClassManager);
            if (this.aborter.signal.aborted) {
                return;
            }
            if (i % 100 === 99) {
                await this.sleep(0);
            }
        }
        this.removeLoadIcon();
    }

    public appendEntry(entry: LogEntry, cssClassManager: CssClassManager): void {
        this.logZone.appendLog(entry, cssClassManager, this.rulerClicked);
    }

    private rulerClicked(entry: LogEntryElement, groupOffset: number) {
        let wasPreviouslyHidden = false;
        let openGroup: LogEntryElement;
        LogEntryElement.runOnGroup(groupOffset, (curr) => {
            if (curr.logData.offset !== groupOffset
                && curr.logData.groupOffset === groupOffset
                && curr.logData.logType === LogType.OpenGroup
                && isHidden(curr)) {
                wasPreviouslyHidden = true;
            }
            if(curr.logData.offset === groupOffset) {
                openGroup = curr;
            }
        });
        LogEntryElement.runOnGroup(groupOffset, (curr) => {
            if (curr.logData.offset !== groupOffset && !(curr.logData.logType === LogType.CloseGroup && curr.logData.groupOffset === groupOffset)) {
                setHidden(curr, !wasPreviouslyHidden);
            }
        });
        const group = openGroup!.logData as LogGroup;
        entry.insertAdjacentElement("afterend",new GroupSummary(group, () => {
            console.log("hello");
        }));
    }
    /**
     *
     * @param displayLoading true => display a loading icon. false => display nothing.
     */
    public reset(displayLoading: boolean): void {
        this.removeLoadIcon();
        this.logZone?.remove();
        this.logZone = new LogZoneElement();
        this.appendChild(this.logZone);
        if (displayLoading) {
            this.loadIcon?.remove();
            this.loadIcon = new LoadingIcon();
            this.appendChild(this.loadIcon);
        }
    }

    public removeLoadIcon(): void {
        this.loadIcon?.remove();
    }
}

customElements.define('log-viewer', LogViewer);


