import { XOR } from "./types";

export function createDiv(options?: DivOptions): HTMLDivElement {
    const div = document.createElement("div");
    setElementOptions(div, options);
    if (options?.onClick !== undefined) {
        div.addEventListener("click", options.onClick);
    }
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

export function setChild(base: HTMLElement, newChild: HTMLElement, oldChild: HTMLElement | undefined): HTMLElement {
    if (oldChild === undefined) {
        base.appendChild(newChild);
    } else {
        base.replaceChild(newChild, oldChild);
    }
    return newChild;
}

export type ElementOptions = NodeOptions & ClassOptions;
export type DivOptions = ElementOptions & DivListenerOptions;
export type NodeOptions = XOR<{
    childNodes?: Node[]
}, {
    childNode?: Node
}>;

export interface ClassOptions {
    classList?: string[],
    className?: string
}

export interface DivListenerOptions {
    onClick?: (this: HTMLDivElement, ev: MouseEvent) => any;
}
