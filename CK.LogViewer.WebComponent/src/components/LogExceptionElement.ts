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
            // const modal = document.createElement('dialog');
            // const closeTag = document.createElement("span");
            // closeTag.setAttribute("class","close");
            // closeTag.innerHTML = "&times;";
            // modal.appendChild(closeTag);
            // modal.innerHTML = log.stackTrace;
            // entry.appendChild(modal);
            // modal.showModal();



            const modal = document.createElement('div');
            modal.setAttribute("class","modal");

            document.addEventListener("click",(event)=>{
                const target = event.target as any;
                if(target.className === "modal"){
                    modal.remove();
                }
            })

            const modalContent = document.createElement('div');
            modalContent.setAttribute("class","modal-content");

            const closeTag = document.createElement("span");
            closeTag.setAttribute("class","close");
            closeTag.innerHTML = "&times;";
            closeTag.addEventListener("click",()=>{
                modal.remove();
            })

            const content = document.createElement("p");
            content.innerHTML = log.stackTrace;

            modalContent.appendChild(closeTag);
            modalContent.appendChild(document.createElement("br"));
            modalContent.appendChild(content);
            modal.appendChild(modalContent)

            entry.appendChild(modal);
            // modal.showModal();

        })
        const br = document.createElement("br");
        entry.appendChild(a);
        entry.appendChild(br);
        return entry;
    }
}

customElements.define('log-exception', LogExceptionElement);

