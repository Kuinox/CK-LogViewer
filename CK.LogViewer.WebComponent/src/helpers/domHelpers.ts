import { XOR } from "./types";

export function createDiv(options?: NodeOptions & ElementOptions): HTMLDivElement {
    const div = document.createElement("div");
    setElementOptions(div, options);
    return div;
}

export function setElementOptions(element: Element, options?: ElementOptions): void {
    if (options === undefined) return;
    if (options.classList !== undefined) {
        element.classList.add(...options.classList);
    }
    if (options.className !== undefined) {
        element.classList.add(options.className);
    }

    if (options.childNode !== undefined) {
        element.appendChild(options.childNode);
    }
    if (options.childNodes !== undefined) {
        for (let i = 0; i < options.childNodes.length; i++) {
            element.appendChild(options.childNodes[i]);
        }
    }
}

export type ElementOptions = NodeOptions & ClassOptions;

export type NodeOptions = XOR<{
    childNodes?: Node[]
}, {
    childNode?: Node
}>;

export interface ClassOptions {
    classList?: string[],
    className?: string
}
