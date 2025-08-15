using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class Card03TextureLoader : MonoBehaviour
{
    [Header("card03 の差し替え先")]
    public Renderer targetRenderer; // 例: card03 / CardMesh の MeshRenderer

    void Awake()
    {
        // SendMessage 宛先の取り違いを防ぐため固有名に
        gameObject.name = "UploadController03";
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void OpenImageFile(string goName, string methodName);
#endif

    // ---- ボタンから呼ぶ ----
    public void OpenFilePicker()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        OpenImageFile(gameObject.name, "OnDataUrlChunk");
#else
        Debug.Log("OpenFilePicker: WebGLビルドで動作します。");
#endif
    }

    // ---- JSから受信（meta/data 形式）----
    private readonly StringBuilder _buf = new StringBuilder();

    // "meta|{...}" または "data|0|<chunk>" / "data|1|<chunk>"（1 が最後）
    public void OnDataUrlChunk(string payload)
    {
        if (string.IsNullOrEmpty(payload)) return;

        if (payload.StartsWith("meta|"))
        {
            _buf.Clear(); // 念のため初期化
            return;
        }
        if (!payload.StartsWith("data|")) return;

        int sep1 = payload.IndexOf('|', 5);
        if (sep1 < 0) return;

        bool isLast = payload[5] == '1';
        string chunk = payload.Substring(sep1 + 1);

        _buf.Append(chunk);

        if (isLast)
        {
            string dataUrl = _buf.ToString();
            _buf.Clear();
            ApplyDataUrl(dataUrl);
        }
    }

    // ---- dataURL → Texture2D → 適用 ----
    private void ApplyDataUrl(string dataUrl)
    {
        try
        {
            int comma = dataUrl.IndexOf(',');
            if (comma < 0) throw new Exception("Invalid data URL");
            string base64 = dataUrl.Substring(comma + 1);
            byte[] bytes = Convert.FromBase64String(base64);

            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false, false); // sRGB
            tex.LoadImage(bytes, markNonReadable: true);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            AssignTexture(targetRenderer, tex);
        }
        catch (Exception e)
        {
            Debug.LogError($"[Card03] ApplyDataUrl error: {e.Message}");
        }
    }

    // 共有マテでも“このRendererだけ”に安全適用（MaterialPropertyBlock）
    private static readonly int _MainTex = Shader.PropertyToID("_MainTex");
    private static readonly int _BaseMap = Shader.PropertyToID("_BaseMap");
    private MaterialPropertyBlock _mpb;

    private void AssignTexture(Renderer r, Texture tex)
    {
        if (!r) { Debug.LogWarning("[Card03] targetRenderer 未設定"); return; }
        if (_mpb == null) _mpb = new MaterialPropertyBlock();
        r.GetPropertyBlock(_mpb);
        _mpb.SetTexture(_MainTex, tex);   // Standard
        _mpb.SetTexture(_BaseMap, tex);   // URP Lit
        r.SetPropertyBlock(_mpb);
    }

    // （任意）URLから差し替えたい時
    public void LoadFromUrl(string url) => StartCoroutine(CoLoad(url));
    private System.Collections.IEnumerator CoLoad(string url)
    {
        using (var req = UnityWebRequestTexture.GetTexture(url))
        {
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success) { Debug.LogError(req.error); yield break; }
            var tex = DownloadHandlerTexture.GetContent(req);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            AssignTexture(targetRenderer, tex);
        }
    }
}
