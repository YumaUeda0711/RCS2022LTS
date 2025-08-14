using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class Card01TextureLoader : MonoBehaviour
{
    [Header("card01 �̍����ւ���")]
    public Renderer targetRenderer;

    // �C�ӁF�����Փˉ���̂��ߌŒ薼��
    void Awake() { gameObject.name = "UploadController01"; }

#if UNITY_WEBGL && !UNITY_EDITOR
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void OpenImageFile(string goName, string methodName);
#endif

    // ===== �{�^������Ă� =====
    public void OpenFilePicker()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        OpenImageFile(gameObject.name, "OnDataUrlChunk"); // JS��OpenImageFile(go, method)�ƈ�v
#else
        Debug.Log("OpenFilePicker: WebGL�r���h�œ��삵�܂��B");
#endif
    }

    // ===== JS����̎�M�imeta/data �`���j =====
    private readonly StringBuilder _buf = new StringBuilder();

    // �`��:
    //  "meta|{...}"  �i�C�Ӂj
    //  "data|0|<chunk>" or "data|1|<chunk>" �i1 ���Ō�j
    public void OnDataUrlChunk(string payload)
    {
        if (string.IsNullOrEmpty(payload)) return;

        if (payload.StartsWith("meta|"))
        {
            // �K�v�Ȃ烁�^������ifile name/type/size �Ȃǁj
            //_ = payload.Substring(5);
            _buf.Clear();
            return;
        }
        if (!payload.StartsWith("data|")) return;

        // "data|<0/1>|<chunk>"
        //           ^ index=5 �� '0' or '1'
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

    // ===== dataURL �� Texture2D �� �K�p =====
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
            Debug.LogError($"[Card01] ApplyDataUrl error: {e.Message}");
        }
    }

    // ���L�}�e���A���ł��g����Renderer�����h�ɓK�p
    private static readonly int _MainTex = Shader.PropertyToID("_MainTex");
    private static readonly int _BaseMap = Shader.PropertyToID("_BaseMap");
    private MaterialPropertyBlock _mpb;
    private void AssignTexture(Renderer r, Texture tex)
    {
        if (!r) { Debug.LogWarning("[Card01] targetRenderer ���ݒ�"); return; }
        if (_mpb == null) _mpb = new MaterialPropertyBlock();
        r.GetPropertyBlock(_mpb);
        _mpb.SetTexture(_MainTex, tex);   // Standard
        _mpb.SetTexture(_BaseMap, tex);   // URP Lit
        r.SetPropertyBlock(_mpb);
    }

    //�i�Q�l�jURL�Ǎ��e�X�g
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
