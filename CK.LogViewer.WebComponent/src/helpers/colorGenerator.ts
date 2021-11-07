export class ColorGenerator {
    private slices: number[];
    private min: number;
    private max: number;
    constructor() {
        this.slices = [];
        this.min = 0;
        this.max = 360;
    }

    public getUniqueColor(): string {
        const hue = this.getUniqueNumber();
        return "hsl(" + hue + ",100%,50%)";
    }

    seed = 1;
    private random(max: number) {
        if (max == 1) return 0;
        const x = Math.sin(this.seed++) * max;
        const val = Math.floor(Math.abs(x));
        return val;
    }

    private getUniqueNumber(): number {
        if (this.slices.length == 0) {
            this.slices[0] = this.min;
            this.slices[1] = this.max;
            return this.min;
        }
        let range = 0;
        let candidates = [];
        for (let i = 0; i < this.slices.length - 1; i++) {
            const element = this.slices[i];
            const nextElement = this.slices[i + 1];
            const currentRange = nextElement - element;
            if (currentRange == range) {
                candidates.push({
                    value: element + currentRange / 2,
                    index: i
                });
            }
            if (currentRange > range) {
                range = currentRange;
                candidates = [];
                candidates.push({
                    value: element + currentRange / 2,
                    index: i
                });
            }
        }
        const i = this.random(candidates.length);
        const candidate = candidates[i];
        this.slices.splice(candidate.index + 1, 0, candidate.value);
        return candidate.value;
    }
}
