using System;
using UnityEngine;

public class PlayersController : MonoBehaviour
{
    CharacterController[] characters;

    bool uiFlag;

    public event Action<bool> OnPause;

    /// <summary>
    /// 初期化
    /// </summary>
    public void Init(CharacterController[] _charcters)
    {
        characters = _charcters;

        uiFlag = false;
    }

    /// <summary>
    /// アップデート関数
    /// </summary>
    public void UpdateMethod()
    {
        if (uiFlag) UIPlayUpdate();
        else GamePlayUpdate();
    }

    // UI操作アップデート ===============================================
    void UIPlayUpdate()
    {
        bool pause = false; // ポーズフラグ

        var data = ControllerInputManager.Instance.GetControllerUIDatas();

        // 全プレイヤーの入力情報を確認
        for(int i = 0; i < data.Length; i++)
        {
            pause |= data[i].Value.MENU.IsPressed;
            pause |= data[i].Value.CANCEL.IsPressed;
        }

        if(pause)
        {
            uiFlag = false;
            ControllerInputManager.Instance.ChangeInputMode(INPUT_MODE.player);
            OnPause?.Invoke(false);
        }
    }

    // ゲーム操作アップデート ===========================================
    void GamePlayUpdate()
    {
        bool pause = false; // ポーズフラグ

        // 全プレイヤーの入力情報を取得
        var inputDatas = ControllerInputManager.Instance.GetControllerDatas();

        int count = 0;
        // 全プレイヤーの入力情報をキャラクターに反映
        for (int i = 0; i < inputDatas.Length; i++)
        {
            // キャラ以上にコントローラーがある場合
            if (count >= characters.Length) break;

            // コントローラーが登録されていないなら
            if (inputDatas[i] == null) continue;

            // データ変換 (入力情報 → コマンド情報)
            var data = GetCommandData(inputDatas[i].Value, out bool _pause);
            // コマンド情報を渡す
            characters[count].SetCommandData(data);
            count++;

            pause |= _pause;
        }

        // ポーズボタンを誰かが押していたら
        if(pause)
        {
            // UI操作に切り替える
            uiFlag = true;
            ControllerInputManager.Instance.ChangeInputMode(INPUT_MODE.ui);
            OnPause?.Invoke(true);
        }
    }
    // ===============================================================

    // データ変換 (入力情報 → コマンド情報)
    InputCommandData GetCommandData(InputGameData _data, out bool pause)
    {
        var data = new InputCommandData();

        data.DIRECTION_DATA = _data.DIRECTION_DATA;
        data.DIRECTION_VEC2 = _data.DIRECTION_VEC2;
        data.JUMP = _data.JUMP.now;
        data.ATTACK_UP = _data.ATTACK_UP.now;
        data.ATTACK_DOWN = _data.ATTACK_DOWN.now;
        data.ATTACK_LEFT = _data.ATTACK_LEFT.now;
        data.ATTACK_RIGHT = _data.ATTACK_RIGHT.now;
        data.SHIELD=_data.SHIELD.now;
        pause = _data.PAUSE;

        return data;
    }
}
