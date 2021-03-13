import { CKExceptionData } from "../LogType";

export class LogExceptionElement extends HTMLElement {
    constructor() {
        super();
    }

    static create(log: CKExceptionData): LogExceptionElement {
        const entry = new LogExceptionElement();
        const a = document.createElement("a");
        a.innerHTML = log.typeException;
        a.href = "";
        a.addEventListener('click', (e) => {
            e.preventDefault();
            this.createModal(log, entry);

        });
        const br = document.createElement("br");
        entry.appendChild(a);
        entry.appendChild(br);
        return entry;
    }

    static createModal(log: CKExceptionData, entry: LogExceptionElement): void {
        const modal = document.createElement('div');
        modal.setAttribute("class", "modal");
        modal.addEventListener("click", (event) => {
            const target = event.target as any;
            if (target.className === "modal") {
                modal.remove();
            }
        });

        const modalContent = document.createElement('div');
        modalContent.setAttribute("class", "modal-content");

        const closeTag = document.createElement("span");
        closeTag.setAttribute("class", "close");
        closeTag.innerHTML = "&times;";
        closeTag.addEventListener("click", () => {
            modal.remove();
        });

        const content = document.createElement("p");
        content.innerHTML = log.stackTrace;

        modalContent.appendChild(closeTag);
        modalContent.appendChild(document.createElement("br"));
        modalContent.appendChild(content);
        modal.appendChild(modalContent);

        entry.appendChild(modal);
    }

}

customElements.define('log-exception', LogExceptionElement);

