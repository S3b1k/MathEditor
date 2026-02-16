window.mathEditor = {
    startRenderLoop: function (dotnetRef) {
        function loop() {
            dotnetRef.invokeMethodAsync("OnAnimationFrame");
            requestAnimationFrame(loop);
        }
        requestAnimationFrame(loop);
    },
    setTitle: function (title) {
        title = title.replace('.mxe', '').replaceAll('"', '');
        document.title = title + " - Math Editor";
    },
    isElementFocused: function (element) {
        return document.activeElement === element;
    },
    focusElement: function (element) {
        this.unfocusElement();
        element.focus();
        
        const range = document.createRange();
        range.selectNodeContents(element);

        const selection = window.getSelection();
        selection.removeAllRanges();
        selection.addRange(range);
    },
    unfocusElement: function () {
        document.activeElement.blur();
    },
    saveFile: function (filename, content) {
        const blob = new Blob([content], { type: "application/octet-stream" });
        const url = URL.createObjectURL(blob);
        
        const a = document.createElement("a");
        a.href = url;
        a.download = filename;
        a.click();
        
        URL.revokeObjectURL(url);
    },
    openFilePicker: function (element) {
        element.click();
    },
    readFile: async function (element) {
        return new Promise((resolve, reject) => {
            const file = element.files[0];
            if (!file) {
                resolve(null);
                return;
            }
            
            const reader = new FileReader();
            reader.onload = () => {
                resolve({
                    content: reader.result,
                    name: file.name
                });
            }
            reader.onerror = reject;
            reader.readAsText(file);
        })
    },
    toggleTheme: function (theme) {
        document.documentElement.setAttribute('data-theme', theme);
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
    getHeight: function (element) {
        return element.scrollHeight;
    },
    getWidth: function (element) {
        return element.scrollWidth;
    },
    hideOverflow: function (element, value) {
        element.style.setProperty("--overflow", value ? "hidden" : "visible");
    }
};


window.textField = {
    getText: function (element) {
        return element.innerText;
    },
    setText: function (element, text) {
        element.innerText = text;
    },
    clearSelection: function () {
        const selection = window.getSelection();
        if (selection)
            selection.removeAllRanges();
    },
    toggleSpellCheck: function (element, value) {
        element.spellcheck = value;
    }
};