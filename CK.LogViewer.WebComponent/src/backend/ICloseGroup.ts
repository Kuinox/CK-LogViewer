import { ILogGroup } from "./ILogGroup";
import { LogType } from "./LogType";

export interface ICloseGroup extends ILogGroup {
    logType: LogType.CloseGroup;
    conclusions?:  {
        text: string,
        tag: string
    }[]
}