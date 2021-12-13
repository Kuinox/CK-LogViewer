export class LoadingIcon extends HTMLElement {
    interval!: NodeJS.Timeout;
    constructor() {
        super();

    }
    connectedCallback(): void {
        this.innerText = "/";
        this.interval = setInterval(this.spin, 100);
    }

    disconnectedCallback(): void {
        clearInterval(this.interval);
    }

    step = 0;
    spin = (): void => {
        switch (this.step) {
            case 0:
                this.innerText = "/";
                break;
            case 1:
                this.innerText = "-";
                break;
            case 3:
                this.innerText = "|";
                this.step = 0;
                return;
            case 2:
                this.innerText = "\\";
                break;
        }
        this.step++;
    };


}
customElements.define('loading-icon', LoadingIcon);
