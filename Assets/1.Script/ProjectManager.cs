using UnityEngine;
using System.Linq;


/// <summary>
/// いない、プレイヤー、CPUを判別する
/// </summary>
public enum PlayerType : byte
{
    NONE,
    PLAYER,
    CPU
}

/// <summary>
/// プロジェクト全体を管理している統括者
/// </summary>
public class ProjectManager : MonoBehaviour
{
    /// <summary> 最大プレイヤー人数 </summary>
    public const int MaxPlayer = 4;

    public static ProjectManager Instance { get; private set; }

    /// <summary> 次の次のシーンを予約する用 </summary>
    public SceneName NextScene;

    [SerializeField] int[] charaIDs;            // 選択したキャラたちのIDが格納させる
    [SerializeField] PlayerType[] playerTypes;  // ○Pが人間かCPUか
    [SerializeField] int winnerIndex;           // 勝者

    [SerializeField] SoundManager soundManager;  // サウンド管理者
    [SerializeField] GameObject backImg;

    SceneLoder sceneLoder;      // シーン管理者


    private async void Awake()
    {
        backImg.SetActive(true); // 背景画像表示

        //すでにほかのContollerInputManagerがある場合は削除
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        //シングルトン化
        Instance = this;
        DontDestroyOnLoad(gameObject);

        //フレームレート固定
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        // 解像度設定
        Screen.SetResolution(1920, 1080, true);

        sceneLoder = new SceneLoder();

        SceneName _scene = NextScene;
#if UNITY_EDITOR
        // デバッグ用 ゲームシーンスタートなら
        if (NextScene > SceneName.SelectScene) _scene = SceneName.JoinScene;
#else
        // 初期化 - タイトルスタート
        NextScene = SceneName.TitleScene;
        _scene = SceneName.TitleScene;
#endif

        // サウンド素材のロード
        await soundManager.Init(_scene);

        // シーンを呼び出す
        ChangeScene(_scene);
    }

    /// <summary>
    /// 指定シーンに切り替える
    /// </summary>
    public void ChangeScene(SceneName sceneName)
    {
        sceneLoder.ChangeScene(sceneName);
    }

    /// <summary>
    /// キャラID配列をProjectManagerに渡す
    /// </summary>
    public void SetCharaIDs(int[] _charaIDs, PlayerType[] _playerTypes)
    {
        charaIDs = _charaIDs.ToArray();
        playerTypes = _playerTypes.ToArray();
    }

    /// <summary> ゲームに参加するキャラクター人数 </summary>
    public int MaxGamePlayerCount()
    {
        int count = 0;

        foreach (var type in playerTypes)
            if(type != PlayerType.NONE) count++;

        return count;
    }
    /// <summary>
    /// キャラIDをProjectManagerから受け取る
    /// </summary>
    public int[] GetCharaIDs() { return charaIDs; }
    public PlayerType[] GetPlayerTypes() { return playerTypes; }

    /// <summary>
    /// 勝者のプレイヤー番号を格納
    /// </summary>
    public void SetWinner(int winner) => winnerIndex = winner;

    /// <summary>
    /// 勝者のプレイヤー番号を渡す
    /// </summary>
    public bool GetWinner(out int playerIndex, out int charaNum)
    {
        playerIndex = 0;
        charaNum = 0;

        if(winnerIndex < 0) return false;

        playerIndex = winnerIndex;
        charaNum = charaIDs[winnerIndex];

        return true;
    }
}
