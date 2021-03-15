// https://stackoverflow.com/questions/42123407/does-typescript-support-mutually-exclusive-types
// Maybe not foolproof.
// Does the job for now.

export type Without<T, U> = { [P in Exclude<keyof T, keyof U>]?: never };
// eslint-disable-next-line @typescript-eslint/ban-types
export type XOR<T, U> = (T | U) extends object ? (Without<T, U> & U) | (Without<U, T> & T) : T | U;
