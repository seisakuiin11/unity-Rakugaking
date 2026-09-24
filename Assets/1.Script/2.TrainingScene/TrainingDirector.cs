using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TrainingDirector : MonoBehaviour
{
    [SerializeField, Header("復活までの時間")] int waitReviveTime = 1000;
    [SerializeField, Header("復活時のポジション")] Vector3 revivePos;

    [SerializeField] Animator hideTransition;
    [SerializeField] int fadeTime;

    [SerializeField] FieldController fieldController;
    [SerializeField] GameUIController gameUIController;
    [SerializeField] PlayersController playersController;
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

        // キャラクターのアップデート処理
        foreach (var character in characters)
            if(character != null) character.UpdateMethod(delta);

        //コライダーコントローラーのアップデート管理
        colManager.UpdateMethod();
    }

    /*** ==================================================================================== ***/

    // ゲーム開始の準備
    async void GameStanby()
    {
        // 各種データを取得
        var types = ProjectManager.Instance.GetPlayerTypes();
        var charaIDs = ProjectManager.Instance.GetCharaIDs();
        int maxPlayer = ProjectManager.Instance.MaxGamePlayerCount(); // (プレイヤー+エネミー)

        // BGM 再生
        SoundManager.Instance.BGMPlay(BGM.SELECT);

        // コントローラーをGame用に変える
        ControllerInputManager.Instance.ChangeInputMode(INPUT_MODE.player);

        gameState = GAMESTATE.WAIT;
        hideTransition.gameObject.SetActive(true);

        // フィールドの準備
        fieldController.SetField(maxPlayer);

        // キャラクター生成
        characters = charCreater.CreateCharacters(types, charaIDs, maxPlayer);

        //　各キャラクターの初期化処理
        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] == null) continue;

            characters[i].Init(i, colManager);
            characters[i].OnDead += OnRevive; // 死んだら復活する処理を格納
        }

        List<CharacterController> playerChars = new(), enemyChars = new();
        // 人間とCPUに割り振る
        for (int i = 0; i < types.Length; i++)
        {
            if (types[i] == PlayerType.NONE) continue; // 無人 飛ばす

            if (types[i] == PlayerType.PLAYER) playerChars.Add(characters[i]);
            else if (types[i] == PlayerType.CPU) enemyChars.Add(characters[i]);
        }

        // プレイヤー管理者の初期化処理
        playersController.Init(playerChars.ToArray());
        playersController.OnPause += Pause;

        // CPU管理者の初期化処理

        // UI管理者の初期化処理
        gameUIController.Init(characters, maxPlayer);


        // トランジション
        hideTransition.SetTrigger("Hide");
        SoundManager.Instance.SEPlay(SE.MEKURU);

        // キャラの登場アニメーション
        foreach (var chara in characters)
            if(chara != null) chara.TransitionAnim("Show");

        await Task.Delay(fadeTime);

        gameState = GAMESTATE.PLAYING;
    }

    // ゲームを終了 -> 選択画面へ
    public async void GameEnd()
    {
        Pause(false);

        // コントローラーをUI用に変える
        ControllerInputManager.Instance.ChangeInputMode(INPUT_MODE.ui);

        gameState = GAMESTATE.WAIT;

        foreach (var chara in characters)
            if(chara != null) chara.OnDead -= OnRevive;
        playersController.OnPause -= Pause;
        gameUIController.GameEnd(characters);

        // トランジション再生 画面を隠す
        hideTransition.SetTrigger("Show");
        SoundManager.Instance.SEPlay(SE.MEKURU);

        await Task.Delay(fadeTime);

        // キャラ選択シーンへ
        ProjectManager.Instance.ChangeScene(SceneName.SelectScene);
    }

    /// <summary>
    /// 一時停止
    /// </summary>
    public void Pause(bool flag)
    {
        if (flag)
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

    // イベント：復活
    void OnRevive(int num, CharacterController character)
    {
        Revive(num, character);
    }
    // 復活処理
    async void Revive(int num, CharacterController character)
    {

        await Task.Delay(waitReviveTime);

        character.Revive();
        character.transform.position = revivePos;
        gameUIController.ChangeHP(num, character.GetHP());
        character.TransitionAnim("Show");
    }
}
