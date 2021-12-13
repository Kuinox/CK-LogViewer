import { XOR } from "./types";

export function setHidden(element: HTMLElement, doHide: boolean): void {
    const hidden = isHidden(element);
    if (hidden == doHide) return;
    if(doHide) {
        element.classList.add("hidden");
    } else {
        element.classList.remove("hidden");
    }
}

export function isHidden(element: HTMLElement): boolean {
    return element.classList.contains("hidden");
}