export class LoadingIcon extends HTMLElement {
    interval!: NodeJS.Timeout;
    constructor() {
        super();

    }
    connectedCallback(): void {
        this.innerHTML = "/";
        this.interval = setInterval(this.spin, 100);
    }

    disconnectedCallback(): void {
        clearInterval(this.interval);
    }

    step = 0;
    spin = (): void => {
        switch (this.step) {
            case 0:
                this.innerHTML = "/";
                break;
            case 1:
                this.innerHTML = "-";
                break;
            case 3:
                this.innerHTML = "|";
                this.step = 0;
                return;
            case 2:
                this.innerHTML = "\\";
                break;
        }
        this.step++;
    };


}
customElements.define('loading-icon', LoadingIcon);
