import { getLogs, uploadLog } from "../backend/api";
import { LogEntryElement } from "./LogEntryElement";
import { LogGroupElement } from "./LogGroupElement";

export class UploadFile extends HTMLElement {

    async connectedCallback(): Promise<void> {
        const fileUploadForm = document.createElement("form");
        const inputFileUpload = document.createElement("input");
        inputFileUpload.setAttribute("id", "files");
        inputFileUpload.setAttribute("name", "files");
        inputFileUpload.setAttribute("type", "file");
        inputFileUpload.setAttribute("size", "1");

        const buttonFileUpload = document.createElement("button");
        buttonFileUpload.innerHTML = "Upload";
        buttonFileUpload.addEventListener("click", async (event) => {
            event.preventDefault();
            await this.uploadFiles();
            // fileUploadForm.remove();
        });


        fileUploadForm.appendChild(inputFileUpload);
        fileUploadForm.appendChild(buttonFileUpload);

        this.appendChild(fileUploadForm);


    }

    async uploadFiles() {
        const input = document.getElementById('files') as any;
        const files = input.files;
        const formData = new FormData();

        for (let i = 0; i != files.length; i++) {
            formData.append("files", files[i]);
        }

        const hash = await uploadLog(formData);
        window.location.replace(`http://localhost:5000/#/${hash}`);
        window.location.reload();

    }


}

customElements.define('upload-file', UploadFile);
