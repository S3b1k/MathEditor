import { ComputeEngine } from "https://cdn.jsdelivr.net/npm/@cortex-js/compute-engine@0.30.2/dist/compute-engine.min.esm.js"
const ce = new ComputeEngine();

window.mathComputeEngine = {
    evaluate: function (latex) {
        if (!latex) return "";

        const expr = ce.parse(latex);
        const evaluated = expr.evaluate();
        return evaluated.latex ?? evaluated.toString();
    },
    evaluateNumeric(latex) {
        if (!latex) return null;

        const expr = ce.parse(latex);
        const evaluated = expr.N();
        return evaluated.valueOf();
    },
}