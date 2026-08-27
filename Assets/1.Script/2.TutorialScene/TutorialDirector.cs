using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialDirector : MonoBehaviour
{
    const byte Player = 0, CPU = 1, MaxPlayer = 2;

    [SerializeField, Header("復活までの時間")] int waitReviveTime = 1000;
    [SerializeField, Header("復活時のポジション")] Vector3 revivePos;

    [SerializeField] Animator hideTransition;
    [SerializeField] int fadeTime;

    [Header("チュートリアル情報")]
    [SerializeField] float waitStateTime;           // 各チュートリアルStateの最低継続時間
    [SerializeField] TextMeshProUGUI tutorialText;  // チュートリアルテキスト
    [SerializeField] Animator tutorialTextAnim;     // チュートリアルテキスト アニメーション
    [SerializeField,TextArea] string tutorialText_Move;      // チュートリアルテキスト 移動,ジャンプ
    [SerializeField,TextArea] string tutorialText_Attack;    // チュートリアルテキスト 攻撃
    [SerializeField, TextArea] string tutorialText_Shield;    // チュートリアルテキスト ガード
    [SerializeField, TextArea] string tutorialText_Free;      // チュートリアルテキスト フリー
    [SerializeField] int attackClearCount = 5;      // 達成目標 ダメージを与えた回数

    [Space]
    [SerializeField] GameUIController gameUIController;
    [SerializeField] CharacterCreater charCreater;
    [SerializeField] ColliderManager colManager;

    [SerializeField] PlayerInput controllerPrefab = default;
    [SerializeField] InputAction playerJoinInputAction = default;

    CharacterController[] characters;
    bool joinPlayer;    // プレイヤーが接続しているか
    float stateTime;    // 各Stateの継続時間
    bool moveFlag;      // 移動したか
    bool jumpFlag;      // ジャンプしたか
    int attackFlag;     // 何回攻撃してダメージを与えたか

    GAMESTATE gameState;
    enum GAMESTATE : byte
    {
        WAIT,
        PLAYING,
        PAUSE
    }

    TUTORIALSTATE tutorialState;
    [System.Flags]
    enum TUTORIALSTATE
    {
        NONE = 0,
        MOVE = 1 << 0,
        ATTACK = 1 << 2,
        SHIELD = 1 << 3,
        FREE = 1 << 4,
    }
    const TUTORIALSTATE Tutorial_Move = TUTORIALSTATE.MOVE;
    const TUTORIALSTATE Tutorial_Attack = TUTORIALSTATE.MOVE | TUTORIALSTATE.ATTACK;
    const TUTORIALSTATE Tutorial_Shield = TUTORIALSTATE.MOVE | TUTORIALSTATE.ATTACK | TUTORIALSTATE.SHIELD;
    const TUTORIALSTATE Tutorial_Free = TUTORIALSTATE.MOVE | TUTORIALSTATE.ATTACK | TUTORIALSTATE.SHIELD | TUTORIALSTATE.FREE;

    void Start()
    {
        playerJoinInputAction.Enable();
        playerJoinInputAction.performed += OnJoin;

        GameStanby();
    }
    private void OnDestroy()
    {
        playerJoinInputAction.performed -= OnJoin;
    }

    void Update()
    {
        if (gameState == GAMESTATE.WAIT) return;

        // ポーズ中の操作
        if (gameState == GAMESTATE.PAUSE) UIPlayUpdate();

        // UIのアップデート処理
        gameUIController.UpdateMethod(characters);

        if (gameState != GAMESTATE.PLAYING) return;

        // 入力情報を取得 キャラクターに渡す チュートリアル判定
        GamePlayUpdate();

        // 一フレーム
        float delta = Time.deltaTime;

        // キャラクターのアップデート処理
        foreach (var character in characters) character.UpdateMethod(delta);

        //コライダーコントローラーのアップデート管理
        colManager.UpdateMethod();
    }

    /*** ==================================================================================== ***/

    // ゲーム開始の準備
    async void GameStanby()
    {
        // BGM 再生
        SoundManager.Instance.BGMPlay(BGM.SELECT);

        // コントローラーをGame用に変える
        ControllerInputManager.Instance.ChangeInputMode(INPUT_MODE.player);

        gameState = GAMESTATE.WAIT;
        hideTransition.gameObject.SetActive(true);

        // キャラクター生成
        int[] charaIDs = new int[MaxPlayer] { 0, 1 };
        PlayerType[] playerTypes = new PlayerType[MaxPlayer] { PlayerType.PLAYER, PlayerType.CPU };
        ProjectManager.Instance.SetCharaIDs(charaIDs, playerTypes);
        characters = charCreater.CreateCharacters(MaxPlayer); // プレイヤーとCPU(的)

        //　各キャラクターの初期化処理
        for (int i = 0; i < characters.Length; i++)
        {
            characters[i].Init(i, colManager);
            characters[i].OnDead += OnRevive; // 死んだら復活する処理を格納
        }

        characters[CPU].OnDamage += OnDamage;

        // UI管理者の初期化処理
        gameUIController.Init(characters);
        tutorialText.text = "";

        // トランジション
        hideTransition.SetTrigger("Hide");
        SoundManager.Instance.SEPlay(SE.MEKURU);

        foreach (var chara in characters) chara.TransitionAnim("Show");

        await Task.Delay(fadeTime);

        gameState = GAMESTATE.PLAYING;
        TutorialMoveInit();
    }

    // ゲームを終了 -> 選択画面へ
    public async void GameEnd()
    {
        Pause(false);

        // コントローラーを削除する
        ControllerInputManager.Instance.RemoveController(Player);

        gameState = GAMESTATE.WAIT;

        foreach (var chara in characters) { chara.OnDead -= OnRevive; chara.OnDamage -= OnDamage; }
        gameUIController.GameEnd(characters);

        // トランジション再生 画面を隠す
        hideTransition.SetTrigger("Show");
        SoundManager.Instance.SEPlay(SE.MEKURU);

        await Task.Delay(fadeTime);

        // キャラ選択シーンへ
        ProjectManager.Instance.ChangeScene(SceneName.TitleScene);
    }

    // ゲーム操作アップデート ===========================================
    void GamePlayUpdate()
    {
        // 全プレイヤーの入力情報を取得
        var inputDatas = ControllerInputManager.Instance.GetControllerDatas();

        if (inputDatas[Player] == null) return;

        // データ変換 (入力情報 → コマンド情報)
        var data = GetCommandData(inputDatas[Player].Value, out bool pause);
        // コマンド情報を渡す
        characters[Player].SetCommandData(data);

        // ポーズボタンを押していたら
        if (pause) Pause(true); // 一時停止

        // チュートリアル判定
        // 移動 ジャンプ
        if (tutorialState == Tutorial_Move) TutorialMove(data);
        // 攻撃
        else if (tutorialState == Tutorial_Attack) TutorialAttack(data);
        // ガード
        else if (tutorialState == Tutorial_Shield) TutorialShield(data);
    }

    // チュートリアル
    // 移動 ジャンプ
    void TutorialMoveInit()
    {
        tutorialState = Tutorial_Move;
        stateTime = waitStateTime;
        tutorialText.text = tutorialText_Move;
        tutorialTextAnim.SetTrigger("Show");
    }
    void TutorialMove(InputCommandData data)
    {
        stateTime -= Time.deltaTime;

        // 移動したか
        if (data.DIRECTION_VEC2.x != 0) moveFlag = true;

        // ジャンプしたか
        if (data.DIRECTION_DATA.HasFlag(DIRECTIONDATA.UP)) jumpFlag = true;

        // 継続時間が終わり、クリア項目を達成していたら
        if (stateTime < 0 && moveFlag && jumpFlag)
        {
            TutorialAttackInit();
        }
    }

    // 攻撃 - ダメージ
    async void TutorialAttackInit()
    {
        tutorialState = Tutorial_Attack;
        stateTime = waitStateTime;
        // 非表示
        tutorialTextAnim.SetTrigger("Hide");

        await Task.Delay(fadeTime);

        tutorialText.text = tutorialText_Attack;
        // 表示
        tutorialTextAnim.SetTrigger("Show");
    }
    void TutorialAttack(InputCommandData data)
    {
        stateTime -= Time.deltaTime;

        // 継続時間が終わり、クリア項目を達成していたら
        if (stateTime < 0 && attackFlag > attackClearCount)
        {
            TutorialShieldInit();
        }
    }
    void OnDamage(int playerNum, int hp)
    {
        attackFlag++;
    }

    // ガード
    async void TutorialShieldInit()
    {
        tutorialState = Tutorial_Shield;
        stateTime = waitStateTime;
        // 非表示
        tutorialTextAnim.SetTrigger("Hide");

        await Task.Delay(fadeTime);

        tutorialText.text = tutorialText_Shield;
        // 表示
        tutorialTextAnim.SetTrigger("Show");
    }
    void TutorialShield(InputCommandData data)
    {
        stateTime -= Time.deltaTime;

        // 継続時間が終わり、クリア項目を達成していたら
        if (stateTime < 0)
        {
            TutorialFreeInit();
        }
    }

    // フリー
    async void TutorialFreeInit()
    {
        tutorialState = Tutorial_Free;
        stateTime = waitStateTime;
        // 非表示
        tutorialTextAnim.SetTrigger("Hide");

        await Task.Delay(fadeTime);

        tutorialText.text = tutorialText_Free;
        // 表示
        tutorialTextAnim.SetTrigger("Show");
    }

    // ポーズ時のUI操作
    void UIPlayUpdate()
    {
        bool pause = false; // ポーズフラグ

        var data = ControllerInputManager.Instance.GetControllerUIDatas();

        pause |= data[Player].Value.MENU.IsPressed;
        pause |= data[Player].Value.CANCEL.IsPressed;

        if (pause) Pause(false); // 一時停止を解除
    }

    /// <summary>
    /// 一時停止
    /// </summary>
    public void Pause(bool flag)
    {
        if (flag)
        {
            gameState = GAMESTATE.PAUSE;
            ControllerInputManager.Instance.ChangeInputMode(INPUT_MODE.ui);
            Time.timeScale = 0f;
            gameUIController.Pause(flag);
        }
        else
        {
            gameState = GAMESTATE.PLAYING;
            ControllerInputManager.Instance.ChangeInputMode(INPUT_MODE.player);
            Time.timeScale = 1f;
            gameUIController.Pause(flag);
        }
        Debug.Log("ポーズ");
    }

    // コントローラーの追加（入室）
    // 入室
    private void OnJoin(InputAction.CallbackContext context)
    {
        // プレイヤーが入室していたら
        if (joinPlayer) return;

        joinPlayer = true;

        // コントローラー生成
        var controller = PlayerInput.Instantiate(
            prefab: controllerPrefab.gameObject,
            playerIndex: 0,
            pairWithDevice: context.control.device
            );

        GameObject obj = controller.gameObject;
        Debug.Log("join", obj);
        ControllerInputManager.Instance.AddNewController(obj);

        ControllerInputManager.Instance.ChangeInputMode(INPUT_MODE.player);
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

    // 入力制限
    // データ変換 (入力情報 → コマンド情報)
    InputCommandData GetCommandData(InputGameData _data, out bool pause)
    {
        var data = new InputCommandData();

        // 移動 ジャンプ
        if (tutorialState.HasFlag(TUTORIALSTATE.MOVE))
        {
            data.DIRECTION_DATA = _data.DIRECTION_DATA;
            data.DIRECTION_VEC2 = _data.DIRECTION_VEC2;
            data.JUMP = _data.JUMP.now;
        }
        // 攻撃
        if (tutorialState.HasFlag(TUTORIALSTATE.ATTACK))
        {
            data.ATTACK_UP = _data.ATTACK_UP.now;
            data.ATTACK_UP_OLD = _data.ATTACK_UP.past;
            data.ATTACK_DOWN = _data.ATTACK_DOWN.now;
            data.ATTACK_LEFT = _data.ATTACK_LEFT.now;
            data.ATTACK_RIGHT = _data.ATTACK_RIGHT.now;
        }
        // ガード
        if (tutorialState.HasFlag(TUTORIALSTATE.SHIELD))
        {
            data.SHIELD = _data.SHIELD.now;
        }
        // ポーズ
        pause = _data.PAUSE;

        return data;
    }
}
