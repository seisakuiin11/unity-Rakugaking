using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;


public class SelectDirector : MonoBehaviour
{
    const byte P1_WINDOW = 0, P2_WINDOW = 1, P3_WINDOW = 2, P4_WINDOW = 3;

    [SerializeField] PlayerJoinManager joinManager;
    [SerializeField] PlayerWindowController[] playerWindows;
    [SerializeField] Animator gameStartUI;
    [SerializeField] GameObject[] playerFrames;
    [SerializeField] CharaIconsController charaIconsCtr;
    [SerializeField] float backBtnHoldTime;
    [SerializeField] Image backArrow;
    [SerializeField] Animator hideTransition;
    [SerializeField] int fadeTime;

    bool stopProcess;               // 全体処理の実行可否（トランジションなどで活用）
    DIRECTIONDATA[] direction_old;  // 方向入力の保存用
    int[] cntrlTargetWindow;        // コントローラーがターゲットとしている枠の番号
    float[] countHoldTime;          // ボタンを押している間カウントする

    // 初期化、片付け =========================================
    private void OnEnable()
    {
        joinManager.Init();
        joinManager.OnPlayerJoined += JoinPlayer;

        // BGM再生
        SoundManager.Instance.BGMPlay(BGM.SELECT);

        int maxPlayer = ProjectManager.MaxPlayer;

        direction_old = new DIRECTIONDATA[maxPlayer];
        cntrlTargetWindow = new int[maxPlayer];
        countHoldTime = new float[maxPlayer];

        // 初期化
        for (int i = 0; i < playerWindows.Length; i++) playerWindows[i].Init(charaIconsCtr, i, LeftPlayer);

        charaIconsCtr.Init();
        charaIconsCtr.OnAccepted += AcceptCharaID;

        foreach (var frame in playerFrames) frame.SetActive(false);

        // トランジション再生
        hideTransition.gameObject.SetActive(true);
        hideTransition.SetTrigger("Hide");
        SoundManager.Instance.SEPlay(SE.MEKURU);

        // すでにコントローラーが登録されており、キャラも選択していたら ==========
        if (joinManager.GetDeviceCount() <= 0) return;

        // ウィンドウにキャラ設定
        var charaIDs = ProjectManager.Instance.GetCharaIDs();
        var playerTypes = ProjectManager.Instance.GetPlayerTypes();
        for (int i = 0; i < playerWindows.Length; i++)
        {
            var type = playerTypes[i];
            if (type == PlayerType.NONE) continue; // 設定なし

            if (type == PlayerType.PLAYER) JoinPlayer(i, type, i); // 人間なら
            else playerWindows[i].JoinPlayer(type, 0); // CPUなら

            AcceptCharaID(i, charaIDs[i]);
            if(type == PlayerType.CPU) playerWindows[i].SetActiveAccessPlayerIcon(0, false, false);
        }
    }
    private void OnDisable()
    {
        joinManager.OnPlayerJoined -= JoinPlayer;
        charaIconsCtr.OnAccepted -= AcceptCharaID;
        foreach(var window in playerWindows) window.OnLeftPlayer -= LeftPlayer;
    }
    // =======================================================

    private void Update()
    {
        if (stopProcess) return;

        // 全プレイヤーの入力情報を取得
        var inputDatas = ControllerInputManager.Instance.GetControllerUIDatas();

        // 入力情報からアクションを行う
        for (int i = 0; i < inputDatas.Length; i++)
        {
            // コントローラーが登録されていない
            if (inputDatas[i] == null) continue;

            // 戻るボタンのホールドチェック 指定時間経過後、その後の処理を行わない
            if (BackBtnChecker(i, inputDatas[i].Value.CANCEL)) break;

            // 入力データを変換
            var data = AdjustInputData(inputDatas[i].Value, i);
            var targetWindow = playerWindows[cntrlTargetWindow[i]];

            // 方向入力があって、キャラ選択中じゃなければ
            if (!targetWindow.IsSelectNow(i) && data.DIRECTION_DATA != DIRECTIONDATA.Neutral)
            {
                MoveWindow(data, i);
                targetWindow = playerWindows[cntrlTargetWindow[i]];
            }

            targetWindow.Process(data, i);

            // 全員決まっていたら、次のシーンへ
            if(IsAllAccept() && data.START) NextScene();
        }

        // 戻るボタンホールド 矢印のゲージアニメーション
        BackArrowDisp();

        // キャラ選択中の枠がないか確認 ゲームスタートUIの表示,非表示
        gameStartUI.SetBool("GameStart",IsAllAccept());
    }

    // =======================================================

    /// <summary>
    /// プレイヤーorCPUが参戦したときに呼ばれる
    /// </summary>
    /// <param name="index">プレイヤー番号</param>
    /// <param name="_playerType">プレイヤーorCPU</param>
    /// <param name="controllerNum">誰が追加をしたのか</param>
    public void JoinPlayer(int index, PlayerType _playerType, int controllerNum)
    {
        Debug.Log("参戦プレイヤーの追加");

        playerWindows[index].JoinPlayer(_playerType, controllerNum);

        cntrlTargetWindow[controllerNum] = index;
    }

    /// <summary>
    /// プレイヤーorCPUが退出したとき
    /// </summary>
    /// <param name="index"></param>
    public void LeftPlayer(int index, int playerNum)
    {
        // Cpuではなく、自枠ではないなら 不可
        int num = playerWindows[index].GetPlayerNum();
        if (num >= 0 && num != playerNum) return;

        Debug.Log("参戦プレイヤーが退出");

        var type = playerWindows[index].GetPlayerType();
        playerWindows[index].PlayerNone();

        SoundManager.Instance.SEPlay(SE.BACK);

        // 対象がプレイヤーかどうか
        if (type != PlayerType.PLAYER) return;
        // プレイヤーなら --------------------------------

        // プレイヤーが操作するアイコンを消す
        playerWindows[index].SetActiveAccessPlayerIcon(playerNum, false, false);

        // コントローラーを削除する
        joinManager.OnLeft(playerNum);
    }

    // コントローラーの入力情報を変換する
    SelectScene.InputData AdjustInputData(InputUIData inputData, int num)
    {
        SelectScene.InputData data = new SelectScene.InputData();

        data.DIRECTION_DATA = inputData.DIRECTION_DATA;
        data.DIRECTION_DATA_OLD = direction_old[num];
        data.ACCEPT = inputData.SUBMIT.IsReleased;
        data.CANCEL = inputData.CANCEL.IsReleased;
        data.START = inputData.START.IsReleased;

        // 入力情報を過去情報として保存
        direction_old[num] = inputData.DIRECTION_DATA;

        return data;
    }

    // ウィンドウ間の移動
    void MoveWindow(SelectScene.InputData data, int index)
    {
        var targetWindowNum = cntrlTargetWindow[index];
        playerWindows[targetWindowNum].SetActiveAccessPlayerIcon(index, false, false);
        var dir = data.DIRECTION_DATA;

        switch (targetWindowNum)
        {
            case P1_WINDOW: // 1Pウィンドウなら
                if (dir.HasFlag(DIRECTIONDATA.RIGHT)) targetWindowNum++;
                if (dir.HasFlag(DIRECTIONDATA.DOWN)) targetWindowNum += 2;
                break;

            case P2_WINDOW: // 2Pウィンドウなら
                if (dir.HasFlag(DIRECTIONDATA.LEFT)) targetWindowNum--;
                if (dir.HasFlag(DIRECTIONDATA.DOWN)) targetWindowNum += 2;
                break;

            case P3_WINDOW: // 3Pウィンドウなら
                if (dir.HasFlag(DIRECTIONDATA.RIGHT)) targetWindowNum++;
                if (dir.HasFlag(DIRECTIONDATA.UP)) targetWindowNum -= 2;
                break;

            case P4_WINDOW: // 4Pウィンドウなら
                if (dir.HasFlag(DIRECTIONDATA.LEFT)) targetWindowNum--;
                if (dir.HasFlag(DIRECTIONDATA.UP)) targetWindowNum -= 2;
                break;

            default: break;
        }

        // SE再生
        if (targetWindowNum != cntrlTargetWindow[index]) SoundManager.Instance.SEPlay(SE.CURSOR_MOVE);

        cntrlTargetWindow[index] = targetWindowNum;
        playerWindows[targetWindowNum].SetActiveAccessPlayerIcon(index, true, false);
    }

    // キャラ選択 決定
    void AcceptCharaID(int index, int id)
    {
        playerWindows[index].Accept(id);
        charaIconsCtr.Accept(index);

        SoundManager.Instance.SEPlay(SE.CHARA_ACCEPT);
    }

    // 全員キャラ選択が決まったかどうか
    bool IsAllAccept()
    {
        int count = 0;

        // ひとりでも選択中なら、ダメ
        foreach (var window in playerWindows)
            if (window.IsSelectNow()) return false;

        // ひとりも参加していない、ダメ
        foreach (var window in playerWindows)
            if (window.GetCharaID() >= 0) count++;

        return count >= 2; // 2人以上の参加
    }

    // 次のシーンへ進む
    async void NextScene()
    {
        // 入力に関する処理をすべて止める
        stopProcess = true;

        // 最大人数
        int maxPlayer = ProjectManager.MaxPlayer;

        // キャラIDと操作プレイヤーのタイプ
        int[] charaIDs = new int[maxPlayer];
        PlayerType[] playerTypes = new PlayerType[maxPlayer];

        // 各ウィンドウから情報取得
        for(int i = 0; i < playerWindows.Length; i++)
        {
            charaIDs[i] = playerWindows[i].GetCharaID();
            playerTypes[i] = playerWindows[i].GetPlayerType();
        }

        // 各プレイヤーの選択したキャラIDを渡す
        ProjectManager.Instance.SetCharaIDs(charaIDs, playerTypes);

        Debug.Log("次のシーンへ");

        hideTransition.SetTrigger("Show");

        SoundManager.Instance.SEPlay(SE.MEKURU);

        await Task.Delay(fadeTime);

        // 次のシーンへ（予約済みのシーンへ）
        var scene = ProjectManager.Instance.NextScene;
        ProjectManager.Instance.ChangeScene(scene);
    }

    // ホールド時間を確認して、タイトルに戻るか調べる
    bool BackBtnChecker(int index, ButtonState backBtn)
    {
        // 押していないなら、戻る
        if (!backBtn.IsHeld)
        {
            countHoldTime[index] = 0f;
            return false;
        }

        // 経過時間をカウントする
        countHoldTime[index] += Time.deltaTime;

        // ホールド時間に達していない
        if (countHoldTime[index] < backBtnHoldTime) return false;

        // タイトルに戻る
        BackScene();

        return true;
    }

    async void BackScene()
    {
        // 入力に関する処理をすべて止める
        stopProcess = true;

        SoundManager.Instance.SEPlay(SE.BACK);

        // コントローラーをすべて削除する
        for(int i = 0; i < playerWindows.Length; i++)
        {
            if (playerWindows[i].GetPlayerType() != PlayerType.PLAYER) continue;

            // コントローラーを削除する
            joinManager.OnLeft(i);
        }

        Debug.Log("前のシーンへ");

        hideTransition.SetTrigger("Show");
        SoundManager.Instance.SEPlay(SE.MEKURU);

        await Task.Delay(fadeTime);

        // 次のシーンへ（予約済みのシーンへ）
        ProjectManager.Instance.ChangeScene(SceneName.TitleScene);
    }

    void BackArrowDisp()
    {
        // 一番長いホールド時間を取得
        float time = 0f;
        foreach(var _time in countHoldTime)
        {
            if (_time <= time) continue;

            time = _time;
        }

        backArrow.fillAmount = time / backBtnHoldTime;
    }
}
