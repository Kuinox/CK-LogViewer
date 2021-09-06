import { XOR } from "./types";

export function createDiv(options?: DivOptions): HTMLDivElement {
    const div = document.createElement("div");
    setElementOptions(div, options);
    if (options?.onClick !== undefined) {
        div.addEventListener("click", options.onClick);
    }
    return div;
}

export function createButton(options?: ButtonOptions): HTMLButtonElement {
    const button = document.createElement("button");
    setElementOptions(button, options);
    if (options?.onClick !== undefined) {
        button.addEventListener("click", options.onClick);
    }
    return button;
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
    if (options.innerHTML !== undefined) {
        element.innerHTML = options.innerHTML;
    }
}

export function setChildOf(base: HTMLElement, newChild: HTMLElement, oldChild: HTMLElement | undefined): HTMLElement {
    if (oldChild === undefined) {
        base.appendChild(newChild);
    } else {
        base.replaceChild(newChild, oldChild);
    }
    return newChild;
}
export function toggleHidden(element: HTMLElement): void {
    if (isHidden(element)) {
        element.classList.remove("hidden");
    } else {
        element.classList.add("hidden");
    }
}

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

export type ElementOptions = NodeOptions & ClassOptions;
export type DivOptions = ElementOptions & DivListenerOptions;
export type ButtonOptions = ElementOptions & ButtonListenerOptions;
export type NodeOptions = XOR<XOR<{
    childNodes?: Node[]
}, {
    childNode?: Node
}>, {
    innerHTML?: string
}>;

export interface ClassOptions {
    classList?: string[],
    className?: string
}

export interface DivListenerOptions {
    onClick?: (this: HTMLDivElement, ev: MouseEvent) => any;
}

export interface ButtonListenerOptions {
    onClick?: (this: HTMLButtonElement, ev: MouseEvent) => any;
}
