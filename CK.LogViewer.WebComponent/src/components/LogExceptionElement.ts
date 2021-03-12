import { CKExceptionData } from "../LogType";

export class LogExceptionElement extends HTMLElement {
    constructor() {
        super();
    }

    openModal = () => {
       console.log("ddd")
    }

    static create(log: CKExceptionData) {
        const entry = new LogExceptionElement();
        const a = document.createElement("a");
        a.innerHTML = log.typeException;
        a.href ="";
        a.addEventListener('click',(e)=>{
            e.preventDefault();
            const modal = document.createElement('dialog');
            modal.innerHTML = log.stackTrace;
            entry.appendChild(modal);
            modal.showModal();
        })
        entry.appendChild(a);
        return entry;
    }
}

customElements.define('log-exception', LogExceptionElement);

