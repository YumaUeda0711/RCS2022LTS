using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class Card02TextureLoader : MonoBehaviour
{
    [Header("差し替え先 Renderers")]
    public Renderer baseRenderer;   // card02 の本体 (例: card02/CardMesh)
    public Renderer hakuRenderer;   // 箔ポリ (例: card02/card _haku)

    private enum Target { None, Base, Haku }
    private Target pendingTarget = Target.None;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void OpenImageFile(string goName, string methodName);
#endif

    // ===== WebGL: ローカルからアップロード =====
    private readonly StringBuilder buf = new StringBuilder();

    // ボタン用：ベース画像アップロード
    public void OpenFilePickerBase()
    {
        pendingTarget = Target.Base;
#if UNITY_WEBGL && !UNITY_EDITOR
        OpenImageFile(gameObject.name, "OnDataUrlChunk");
#else
        Debug.Log("OpenFilePickerBase: WebGLビルドで動作します。");
#endif
    }

    // ボタン用：箔テクスチャアップロード
    public void OpenFilePickerHaku()
    {
        pendingTarget = Target.Haku;
#if UNITY_WEBGL && !UNITY_EDITOR
        OpenImageFile(gameObject.name, "OnDataUrlChunk");
#else
        Debug.Log("OpenFilePickerHaku: WebGLビルドで動作します。");
#endif
    }

    // JSからの受信。形式: "meta|{...}" or "data|{0/1}|<chunk>"
    public void OnDataUrlChunk(string payload)
    {
        if (payload.StartsWith("meta|"))
        {
            buf.Clear(); // 念のため初期化
            return;
        }
        if (!payload.StartsWith("data|")) return;

        // "data|<0/1>|<chunk>"
        int sep1 = payload.IndexOf('|', 5);
        if (sep1 < 0) return;
        bool isLast = payload[5] == '1';
        string chunk = payload.Substring(sep1 + 1);

        buf.Append(chunk);
        if (isLast)
        {
            string dataUrl = buf.ToString();
            buf.Clear();
            ApplyDataUrlToPending(dataUrl);
        }
    }

    private void ApplyDataUrlToPending(string dataUrl)
    {
        try
        {
            int comma = dataUrl.IndexOf(',');
            if (comma < 0) throw new Exception("Invalid dataURL.");
            string base64 = dataUrl.Substring(comma + 1);
            byte[] bytes = Convert.FromBase64String(base64);

            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false, false); // sRGB
            tex.LoadImage(bytes, markNonReadable: true);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            switch (pendingTarget)
            {
                case Target.Base: AssignTexture(baseRenderer, tex); break;
                case Target.Haku: AssignTexture(hakuRenderer, tex); break;
                default: Debug.LogWarning("No pending target."); break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"ApplyDataUrl error: {e.Message}");
        }
        finally
        {
            pendingTarget = Target.None;
        }
    }

    private void AssignTexture(Renderer r, Texture2D tex)
    {
        if (!r) { Debug.LogWarning("Renderer 未設定"); return; }
        var mat = r.material;                 // 共有を汚さないようインスタンス化
        mat.mainTexture = tex;                // Standard 互換
        if (mat.HasProperty("_BaseMap"))      // URP Lit 互換
            mat.SetTexture("_BaseMap", tex);
    }

    // ===== URLから読込（永続運用向け） =====
    public void LoadBaseFromUrl(string url)  => StartCoroutine(CoLoadUrl(url, Target.Base));
    public void LoadHakuFromUrl(string url)  => StartCoroutine(CoLoadUrl(url, Target.Haku));

    private IEnumerator CoLoadUrl(string url, Target t)
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

            if (t == Target.Base) AssignTexture(baseRenderer, tex);
            else if (t == Target.Haku) AssignTexture(hakuRenderer, tex);
        }
    }
}
