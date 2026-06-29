using System;
using UnityEngine;
using UnityEngine.UI;


public class PlayerWindowController : MonoBehaviour
{
    [SerializeField, Header("ウィンドウ")] Image window;
    [SerializeField, Header("キャラImage")] Image charaImg;
    [SerializeField, Header("決定アイコン")] GameObject acceptImg;

    CharaIconsController charaSelecter;

    /// <summary> プレイヤー不参加処理 </summary>
    public event Action<int,int> OnLeftPlayer;

    int index;
    PlayerType playerType;
    int playerNum;
    int charaID;
    int accessPlayer = -1;

    public void Init(CharaIconsController _charaSelecter, int _index, Action<int,int> action)
    {
        charaSelecter = _charaSelecter;
        index = _index;
        OnLeftPlayer += action;

        PlayerNone();
    }

    /// <summary>
    /// 入力情報をもとにアクションを行う　操作しているプレイヤー番号も必要
    /// </summary>
    /// <param name="inputData">入力情報</param>
    /// <param name="_playerNum">誰が操作しているか</param>
    public void Process(SelectScene.InputData inputData, int _playerNum)
    {
        // アクセスしているプレイヤーと同一人物ではない
        if (accessPlayer >= 0 && accessPlayer != _playerNum) return;

        // キャラ選択中
        if (accessPlayer >= 0) { charaImg.sprite = charaSelecter.Process(index, inputData); return; }

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

        window.color = new Color(0.3f, 0.3f, 0.3f, 3f);
        charaImg.gameObject.SetActive(false);
        acceptImg.SetActive(false);
    }

    /// <summary>
    /// Windowにプレイヤーをジョインする
    /// </summary>
    /// <param name="_player">有人orCPU</param>
    /// <param name="_accessPlayer">アクセスしているプレイヤー番号</param>
    public void JoinPlayer(PlayerType _player, int _accessPlayer)
    {
        playerType = _player;
        accessPlayer = _accessPlayer;
        // 有人ならプレイヤー番号を 無人なら-1を
        playerNum = _player == PlayerType.PLAYER ? _accessPlayer : -1;

        var color = playerType == PlayerType.PLAYER ? Color.red : Color.gray;
        window.color = color;
        charaImg.gameObject.SetActive(true);
        acceptImg.SetActive(false);

        // キャライラストの取得
        charaImg.sprite = charaSelecter.SelectCharacter(_player, index, charaID);
    }

    /// <summary>
    /// キャラクターの決定
    /// </summary>
    public void Accept(int id)
    {
        // アクセスを解除
        accessPlayer = -1;
        charaID = id;

        // キャライラストの取得
        charaImg.sprite = charaSelecter.SelectCharacter(playerType, index, id);

        // 確定アイコン表示
        acceptImg.SetActive(true);
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
}
