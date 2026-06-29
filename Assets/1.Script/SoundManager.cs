using UnityEngine;

public enum BGM
{
    TITLE,
    SELECT,
    GAME,
    RESULT
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] AudioSource BGMPlayer;
    [SerializeField] AudioSource SEPlayer;

    [SerializeField] AudioClip[] bgms;
    [SerializeField] AudioClip[] ses;


    private void Awake()
    {
        //‚·‚Å‚É‚Ù‚©‚ÌContollerInputManager‚ª‚ ‚éê‡‚Ííœ
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        //ƒVƒ“ƒOƒ‹ƒgƒ“‰»
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void BGMPlay(BGM bgm)
    {
        BGMPlayer.resource = bgms[(int)bgm];
    }
}
