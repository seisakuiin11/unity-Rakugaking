using UnityEngine;


[System.Flags]
/// <summary>
/// 左スティックの方角をコマンド化
/// </summary>
public enum DIRECTIONDATA
{
    Neutral = 0,
    UP = 1 << 0,
    DOWN = 1 << 1,
    LEFT = 1 << 2,
    RIGHT = 1 << 3,
}


/// <summary>
/// ゲーム中のコントローラーの入力情報
/// </summary>
public  struct InputGameData
{
    ///<summary> 左スティックの方角コマンド </summary>
    public DIRECTIONDATA DIRECTION_DATA ;

    ///<summary> 左スティックの入力ベクトル </summary>
    public Vector2 DIRECTION_VEC2;

    ///<summary> 攻撃上ボタンの入力情報 </summary>
    public ButtonState ATTACK_UP;

    ///<summary> 攻撃下ボタンの入力情報 </summary>
    public ButtonState ATTACK_DOWN;

    ///<summary> 攻撃左ボタンの入力情報 </summary>
    public ButtonState ATTACK_LEFT;
    
    ///<summary> 攻撃右ボタンの入力情報 </summary>
    public ButtonState ATTACK_RIGHT;

    ///<summary> ジャンプボタンの入力情報 </summary>
    public ButtonState JUMP;

    ///<summary> シールドボタンの入力情報 </summary>
    public ButtonState SHIELD;
    
    /// <summary> ポーズボタンの入力情報</summary>
    public bool PAUSE;

}


/// <summary>
/// UI状態のコントローラーの入力情報
/// </summary>
public struct InputUIData
{
    ///<summary> 左スティックの方角コマンド </summary>
    public DIRECTIONDATA DIRECTION_DATA;

    ///<summary> 左スティックの入力ベクトル </summary>
    public Vector2 DIRECTION_VEC2;

    ///<summary> 決定ボタンの入力情報 </summary>
    public ButtonState SUBMIT;

    ///<summary> 戻るボタンの入力情報 </summary>
    public ButtonState CANCEL;

    /// <summary> スタート </summary>
    public ButtonState START;

    /// <summary> メニュー </summary>
    public ButtonState MENU;

    ///<summary> Rボタンの入力情報 </summary>
    public ButtonState R_BUTTON;

    ///<summary> Lボタンの入力情報 </summary>
    public ButtonState L_BUTTON;

}



public struct ButtonState
{
    /// <summary> 今の入力 </summary>
    public bool now;
    /// <summary> 前の入力 </summary>
    public bool past;

    public void Set(bool value) { now = value; }
    public void Save() { past = now; }

    /// <summary> 押した瞬間 </summary>
    public bool IsPressed => now && !past;

    /// <summary> 離した瞬間 </summary>
    public bool IsReleased => !now && past;

    /// <summary> 押している間 </summary>
    public bool IsHeld => now && past;
}