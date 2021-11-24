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
export class LogViewer extends HTMLElement { //TODO: hide this behind an object, so consumer dont see HTML methods.
    private loadIcon: LoadingIcon | undefined;
    private logZone!: HTMLDivElement;
    private cssClassManager = new CssClassManager();
    private colorGenerator = new ColorGenerator();
    private logviewerState = new LogViewerState();
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
        const perf = performance.now();
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
        console.log("renderTime: " + (performance.now() - perf) + "ms");
    }

    public appendEntry(entry: ILogEntry): void {
        this.logZone.append(new LogEntryElement(entry, this.cssClassManager, this.colorGenerator, this.logviewerState, this.rulerClicked));
    }

    private rulerClicked = (groupOffset: number) => {
        console.log(groupOffset);
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
    }
}

customElements.define('log-viewer', LogViewer);


