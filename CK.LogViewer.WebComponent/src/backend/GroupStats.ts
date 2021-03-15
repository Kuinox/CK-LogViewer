import { LogLevel } from "./LogLevel";

export type GroupStats = {
    [key in  keyof typeof LogLevel]?: number;
};
