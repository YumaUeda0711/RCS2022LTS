using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class Card03TextureLoader : MonoBehaviour
{
    [Header("card03 �̍����ւ���")]
    public Renderer targetRenderer; // ��: card03 / CardMesh �� MeshRenderer

    void Awake()
    {
        // SendMessage ����̎��Ⴂ��h�����ߌŗL����
        gameObject.name = "UploadController03";
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void OpenImageFile(string goName, string methodName);
#endif

    // ---- �{�^������Ă� ----
    public void OpenFilePicker()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        OpenImageFile(gameObject.name, "OnDataUrlChunk");
#else
        Debug.Log("OpenFilePicker: WebGL�r���h�œ��삵�܂��B");
#endif
    }

    // ---- JS�����M�imeta/data �`���j----
    private readonly StringBuilder _buf = new StringBuilder();

    // "meta|{...}" �܂��� "data|0|<chunk>" / "data|1|<chunk>"�i1 ���Ō�j
    public void OnDataUrlChunk(string payload)
    {
        if (string.IsNullOrEmpty(payload)) return;

        if (payload.StartsWith("meta|"))
        {
            _buf.Clear(); // �O�̂��ߏ�����
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

    // ---- dataURL �� Texture2D �� �K�p ----
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

    // ���L�}�e�ł��g����Renderer�����h�Ɉ��S�K�p�iMaterialPropertyBlock�j
    private static readonly int _MainTex = Shader.PropertyToID("_MainTex");
    private static readonly int _BaseMap = Shader.PropertyToID("_BaseMap");
    private MaterialPropertyBlock _mpb;

    private void AssignTexture(Renderer r, Texture tex)
    {
        if (!r) { Debug.LogWarning("[Card03] targetRenderer ���ݒ�"); return; }
        if (_mpb == null) _mpb = new MaterialPropertyBlock();
        r.GetPropertyBlock(_mpb);
        _mpb.SetTexture(_MainTex, tex);   // Standard
        _mpb.SetTexture(_BaseMap, tex);   // URP Lit
        r.SetPropertyBlock(_mpb);
    }

    // �i�C�ӁjURL���獷���ւ�������
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
