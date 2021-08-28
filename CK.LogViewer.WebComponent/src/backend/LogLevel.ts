export enum LogLevel {
    /**
    No logging level.
    **/
    None = 0,
    /**
    Debug logging level (the most verbose level).
    **/
    Debug = 1,
    /**
    A trace logging level (quite verbose level).
    **/
    Trace = 2,
    /**
    An info logging level.
    **/
    Info = 4,
    /**
    A warn logging level.
    **/
    Warn = 8,
    /**
    An error logging level: denotes an error for the current activity. This error
    does not necessarily abort the activity.
    **/
    Error = 16,
    /**
    A fatal error logging level: denotes an error that breaks (aborts) the current
    activity. This kind of error may have important side effects on the system.
    **/
    Fatal = 32,
    /**
    Mask that covers actual levels to easily ignore CK.Core.LogLevel.IsFiltered bit.
    **/
    Mask = 63
}

export const stringToLogLevel: ReadonlyMap<string, LogLevel> = new Map([
    ["none", LogLevel.None],
    ["debug", LogLevel.Debug],
    ["trace", LogLevel.Trace],
    ["info", LogLevel.Info],
    ["warn", LogLevel.Warn],
    ["error", LogLevel.Error],
    ["fatal", LogLevel.Fatal],
]);


export const logLevelToString: ReadonlyMap<LogLevel, string> = new Map([
    [LogLevel.Debug, "debug"],
    [LogLevel.Trace, "trace"],
    [LogLevel.Info, "info"],
    [LogLevel.Warn, "warn"],
    [LogLevel.Error, "error"],
    [LogLevel.Fatal, "fatal"],
    [LogLevel.None, "none"]
]);
