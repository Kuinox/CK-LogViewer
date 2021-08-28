import { Api } from "../../backend/api";
import { LogEntry } from "../../backend/LogEntry";
import { LoadingIcon } from "../Common/LoadingIcon";
import { LogMetadata } from "./New/LogMetadataElement";
import { LogZoneElement } from "./New/LogZoneElement";

export class LogViewer extends HTMLElement { //TODO: hide this behind an object, so consumer dont see HTML methods.
    private loadIcon: LoadingIcon | undefined;
    private logZone!: LogZoneElement;
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
            this.appendEntry(curr);
            if (this.aborter.signal.aborted) {
                return;
            }
            if (i % 100 === 99) {
                await this.sleep(0);
            }
        }
        this.removeLoadIcon();
    }

    public appendEntry(entry: LogEntry): void {
        this.logZone.appendLog(entry);
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
