window.mathEditor = {
    startRenderLoop: function (dotnetRef) {
        function loop(timestamp) {
            dotnetRef.invokeMethodAsync("OnAnimationFrame", timestamp);
            requestAnimationFrame(loop);
        }
        requestAnimationFrame(loop);
    },
    getViewportSize: function () {
        return {
            width: window.innerWidth,
            height: window.innerHeight
        }
    },
    setTitle: function (title) {
        const name = "Math Editor";
        
        if (title === "")
            title = name;
        else {
            title = title.replace('.mxe', '').replaceAll('"', '');
            title += " - " + name;
        }
        document.title = title;
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
    copyToClipboard: async function (content) {
        await navigator.clipboard.writeText(content);
    },
    registerPasteHandler: function (dotnetRef) {
        document.addEventListener("paste", (event) => {
            const content = event.clipboardData.getData("text");
            dotnetRef.invokeMethodAsync("OnPaste", content);
        });
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

            const reservedKeys = new Set(["c", "z", "y", "s", "o"])
            if (ctrl && reservedKeys.has(key))
                e.preventDefault();
            
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


window.mathField = {
    init: function (mf, dotnetRef, latex) {
        if (latex) mf.value = latex;

        mf.addEventListener("input", () => {
            dotnetRef.invokeMethodAsync("OnMathChanged", mf.value);
        });
    },
    setValue: function (element, latex) {
        element.value = latex;
    },
    getValue: function (element) {
        return element.value;
    },
    getWidth: function (element) {
        const mf = element._mathField || element;

        const root = mf.shadowRoot;
        if (!root) return 0;

        const content = root.querySelector('[part="content"]');
        if (!content) return 0;

        return content.scrollWidth;
    }
}