window.mathEditor = {
    startRenderLoop: function (dotnetRef) {
        function loop() {
            dotnetRef.invokeMethodAsync("OnAnimationFrame");
            requestAnimationFrame(loop);
        }
        requestAnimationFrame(loop);
    }
};

window.keyboardActions = {
    register: function (dotnetRef) {
        document.addEventListener("keydown", function (e) {
            const key = e.key.toLowerCase();
            const ctrl = e.ctrlKey || e.metaKey;
            const shift = e.shiftKey;
            const alt = e.altKey;

            dotnetRef.invokeMethodAsync("OnKeypress", key, ctrl, shift, alt);
        });
    }
};

window.field = {
    hasContentFocus: function (element) {
        return document.activeElement === element;
    }
};

window.textField = {
    getText: function (element) {
        return element.innerText;
    },
    setText: function (element, text) {
        element.innerText = text;
    },
    getHeight: function (element) {
        return element.scrollHeight;
    },
    getWidth: function (element) {
        return element.scrollWidth;
    },
    clearSelection: function () {
        const selection = window.getSelection();
        if (selection)
            selection.removeAllRanges();
    }
};