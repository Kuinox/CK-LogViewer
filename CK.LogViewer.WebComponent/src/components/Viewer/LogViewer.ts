import { Api } from "../../backend/api";
import { LoadingIcon } from "../Common/LoadingIcon";
import { LogGroupElement } from "./LogGroup/LogGroupElement";

export class LogViewer extends HTMLElement {
    groups: HTMLDivElement | undefined;

    constructor() {
        super();
    }

    async connectedCallback(): Promise<void> {
        if (window.location.hash.length > 1) {
            const filename = window.location.hash.substring(1);
            console.log("filename:"+filename);
            if (filename !== undefined) {
                this.render(filename);
            }
        }
    }

    aborter: AbortController | undefined;

    sleep(ms: number) : Promise<void> {
        return new Promise(resolve => setTimeout(resolve, ms));
    }

    async render(filename: string): Promise<void> {
        const api = new Api(filename);
        this.aborter?.abort();
        const aborter = new AbortController();
        this.aborter = aborter;
        const load = new LoadingIcon();
        load.style.fontSize = "50px";
        this.appendChild(load);
        try {
            this.groups?.remove();
            const logs = await api.getLogs(aborter.signal);
            if (this.aborter.signal.aborted) {
                return;
            }
            const groups = document.createElement("div");
            this.groups = groups;
            this.appendChild(groups);
            for (let i = 0; i < logs.length; i++) {
                const curr = logs[i];
                groups.appendChild(LogGroupElement.fromLogEntry(curr, api));
                if (this.aborter.signal.aborted) {
                    return;
                }
                if (i % 100 === 99) {
                    load.remove();
                    await this.sleep(0);
                }
            }
        } finally {
            load.remove();
        }
    }
}

customElements.define('log-viewer', LogViewer);
