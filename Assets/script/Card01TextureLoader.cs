using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class Card01TextureLoader : MonoBehaviour
{
    [Header("�����ւ��� (card01 �� Renderer)")]
    public Renderer targetRenderer;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void OpenImageFile(string goName, string methodName);
#endif

    // ==== A) WebGL: ���[�J������A�b�v���[�h�i�Z�b�V�������j ====
    private readonly StringBuilder dataUrlBuffer = new StringBuilder();

    // �A�b�v���[�h�_�C�A���O���J���i�{�^����OnClick�Ɋ��蓖�āj
    public void OpenFilePicker()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        OpenImageFile(gameObject.name, "OnDataUrlChunk");
#else
        Debug.Log("OpenFilePicker: WebGL�r���h�œ��삵�܂��iEditor/Native�ł̓_�C�A���O�͏o�܂���j");
#endif
    }

    // JS ���番���Ŕ��ł��� dataURL ����M�i"0|..." / "1|..."�j
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
        if (!targetRenderer) { Debug.LogWarning("targetRenderer ���ݒ�"); return; }
        var mat = targetRenderer.material; // �C���X�^���X��
        // URP Lit �Ȃ� _BaseMap�AStandard �Ȃ� mainTexture �ǂ���ł����f����܂�
        mat.mainTexture = tex;
        if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
    }

    // ==== B) URL�Ǎ��i�i���ۑ�����Ȃ�T�[�o�ɏグ��URL��n���j ====
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
