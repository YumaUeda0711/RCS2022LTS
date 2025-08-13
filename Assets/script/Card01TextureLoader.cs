using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class Card01TextureLoader : MonoBehaviour
{
    [Header("差し替え先 (card01 の Renderer)")]
    public Renderer targetRenderer;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void OpenImageFile(string goName, string methodName);
#endif

    // ==== A) WebGL: ローカルからアップロード（セッション内） ====
    private readonly StringBuilder dataUrlBuffer = new StringBuilder();

    // アップロードダイアログを開く（ボタンのOnClickに割り当て）
    public void OpenFilePicker()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        OpenImageFile(gameObject.name, "OnDataUrlChunk");
#else
        Debug.Log("OpenFilePicker: WebGLビルドで動作します（Editor/Nativeではダイアログは出ません）");
#endif
    }

    // JS から分割で飛んでくる dataURL を受信（"0|..." / "1|..."）
    public void OnDataUrlChunk(string payload)
    {
        int sep = payload.IndexOf('|');
        bool isLast = payload.Substring(0, sep) == "1";
        string chunk = payload.Substring(sep + 1);
        dataUrlBuffer.Append(chunk);

        if (isLast)
        {
            string dataUrl = dataUrlBuffer.ToString();
            dataUrlBuffer.Clear();
            ApplyDataUrl(dataUrl);
        }
    }

    private void ApplyDataUrl(string dataUrl)
    {
        try
        {
            int comma = dataUrl.IndexOf(',');
            if (comma < 0) throw new Exception("Invalid dataURL");
            string base64 = dataUrl.Substring(comma + 1);
            byte[] bytes = Convert.FromBase64String(base64);

            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false, false); // sRGB
            tex.LoadImage(bytes, markNonReadable: true);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            AssignTexture(tex);
        }
        catch (Exception e)
        {
            Debug.LogError($"ApplyDataUrl error: {e.Message}");
        }
    }

    private void AssignTexture(Texture2D tex)
    {
        if (!targetRenderer) { Debug.LogWarning("targetRenderer 未設定"); return; }
        var mat = targetRenderer.material; // インスタンス化
        // URP Lit なら _BaseMap、Standard なら mainTexture どちらでも反映されます
        mat.mainTexture = tex;
        if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
    }

    // ==== B) URL読込（永続保存するならサーバに上げてURLを渡す） ====
    public void LoadFromUrl(string url)
    {
        StartCoroutine(CoLoadFromUrl(url));
    }

    private IEnumerator CoLoadFromUrl(string url)
    {
        using (var req = UnityWebRequestTexture.GetTexture(url))
        {
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(req.error);
                yield break;
            }
            var tex = DownloadHandlerTexture.GetContent(req);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            AssignTexture(tex);
        }
    }
}
