import { CKExceptionData } from "../backend/CKExceptionData";

export class LogExceptionElement extends HTMLElement {
    constructor(log: CKExceptionData) {
        super();
        const a = document.createElement("a");
        a.innerHTML = log.typeException;
        a.href = "";
        a.addEventListener('click', (e) => {
            e.preventDefault();
            this.appendChild(LogExceptionElement.createModal(log));
        });
        const br = document.createElement("br");
        this.appendChild(a);
        this.appendChild(br);
    }

    private static createModal(log: CKExceptionData ): HTMLElement {
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

        return modal;
    }

}

customElements.define('log-exception', LogExceptionElement);

