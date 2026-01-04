window.mathFieldInterop = {
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
};