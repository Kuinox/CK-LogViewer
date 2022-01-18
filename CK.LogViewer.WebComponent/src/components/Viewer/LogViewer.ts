import { Api } from "../../backend/api";
import { ILogEntry } from "../../backend/ILogEntry";
import { ILogGroup } from "../../backend/ILogGroup";
import { LogType } from "../../backend/LogType";
import { isHidden, setHidden } from "../../helpers/domHelpers";
import { LoadingIcon } from "../Common/LoadingIcon";
import { CssClassManager } from "./CssClassManager";
import { LogEntryElement } from "./LogEntryElement";
import { GroupSummary } from "./GroupSummary";
import { LogLevel } from "../../backend/LogLevel";
import { ColorGenerator } from "../../helpers/colorGenerator";
import { LogViewerState } from "./LogLineBaseElement";
import { isPublicInstance, openOnPublicInstance } from "../../services/mainServerService";
import { MQTTService } from "../../backend/MQTTHelpers";
import { IPublishPacket } from "mqtt";

export class LogViewer extends HTMLElement { //TODO: hide this behind an object, so consumer dont see HTML methods.
    private loadIcon: LoadingIcon | undefined;
    private logZone!: HTMLDivElement;
    private cssClassManager = new CssClassManager();
    private colorGenerator = new ColorGenerator();
    private logviewerState = new LogViewerState();
    private mqttService = new MQTTService("localhost:1884");
    private isSetup = false;
    private previousSubscribe: undefined | (() => void);
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

    uploadLogToPublicInstance = (): void => {
        const api = this.api;
        if (api === undefined) {
            console.error("API is undefined.");
            return;
        }
        openOnPublicInstance(api);
    };
    downloadTextLogFile = async (): Promise<void> => {
        const api = this.api;
        if (api === undefined) {
            console.error("API is undefined.");
            return;
        }
        const text = await api.getLogsText();
        console.log(text);
        // Download the text string as a file.
        const element = document.createElement('a');
        element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(text));
        element.setAttribute('download', `${this.api?.filename}.log`);

        element.style.display = 'none';
        document.body.appendChild(element);

        element.click();

        document.body.removeChild(element);
    };


    aborter: AbortController | undefined;
    sleep(ms: number): Promise<void> {
        return new Promise(resolve => setTimeout(resolve, ms));
    }
    api: Api | undefined;


    async render(filename: string): Promise<void> { //TODO: move this out of the LogViewer component.
        if (this.previousSubscribe !== undefined) {
            this.previousSubscribe();
        }
        this.previousSubscribe = await this.mqttService.listenTo(filename, this.processLogStream);
        if (!this.isSetup) {
            if (!isPublicInstance()) {
                const buttonsDiv = document.createElement("div");
                buttonsDiv.classList.add("buttons-toolbar");
                const uploadToInstance = document.createElement("div");
                uploadToInstance.classList.add("upload-button");
                uploadToInstance.innerHTML = `<svg viewBox="0 0 122.88 88.98"><g><path class="st0" d="M85.33,16.83c12.99-9.83,31.92,1.63,31.92,13.63c0,7.75-2.97,10.79-7.57,14.03 c23.2,12.41,12.7,39.86-7.54,44.49l-70.69,0c-33.2,0-45.48-44.99-10.13-55.89C14.69,6.66,66.5-17.2,85.33,16.83L85.33,16.83z M53.37,69.54V53.66H39.16l22.29-26.82l22.29,26.82H69.53v15.88H53.37L53.37,69.54z"/></g></svg>`;
                uploadToInstance.onclick = this.uploadLogToPublicInstance;
                buttonsDiv.appendChild(uploadToInstance);
                const downloadFile = document.createElement("div");
                downloadFile.classList.add("download-button");
                downloadFile.innerHTML = `<svg viewBox="0 0 115.28 122.88"><g><path class="st0" d="M25.38,57h64.88V37.34H69.59c-2.17,0-5.19-1.17-6.62-2.6c-1.43-1.43-2.3-4.01-2.3-6.17V7.64l0,0H8.15 c-0.18,0-0.32,0.09-0.41,0.18C7.59,7.92,7.55,8.05,7.55,8.24v106.45c0,0.14,0.09,0.32,0.18,0.41c0.09,0.14,0.28,0.18,0.41,0.18 c22.78,0,58.09,0,81.51,0c0.18,0,0.17-0.09,0.27-0.18c0.14-0.09,0.33-0.28,0.33-0.41v-11.16H25.38c-4.14,0-7.56-3.4-7.56-7.56 V64.55C17.82,60.4,21.22,57,25.38,57L25.38,57z M29.56,68.12h22.34V74h-7.5v17.9h-7.34V74h-7.5V68.12L29.56,68.12z M53.63,68.12 h8.1l4.21,7.32l4.09-7.32h8l-7.39,11.52l8.08,12.26h-8.27l-4.66-7.64l-4.7,7.64h-8.2l8.2-12.39L53.63,68.12L53.63,68.12z M79.49,68.12h22.34V74h-7.5v17.9h-7.34V74h-7.5V68.12L79.49,68.12z M97.79,57h9.93c4.16,0,7.56,3.41,7.56,7.56v31.42 c0,4.15-3.41,7.56-7.56,7.56h-9.93v13.55c0,1.61-0.65,3.04-1.7,4.1c-1.06,1.06-2.49,1.7-4.1,1.7c-29.44,0-56.59,0-86.18,0 c-1.61,0-3.04-0.64-4.1-1.7c-1.06-1.06-1.7-2.49-1.7-4.1V5.85c0-1.61,0.65-3.04,1.7-4.1c1.06-1.06,2.53-1.7,4.1-1.7h58.72 C64.66,0,64.8,0,64.94,0c0.64,0,1.29,0.28,1.75,0.69h0.09c0.09,0.05,0.14,0.09,0.23,0.18l29.99,30.36c0.51,0.51,0.88,1.2,0.88,1.98 c0,0.23-0.05,0.41-0.09,0.65V57L97.79,57z M67.52,27.97V8.94l21.43,21.7H70.19c-0.74,0-1.38-0.32-1.89-0.78 C67.84,29.4,67.52,28.71,67.52,27.97L67.52,27.97z"/></g></svg>`;
                downloadFile.onclick = this.downloadTextLogFile;
                buttonsDiv.appendChild(downloadFile);
                this.appendChild(buttonsDiv);
            }
            this.isSetup = true;
        }
        this.api = new Api(filename);
        this.aborter?.abort();
        const aborter = new AbortController();
        this.aborter = aborter;
        const logs = await this.api.getLogs(aborter.signal);
        console.log(`Received ${logs.length} logs.`);
        if (this.aborter.signal.aborted) {
            return;
        }
        this.reset(true);
        this.renderPromise = this.doRender(logs);
        await this.renderPromise;
        this.removeLoadIcon();
    }
    private lastLog: ILogEntry | undefined;
    private renderingDone = true;
    private async doRender(logs: ILogEntry[]) {
        console.log(`Rendering ${logs.length} logs.`);
        this.renderingDone = false;
        this.lastLog = logs[logs.length - 1];
        const perf = performance.now();
        for (let i = 0; i < logs.length; i++) {
            const curr = logs[i];
            this.appendEntry(curr);
            if (this.aborter!.signal.aborted) {
                return;
            }
            if (i % 100 === 99) {
                await this.sleep(0);
            }
        }
        console.log("renderTime: " + (performance.now() - perf) + "ms");
        this.renderingDone = true;
    }
    renderPromise: Promise<void> | undefined;
    subscribed = false;
    waitingMessages: ILogEntry[] = [];
    private processLogStream = (message: IPublishPacket) => {
        const logEntry = JSON.parse(message.payload.toString()) as ILogEntry;
        if (this.lastLog?.logTime !== undefined && this.lastLog.logTime > logEntry.logTime) return;
        if (!this.renderingDone) {
            if (!this.subscribed) {
                if (this.renderPromise === undefined) throw new Error("renderPromise is undefined");
                this.renderPromise.then(() => {
                    this.waitingMessages.forEach(entry => {
                        this.appendEntry(entry);
                    });
                });
            } else {
                this.waitingMessages.push(logEntry);
            }
        } else {
            this.appendEntry(logEntry);
        }
    };

    public appendEntry(entry: ILogEntry): void {
        this.logZone.append(new LogEntryElement(entry, this.cssClassManager, this.colorGenerator, this.logviewerState, this.rulerClicked));
    }

    private rulerClicked = (groupOffset: number) => {
        let hasOpenGroupHidden = false;
        let hasOpenGroup = false;
        let isSimpleLogHidden = false;
        let openGroup: LogEntryElement | undefined;
        LogEntryElement.runOnGroup(groupOffset, (curr) => {
            if (curr.logData.offset !== groupOffset
                && curr.logData.groupOffset === groupOffset
            ) {
                if (curr.logData.logType === LogType.OpenGroup) {

                    if (isHidden(curr)) {
                        hasOpenGroupHidden = true;
                    }
                    hasOpenGroup = true;
                } else if (curr.logData.logType !== LogType.CloseGroup) {
                    isSimpleLogHidden = isHidden(curr) || curr instanceof GroupSummary;
                }
            }
            if (curr.logData.offset === groupOffset) {
                openGroup = curr;
            }
        });
        if (openGroup === undefined) {
            throw new Error("Logic error.");
        }
        const shouldHide = hasOpenGroup ? !hasOpenGroupHidden : !isSimpleLogHidden;
        LogEntryElement.runOnGroup(groupOffset, (curr) => {
            if (curr.logData.offset !== groupOffset && !(curr.logData.logType === LogType.CloseGroup && curr.logData.groupOffset === groupOffset)) {
                setHidden(curr, shouldHide);
            }
        });
        LogEntryElement.runOnGroup(groupOffset, (current) => {
            if (current instanceof GroupSummary) {
                current.remove();
            }
        });
        const group = openGroup!.logData as ILogGroup;
        if (shouldHide) {
            openGroup.insertAdjacentElement("afterend", new GroupSummary({
                groupOffset: groupOffset,
                logType: LogType.Line,
                parentsLogLevel: group.parentsLogLevel.concat([{
                    groupOffset: groupOffset,
                    logLevel: group.logLevel
                }]),
                stats: group.stats,
                logLevel: LogLevel.None,
                offset: -1,
                logTime: "",
                monitorId: group.monitorId,
                tags: "",
                text: undefined
            },
                this.cssClassManager,
                this.colorGenerator,
                this.logviewerState,
                this.rulerClicked, (curr) => {
                    if (curr.isConnected) {
                        this.rulerClicked(groupOffset);
                    }
                }));
        }
    };
    /**
     *
     * @param displayLoading true => display a loading icon. false => display nothing.
     */
    public reset(displayLoading: boolean): void {
        this.removeLoadIcon();
        this.logZone?.remove();
        this.logZone = document.createElement("div");
        this.appendChild(this.logZone);
        if (displayLoading) {
            this.loadIcon?.remove();
            this.loadIcon = new LoadingIcon();
            this.appendChild(this.loadIcon);
        }
    }

    public removeLoadIcon(): void {
        this.loadIcon?.remove();
        this.loadIcon = undefined;
    }
}

customElements.define('log-viewer', LogViewer);


