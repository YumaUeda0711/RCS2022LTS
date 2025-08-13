mergeInto(LibraryManager.library, {
  OpenImageFile: function(goPtr, methodPtr) {
    var go = UTF8ToString(goPtr);
    var method = UTF8ToString(methodPtr);

    var input = document.createElement('input');
    input.type = 'file';
    input.accept = 'image/*';
    input.style.display = 'none';

    input.onchange = function(e) {
      var file = e.target.files[0];
      if (!file) return;
      var reader = new FileReader();
      reader.onload = function(ev) {
        var dataUrl = ev.target.result; // "data:image/xxx;base64,...."
        var maxLen = 64000; // ‘—M•¶š—ñ‚ğ•ªŠ„iŠÂ‹«ˆË‘¶‚Ì’·‚³§ŒÀ‘Îôj
        for (var i = 0; i < dataUrl.length; i += maxLen) {
          var chunk = dataUrl.substring(i, i + maxLen);
          var isLast = (i + maxLen >= dataUrl.length) ? "1" : "0";
          SendMessage(go, method, isLast + "|" + chunk);
        }
      };
      reader.readAsDataURL(file);
    };

    document.body.appendChild(input);
    input.click();
    setTimeout(function(){ document.body.removeChild(input); }, 0);
  }
});
