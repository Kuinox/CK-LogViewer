import { uploadLog } from "../backend/api";
import { createButton } from "../helpers/domHelpers";

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

        fileUploadForm.appendChild(createButton({
            innerHTML: "Upload",
            onClick: async (event) => {
                event.preventDefault();
                this.uploadFiles();
                // fileUploadForm.remove();
            }
        }));

        this.appendChild(fileUploadForm);
    }

    async uploadFiles(): Promise<void> {
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
