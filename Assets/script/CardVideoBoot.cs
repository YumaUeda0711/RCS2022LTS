using UnityEngine;
using UnityEngine.Video;

public class CardVideoBoot : MonoBehaviour
{
    public VideoPlayer vp;
    public string relativeUrl = "videos/card_anime.mp4"; // WebGLなら相対URL推奨
    public bool loop = true;

    void Awake()
    {
        vp.source = VideoSource.Url;      // ネイティブなら VideoClip に置き換え可
        vp.url = relativeUrl;
        vp.playOnAwake = false;
        vp.isLooping = loop;
        vp.audioOutputMode = VideoAudioOutputMode.None; // ミュートで起動（自動再生制限回避）
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
