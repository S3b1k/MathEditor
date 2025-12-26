window.mathEditor = {
    startRenderLoop: function (dotnetRef) {
        function loop() {
            dotnetRef.invokeMethodAsync("OnAnimationFrame");
            requestAnimationFrame(loop);
        }
        requestAnimationFrame(loop);
    }
};