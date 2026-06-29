using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GameDirector : MonoBehaviour
{
    [SerializeField] Animator hideTransition;
    [SerializeField] int fadeTime;

    [SerializeField] GameUIController gameUIController;
    [SerializeField] PlayersController playersController;
    [SerializeField] CharacterCreater charCreater;
    [SerializeField] ColliderManager colManager;

    CharacterController[] characters;

    GAMESTATE gameState;
    enum GAMESTATE
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

        //コライダーコントローラーのアップデート管理
        colManager.UpdateMethod();

        if (gameState == GAMESTATE.PAUSE) return;

        // 一フレーム
        float delta = Time.deltaTime;


        // キャラクターのアップデート処理
        foreach (var character in characters) character.UpdateMethod(delta);

        Judge();
    }

    /*** ==================================================================================== ***/

    // ゲーム開始の準備
    async void GameStanby()
    {
        // コントローラーをGame用に変える
        ControllerInputManager.Instance.ChangeInputMode(INPUT_MODE.player);

        gameState = GAMESTATE.WAIT;
        hideTransition.gameObject.SetActive(true);

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

        // UI管理者の初期化処理
        gameUIController.Init(characters);

        
        // トランジション
        hideTransition.SetTrigger("Hide");

        await Task.Delay(fadeTime);

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

        // 勝者を記録する
        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i].GetIsDead()) continue;

            ProjectManager.Instance.SetWinner(i);
        }

        // トランジション再生 画面を隠す
        hideTransition.SetTrigger("Show");

        await Task.Delay(fadeTime);

        // リザルトシーンへ
        ProjectManager.Instance.ChangeScene(SceneName.ResultScene);
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
