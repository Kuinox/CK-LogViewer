import { uploadLog } from "../backend/api";
import { LogViewer } from "./Viewer/LogViewer";

export class UploadFile extends HTMLElement {

    constructor() {
        super();
        const fileUploadForm = document.createElement("form");
        const inputFileUpload = document.createElement("input");
        inputFileUpload.setAttribute("id", "files");
        inputFileUpload.setAttribute("name", "files");
        inputFileUpload.setAttribute("type", "file");
        inputFileUpload.setAttribute("size", "1");
        fileUploadForm.appendChild(inputFileUpload);
        inputFileUpload.addEventListener("change", this.uploadFiles);
        this.appendChild(fileUploadForm);
    }

    uploadFiles = async (): Promise<void> => {
        const input = document.getElementById('files') as any;
        const files = input.files;
        const formData = new FormData();

        for (let i = 0; i != files.length; i++) {
            formData.append("files", files[i]);
        }
        const hash = await uploadLog(formData);
        window.location.replace(`http://localhost:5000/#/${hash}`);
        (document.getElementsByTagName("log-viewer")[0] as LogViewer).render(hash);
    };


}

customElements.define('upload-file', UploadFile);
