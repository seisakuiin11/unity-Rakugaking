using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ResultDirector : MonoBehaviour
{
    [SerializeField] Image winnerImg;
    [SerializeField] Animator playerAnim;
    [SerializeField] Animator transitionAnim;
    [SerializeField,Tooltip("ミリ秒")] int TransitionAnimTime;

    bool waitFlag;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        if (waitFlag) return;

        // 入力情報を取得
        var data = ControllerInputManager.Instance.GetControllerUIDatas();

        // 誰か一人でもボタンを押していたら
        bool flag = false;
        foreach (var dt in data) {
            if(dt == null) continue;
            flag |= dt.Value.SUBMIT.IsReleased;
        }

        if(flag) NextScene();
    }

    // 初期化
    void Init()
    {
        // 勝者のプレイヤー番号とキャラIDを取得
        ProjectManager.Instance.GetWinner(out var playerIndex, out var charaNum);

        winnerImg.sprite = CharaDataManager.Instance.GetVisualData(charaNum).KeyVisual;
        playerAnim.SetInteger("Player", playerIndex);

        // キャラID取得
        var charaIDs = ProjectManager.Instance.GetCharaIDs();
        int count = 0;
        // 全プレイヤーのキャラアイコンの用意
        for(int i = 0; i < charaIDs.Length; i++)
        {
            if (charaIDs[i] < 0) continue; // -1除外

            if (i == playerIndex) continue; // 勝者のアイコンは用意しない

            int id = charaIDs[i];
            count++;
        }

        // BGM再生
        SoundManager.Instance.BGMPlay(BGM.RESULT);
        SoundManager.Instance.SEPlay(SE.RESULT_JINGLE);

        // トランジション再生 画面表示
        transitionAnim.gameObject.SetActive(true);
        transitionAnim.SetTrigger("Hide");
        SoundManager.Instance.SEPlay(SE.MEKURU);
    }

    // 次のシーンへ
    async void NextScene()
    {
        waitFlag = true;

        transitionAnim.SetTrigger("Show");
        SoundManager.Instance.SEPlay(SE.MEKURU);

        await Task.Delay(TransitionAnimTime);

        ProjectManager.Instance.ChangeScene(SceneName.SelectScene);
    }
}
