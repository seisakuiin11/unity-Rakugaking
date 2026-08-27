using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GameDirector : MonoBehaviour
{
    [SerializeField] Animator hideTransition;
    [SerializeField] int fadeTime;
    [SerializeField] int waitFadeTime;

    [SerializeField] int CountDownNum = 3;

    [SerializeField] GameUIController gameUIController;
    [SerializeField] PlayersController playersController;
    [SerializeField] CpuController cpuController;
    [SerializeField] CharacterCreater charCreater;
    [SerializeField] ColliderManager colManager;

    CharacterController[] characters;

    GAMESTATE gameState;
    enum GAMESTATE : byte
    {
        WAIT,
        PLAYING,
        PAUSE
    }
    void Start()
    {
        GameStanby();
    }

    void Update()
    {
        if (gameState == GAMESTATE.WAIT) return;

        // プレイヤー管理者のアップデート処理
        playersController.UpdateMethod();

        // UIのアップデート処理
        gameUIController.UpdateMethod(characters);

        if (gameState != GAMESTATE.PLAYING) return;

        // 一フレーム
        float delta = Time.deltaTime;

        // CPUのアップデート処理
        cpuController.UpdateMethod(characters);

        // キャラクターのアップデート処理
        foreach (var character in characters) character.UpdateMethod(delta);

        //コライダーコントローラーのアップデート管理
        colManager.UpdateMethod();

        Judge();
    }

    /*** ==================================================================================== ***/

    // ゲーム開始の準備
    async void GameStanby()
    {
        // コントローラーをGame用に変える
        ControllerInputManager.Instance.ChangeInputMode(INPUT_MODE.player);

        // BGM再生
        SoundManager.Instance.BGMPlay(BGM.NARBO);
        SoundManager.Instance.SEPlay(SE.BATTLE_START);

        // トランジション
        hideTransition.gameObject.SetActive(true);
        hideTransition.SetTrigger("Hide");
        SoundManager.Instance.SEPlay(SE.MEKURU);

        // キャラクター生成
        int maxPlayer = ProjectManager.Instance.MaxGamePlayerCount(); // (プレイヤー+エネミー)
        characters = charCreater.CreateCharacters(maxPlayer);

        //　各キャラクターの初期化処理
        for (int i = 0; i < characters.Length; i++)
        {
            characters[i].Init(i, colManager);
        }

        int charaCount = 0;
        List<CharacterController> playerChars = new(), enemyChars = new();
        // 人間とCPUに割り振る
        foreach (var type in ProjectManager.Instance.GetPlayerTypes())
        {
            if(type == PlayerType.NONE) continue; // 無人 飛ばす

            if (type == PlayerType.PLAYER) playerChars.Add(characters[charaCount]);
            else if (type == PlayerType.CPU) enemyChars.Add(characters[charaCount]);
            charaCount++;
        }

        // プレイヤー管理者の初期化処理
        playersController.Init(playerChars.ToArray());
        playersController.OnPause += Pause;

        // CPU管理者の初期化処理
        cpuController.Init(enemyChars.ToArray());

        // UI管理者の初期化処理
        gameUIController.Init(characters);

        // カウントダウン
        gameUIController.CountDownText(CountDownNum);

        gameState = GAMESTATE.WAIT;

        await Task.Delay(fadeTime); // 画面遷移待ち

        // キャラクターの表示
        foreach (var chara in characters) chara.TransitionAnim("Show");

        await Task.Delay(CountDownNum * 1000);

        gameState = GAMESTATE.PLAYING;
    }

    // 勝者が誕生したかジャッジする
    void Judge()
    {
        int aliveCount = 0;
        // 生存者を確認
        foreach(var character in characters) if(!character.GetIsDead()) aliveCount++;

        if(aliveCount <= 1) GameEnd();
    }

    // ゲームを終了 -> リザルトへ
    async void GameEnd()
    {
        // コントローラーをUI用に変える
        ControllerInputManager.Instance.ChangeInputMode(INPUT_MODE.ui);

        gameState = GAMESTATE.WAIT;

       
       

        playersController.OnPause -= Pause;
        gameUIController.GameEnd(characters);

       
        for (int i = 0; i < characters.Length; i++)
        {


            // 勝者ではない場合次へ
            if (characters[i].GetIsDead()) continue;


            //キャラクターのrigidbodyを止める
            characters[i].StopXMove();
           
            ProjectManager.Instance.SetWinner(i);
        }

        // SE 終了ホイッスル
        SoundManager.Instance.SEPlay(SE.BATTLE_END);

        // 少ししてから、フェイドアニメーション
        await Task.Delay(waitFadeTime);

        // トランジション再生 画面を隠す
        hideTransition.SetTrigger("Show");
        SoundManager.Instance.SEPlay(SE.MEKURU);

        await Task.Delay(fadeTime);

        // リザルトシーンへ
        ProjectManager.Instance.ChangeScene(SceneName.ResultScene);
    }

    /// <summary>
    /// 途中退出用
    /// </summary>
    public async void BackScene()
    {
        Pause(false);

        // コントローラーをUI用に変える
        ControllerInputManager.Instance.ChangeInputMode(INPUT_MODE.ui);

        gameState = GAMESTATE.WAIT;

        playersController.OnPause -= Pause;
        gameUIController.GameEnd(characters);

        // トランジション再生 画面を隠す
        hideTransition.SetTrigger("Show");
        SoundManager.Instance.SEPlay(SE.MEKURU);

        await Task.Delay(fadeTime);

        // リザルトシーンへ
        ProjectManager.Instance.ChangeScene(SceneName.SelectScene);
    }

    /// <summary>
    /// 一時停止
    /// </summary>
    public void Pause(bool flag)
    {
        if(flag)
        {
            gameState = GAMESTATE.PAUSE;
            Time.timeScale = 0f;
            gameUIController.Pause(flag);
        }
        else
        {
            gameState = GAMESTATE.PLAYING;
            Time.timeScale = 1f;
            gameUIController.Pause(flag);
        }
    }
}
