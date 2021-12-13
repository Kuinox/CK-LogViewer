import { ICKExceptionData } from "../../backend/ICKExceptionData";

export class LogExceptionElement extends HTMLElement {
    constructor(log: ICKExceptionData) {
        super();

        const content = document.createElement("div");
        const exceptionTitle = document.createElement("h1");
        exceptionTitle.innerText = log.typeException;
        content.appendChild(exceptionTitle);
        const message = document.createElement("p");
        message.innerText = log.message;
        content.appendChild(message);
        const stacktrace = document.createElement("p");
        stacktrace.innerText = log.stackTrace;
        content.appendChild(stacktrace);
        this.appendChild(content);
    }
}

customElements.define('log-exception', LogExceptionElement);

