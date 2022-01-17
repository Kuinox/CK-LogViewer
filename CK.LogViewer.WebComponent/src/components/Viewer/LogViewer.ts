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
                const div = document.createElement("div");
                div.classList.add("upload-button");
                div.innerHTML = `<svg viewBox="0 0 122.88 88.98"><style type="text/css">.st0{fill:#fff;fill-rule:evenodd;clip-rule:evenodd;}</style><g><path class="st0" d="M85.33,16.83c12.99-9.83,31.92,1.63,31.92,13.63c0,7.75-2.97,10.79-7.57,14.03 c23.2,12.41,12.7,39.86-7.54,44.49l-70.69,0c-33.2,0-45.48-44.99-10.13-55.89C14.69,6.66,66.5-17.2,85.33,16.83L85.33,16.83z M53.37,69.54V53.66H39.16l22.29-26.82l22.29,26.82H69.53v15.88H53.37L53.37,69.54z"/></g></svg>`;
                div.onclick = this.uploadLogToPublicInstance;
                this.appendChild(div);
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
    waitingMessages : ILogEntry[] = [];
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


