// A highlighted <pre> sits under a transparent <textarea>, so the caret, undo and binding keep working
window.markdownDemo = (function () {
    function paint(textarea) {
        var box = textarea.parentElement;
        var code = box.querySelector('code');
        if (!code)
            return;

        // A trailing new line collapses in the pre, so the last line keeps a placeholder
        var text = textarea.value;
        code.textContent = text.endsWith('\n') ? text + ' ' : text;

        if (window.Prism)
            window.Prism.highlightElement(code, false);

        var pre = code.parentElement;
        pre.scrollTop = textarea.scrollTop;
        pre.scrollLeft = textarea.scrollLeft;
    }

    return {
        attach: function (id) {
            var textarea = document.getElementById(id);
            if (!textarea || textarea.dataset.painted)
                return;

            textarea.dataset.painted = '1';
            textarea.addEventListener('input', function () { paint(textarea); });
            textarea.addEventListener('scroll', function () { paint(textarea); });
            paint(textarea);
        },
        refresh: function (id) {
            var textarea = document.getElementById(id);
            if (textarea)
                paint(textarea);
        }
    };
})();
