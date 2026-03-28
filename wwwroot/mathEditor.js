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
        
        field.clearSelection();
        field.selectAll(element);
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
    },
    registerCanvasDropHandler: function (element, dotnetRef) {
        let dragCounter = 0;

        element.addEventListener("dragenter", (e) => {
            e.preventDefault();
            const item = e.dataTransfer?.items?.[0];
            if (!item || item.kind !== "file") return;

            dragCounter++;
            if (dragCounter !== 1) return;

            const isImage = item.type.startsWith("image/");
            dotnetRef.invokeMethodAsync("OnCanvasDragEnter", isImage);
        });

        element.addEventListener("dragleave", (e) => {
            e.preventDefault();
            const item = e.dataTransfer?.items?.[0];
            if (!item || item.kind !== "file") return;

            dragCounter--;
            if (dragCounter === 0)
                dotnetRef.invokeMethodAsync("OnCanvasDragLeave");
        });

        element.addEventListener("dragover", (e) => {
            e.preventDefault();
        });

        element.addEventListener("drop", async (e) => {
            e.preventDefault();
            dragCounter = 0;

            const file = e.dataTransfer?.files?.[0];
            if (!file) {
                dotnetRef.invokeMethodAsync("OnCanvasDragLeave");
                return;
            }

            if (file.name.endsWith(".mxe")) {
                const text = await file.text();
                dotnetRef.invokeMethodAsync("OnFileDrop", text);
            } else if (file.type.startsWith("image/")) {
                const reader = new FileReader();
                reader.onload = () => {
                    dotnetRef.invokeMethodAsync("OnImageFileDrop", reader.result, e.clientX, e.clientY);
                };
                reader.readAsDataURL(file);
            } else {
                dotnetRef.invokeMethodAsync("OnCanvasDragLeave");
            }
        });
    },
    registerImageDropHandler: function (element, dotnetRef) {
        element.addEventListener("dragover", (e) => {
            e.preventDefault();
        });
        
        element.addEventListener("drop", (e) => {
            e.preventDefault();
            e.stopPropagation();
            
            const file = e.dataTransfer?.files?.[0];
            if (!file || !file.type.startsWith("image/")) return;
            
            const reader = new FileReader();
            reader.onload = () => {
                dotnetRef.invokeMethodAsync("ApplyImage", reader.result);
            };
            reader.readAsDataURL(file);
        });
    }
};


window.keyboardActions = {
    register: function (dotnetRef) {
        document.addEventListener("keydown", (e) => {
            const key = e.key.toLowerCase();
            const ctrl = e.ctrlKey || e.metaKey;
            const shift = e.shiftKey;
            const alt = e.altKey;

            const reservedKeys = new Set(["s", "o"])
            if ((ctrl && reservedKeys.has(key)) || alt)
                e.preventDefault();
            
            dotnetRef.invokeMethodAsync("OnKeypress", key, ctrl, shift, alt);
        });
    }
};


window.clipBoard = {
    copyToClipboard: async function (content) {
        await navigator.clipboard.writeText(content);
    },
    registerPasteHandler: function (dotnetRef) {
        // Prevent linux middle-click paste
        document.addEventListener("auxclick", (e) => {
            if (e.button === 1)
                e.preventDefault();
        });

        document.addEventListener("paste", async (e) => {
            
            for (const item of e.clipboardData.items) {
                if (item.type.startsWith("image/")) {
                    e.preventDefault();
                    const blob = item.getAsFile();
                    const reader = new FileReader();
                    reader.onload = () => {
                        dotnetRef.invokeMethodAsync("OnPaste", reader.result);
                    };
                    reader.readAsDataURL(blob);
                    return;
                }
            }
            
            const text = e.clipboardData.getData("text");
            dotnetRef.invokeMethodAsync("OnPaste", text);
        });
    }
}


window.field = {
    getHeight: function (element) {
        return element.scrollHeight;
    },
    getWidth: function (element) {
        return element.scrollWidth;
    },
    selectAll: function (element) {
        const range = document.createRange();
        range.selectNodeContents(element);

        const selection = window.getSelection();
        selection.removeAllRanges();
        selection.addRange(range);
    },
    clearSelection: function () {
        const selection = window.getSelection();
        if (selection)
            selection.removeAllRanges();
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
    toggleSpellCheck: function (element, value) {
        element.spellcheck = value;
    }
};


window.mathField = {
    registerHandler: function (element, dotnetRef) {
        element.addEventListener("keydown", (e) => {
            switch (e.key) {
                case ':':
                    e.preventDefault();
                    element.executeCommand(["insert", "\\coloneq"])
            }
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