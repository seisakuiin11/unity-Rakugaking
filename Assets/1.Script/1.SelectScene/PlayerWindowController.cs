using SelectScene;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class PlayerWindowController : MonoBehaviour
{
    [SerializeField, Header("ウィンドウ")] Image window;
    [Header("選択中のテキスト群")]
    [SerializeField] GameObject textSelectArea;
    [SerializeField] Image charaImg_Select;
    [SerializeField] TextMeshProUGUI nameText_Select;
    [SerializeField] TextMeshProUGUI[] skillNameText_Select;
    [SerializeField] TextMeshProUGUI[] skillExplanationText_Select;
    [Header("決定後のテキスト群+キャラImage")]
    [SerializeField] GameObject textAcceptArea;
    [SerializeField] TextMeshProUGUI nameText_Accept;
    [SerializeField] TextMeshProUGUI[] skillNameText_Accept;
    [SerializeField] Image charaImg;

    [SerializeField, Header("アクセスプレイヤーアイコン")] Animator[] accessPlayerIcons;
    [SerializeField, Header("決定アイコン")] GameObject acceptImg;

    [Header("素材")]
    [SerializeField] Sprite noneWindow;
    [SerializeField] Sprite playerWindow;
    [SerializeField] Sprite cpuWindow;

    CharaIconsController charaSelecter;

    /// <summary> プレイヤー不参加処理 </summary>
    public event Action<int,int> OnLeftPlayer;

    int index;
    PlayerType playerType;
    int charaID;
    int playerNum = -1;
    int accessPlayer = -1;

    public void Init(CharaIconsController _charaSelecter, int _index, Action<int,int> action)
    {
        charaSelecter = _charaSelecter;
        index = _index;
        OnLeftPlayer += action;

        for (int i = 0; i < accessPlayerIcons.Length; i++) { accessPlayerIcons[i].gameObject.SetActive(false); }

        PlayerNone();
    }

    /// <summary>
    /// 入力情報をもとにアクションを行う　操作しているプレイヤー番号も必要
    /// </summary>
    /// <param name="inputData">入力情報</param>
    /// <param name="_playerNum">誰が操作しているか</param>
    public void Process(SelectScene.InputData inputData, int _playerNum)
    {
        // 所有者がいる場合、所有者にしか操作できない
        if (playerNum >= 0 && playerNum != _playerNum) return;

        // アクセスしているプレイヤーと同一人物ではない
        if (accessPlayer >= 0 && accessPlayer != _playerNum) return;

        // キャラ選択中
        if (accessPlayer >= 0)
        {
            int id = charaSelecter.Process(index, inputData);
            SetDataSelect(id);
            return;
        }

        // キャラの再選択 or CPUの追加
        if(inputData.ACCEPT)
        {
            playerType = playerType == PlayerType.NONE ? PlayerType.CPU : playerType;
            JoinPlayer(playerType, _playerNum);

            return;
        }

        // 不参加
        if(inputData.CANCEL) { OnLeftPlayer(index,_playerNum); return; }

        // CPUの強さ設定
    }

    /// <summary>
    /// 無人にする
    /// </summary>
    /// <returns>元々のPlayerType</returns>
    public void PlayerNone()
    {
        playerType = PlayerType.NONE;
        accessPlayer = -1;
        playerNum = -1;

        // ウィンドウを変えて非表示
        window.sprite = noneWindow;
        textSelectArea.SetActive(false);
        textAcceptArea.SetActive(false);
        acceptImg.SetActive(false);
    }

    /// <summary>
    /// Windowにプレイヤーをジョインする
    /// </summary>
    /// <param name="_player">有人orCPU</param>
    /// <param name="_accessPlayer">アクセスしているプレイヤー番号</param>
    public void JoinPlayer(PlayerType _player, int _accessPlayer)
    {
        // アクセスしている人がいたら、一度リセット
        if(accessPlayer >= 0)
        {
            charaSelecter.Accept(index);
            SetActiveAccessPlayerIcon(accessPlayer, true, false);
            PlayerNone();
        }

        playerType = _player;
        accessPlayer = _accessPlayer;
        // 有人ならプレイヤー番号を 無人なら-1を
        playerNum = _player == PlayerType.PLAYER ? _accessPlayer : -1;

        // プレイヤーなら、プレイヤーの枠に
        if(_player == PlayerType.PLAYER)
        {
            window.sprite = playerWindow;
        }
        else // CPUなら、CPUの枠に
        {
            window.sprite= cpuWindow;
        }

        SetActiveAccessPlayerIcon(_accessPlayer, true, true);
        acceptImg.SetActive(false);

        charaSelecter.SelectCharacter(_player, index, charaID);

        // キャライラストの取得
        textAcceptArea.SetActive(false);
        textSelectArea.SetActive(true);
        SetDataSelect(charaID);

        SoundManager.Instance.SEPlay(SE.PLAYER_ENTRY);
    }

    /// <summary>
    /// キャラクターの決定
    /// </summary>
    public void Accept(int id)
    {
        SetActiveAccessPlayerIcon(accessPlayer, true, false);
        charaSelecter.SelectCharacter(playerType, index, id);

        // アクセスを解除
        accessPlayer = -1;
        charaID = id;

        // キャライラストの取得
        textSelectArea.SetActive(false);
        textAcceptArea.SetActive(true);
        SetDataAccept(id);

        // 確定アイコン表示
        acceptImg.SetActive(true);
    }

    // テキストデータを設定する
    void SetDataSelect(int id)
    {
        var visu = CharaDataManager.Instance.GetVisualData(id);
        charaImg_Select.sprite = visu.BustUp;

        var profile = CharaDataManager.Instance.GetCharacterProfile(id);

        if (profile == null) return;

        // 名前
        nameText_Select.text = profile.CharaName;
        // スキル名
        skillNameText_Select[0].text = profile.SkillNames[CharacterProfile.ATTACK_UP];
        skillNameText_Select[1].text = profile.SkillNames[CharacterProfile.ATTACK_RIGHT];
        skillNameText_Select[2].text = profile.SkillNames[CharacterProfile.ATTACK_LEFT];
        skillNameText_Select[3].text = profile.SkillNames[CharacterProfile.ATTACK_DOWN];
        // スキル説明
        skillExplanationText_Select[0].text = profile.SkillExplanation[CharacterProfile.ATTACK_UP];
        skillExplanationText_Select[1].text = profile.SkillExplanation[CharacterProfile.ATTACK_RIGHT];
        skillExplanationText_Select[2].text = profile.SkillExplanation[CharacterProfile.ATTACK_LEFT];
        skillExplanationText_Select[3].text = profile.SkillExplanation[CharacterProfile.ATTACK_DOWN];
    }

    void SetDataAccept(int id)
    {
        var visu = CharaDataManager.Instance.GetVisualData(id);
        charaImg.sprite = visu.BustUp;

        var profile = CharaDataManager.Instance.GetCharacterProfile(id);

        if (profile == null) return;

        // 名前
        nameText_Accept.text = profile.CharaName;
        // スキル名
        skillNameText_Accept[0].text = profile.SkillNames[CharacterProfile.ATTACK_UP];
        skillNameText_Accept[1].text = profile.SkillNames[CharacterProfile.ATTACK_RIGHT];
        skillNameText_Accept[2].text = profile.SkillNames[CharacterProfile.ATTACK_LEFT];
        skillNameText_Accept[3].text = profile.SkillNames[CharacterProfile.ATTACK_DOWN];

    }

    /// <summary>
    /// 誰かしらがアクセスしている
    /// </summary>
    public bool IsSelectNow() { return accessPlayer >= 0; }
    /// <summary>
    /// 指定したプレイヤーがアクセスしている
    /// </summary>
    /// <param name="_playerNum">指定プレイヤー</param>
    public bool IsSelectNow(int _playerNum) { return accessPlayer == _playerNum; }

    /// <summary>
    /// キャラIDを返す（不参加枠なら-1を返す）
    /// </summary>
    public int GetCharaID() { return playerType == PlayerType.NONE ? -1 : charaID; }

    public PlayerType GetPlayerType() => playerType;

    /// <summary>
    /// 有人だった場合、そのプレイヤーの番号を返す, Cpuだった場合、-1を返す
    /// </summary>
    public int GetPlayerNum() => playerNum;

    /// <summary>
    /// アクセス中のプレイヤーアイコンの表示,非表示
    /// </summary>
    /// <param name="_playerNum">プレイヤー番号</param>
    /// <param name="flag">表示,非表示</param>
    public void SetActiveAccessPlayerIcon(int _playerNum, bool flag, bool animationFlag)
    {
        accessPlayerIcons[_playerNum].gameObject.SetActive(flag);
        accessPlayerIcons[_playerNum].SetBool("Play", animationFlag);
    }
}
