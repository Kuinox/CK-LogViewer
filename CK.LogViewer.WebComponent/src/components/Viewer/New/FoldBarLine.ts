export class FoldBarLine {
    static idCounter = 0;
    className = "fold-bar-" + (FoldBarLine.idCounter++);
    constructor() {
        const style = document.createElement("style");
        style.innerHTML = this.className+ "{ background-color: rgba(255, 255, 255, 0.1); }";
        document.getElementsByTagName('head')[0].appendChild(style);
    }


    add(foldBar: HTMLElement): void {
        foldBar.classList.add(this.className);
    }
//instead of keeping a list of foldBar we could fetch from the css class.
}
