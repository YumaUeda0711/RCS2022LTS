using UnityEngine;
using UnityEngine.Video;

public class CardVideoBoot : MonoBehaviour
{
    public VideoPlayer vp;
    public string relativeUrl = "videos/card_anime.mp4"; // WebGL�Ȃ瑊��URL����
    public bool loop = true;

    void Awake()
    {
        vp.source = VideoSource.Url;      // �l�C�e�B�u�Ȃ� VideoClip �ɒu��������
        vp.url = relativeUrl;
        vp.playOnAwake = false;
        vp.isLooping = loop;
        vp.audioOutputMode = VideoAudioOutputMode.None; // �~���[�g�ŋN���i�����Đ���������j
        vp.waitForFirstFrame = true;
    }

    public void Play()
    {
        if (!vp.isPrepared) vp.Prepare();
        vp.Play();
    }

    public void Stop()
    {
        vp.Stop();
    }
}
