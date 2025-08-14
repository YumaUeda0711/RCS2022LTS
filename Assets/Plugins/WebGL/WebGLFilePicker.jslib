mergeInto(LibraryManager.library, {
  // C# から: OpenImageFile(gameObject.name, "OnDataUrlChunk");
  OpenImageFile: function(goPtr, methodPtr) {
    var goName = UTF8ToString(goPtr);
    var method = UTF8ToString(methodPtr);

    // デバッグログ（必要なければ消してOK）
    console.log("[JS] OpenImageFile -> go:", goName, "method:", method);

    var input = document.createElement('input');
    input.type = 'file';
    input.accept = 'image/*';  // 必要なら '.png,.jpg,.jpeg' に絞る
    input.style.display = 'none';

    input.onchange = function(e) {
      var file = e.target.files && e.target.files[0];
      if (!file) return;

      var reader = new FileReader();
      reader.onload = function(ev) {
        var dataUrl = ev.target.result; // "data:image/png;base64,AAAA..."
        var CHUNK = 32768;
        var len = dataUrl.length;

        // 1) メタ（任意だが便利）
        var meta = JSON.stringify({ name: file.name, type: file.type, size: file.size, dataUrlLength: len });
        try { SendMessage(goName, method, "meta|" + meta); }
        catch (err) { console.error("[JS] SendMessage meta failed:", err); }

        // 2) 本体（分割送信）
        for (var i = 0; i < len; i += CHUNK) {
          var chunk = dataUrl.substring(i, i + CHUNK);
          var isLast = (i + CHUNK >= len) ? "1" : "0";
          try {
            SendMessage(goName, method, "data|" + isLast + "|" + chunk);
          } catch (err) {
            console.error("[JS] SendMessage data failed at", i, err);
            break;
          }
        }
      };
      reader.readAsDataURL(file);
    };

    document.body.appendChild(input);
    input.click();
    setTimeout(function(){ document.body.removeChild(input); }, 0);
  }
});
