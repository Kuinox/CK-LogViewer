export class CssClassManager {
    private refCount: Map<string, { refCount: number, style: HTMLStyleElement }>;
    /**
     * Assume you already have a class in your html doc.
     */
    constructor() {
        this.refCount = new Map();
    }

    public requireClass(ruleName: string, rule: string): void {
        const classInfo = this.refCount.get(ruleName);
        if (classInfo === undefined) {
            const style = document.createElement("style");
            style.innerHTML = rule;
            document.body.appendChild(style);
            this.refCount.set(ruleName, {
                refCount: 1,
                style: style
            });
        }
    }

    public updateClass(ruleName: string, newRule: string): void {
        const classInfo = this.refCount.get(ruleName);
        if (classInfo === undefined) throw new RangeError("This rule is not registered.");
        classInfo.style.innerHTML = newRule;
    }

    public releaseClass(ruleName: string): void {
        const classInfo = this.refCount.get(ruleName);
        if (classInfo === undefined) throw new RangeError("This rule is not registered.");
        if (classInfo.refCount === 1) {
            this.refCount.delete(ruleName);
            classInfo.style.remove();
        } else {
            this.refCount.set(ruleName, {
                refCount: classInfo.refCount - 1,
                style: classInfo.style
            });
        }
    }
}
