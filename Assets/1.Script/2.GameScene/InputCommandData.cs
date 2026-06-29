using UnityEngine;

/// <summary>
/// キャラクターのコマンド構造体
/// </summary>
public struct InputCommandData
{
    ///<summary> 左スティックの方角コマンド </summary>
    public DIRECTIONDATA DIRECTION_DATA;

    ///<summary> 左スティックの入力ベクトル </summary>
    public Vector2 DIRECTION_VEC2;

    ///<summary> 攻撃上ボタンの入力情報 </summary>
    public bool ATTACK_UP;

    ///<summary> 前回の攻撃上ボタンの入力情報 </summary>
    public bool ATTACK_UP_OLD;

    ///<summary> 攻撃下ボタンの入力情報 </summary>
    public bool ATTACK_DOWN;

    ///<summary> 前回の攻撃下ボタンの入力情報 </summary>
    public bool ATTACK_DOWN_OLD;

    ///<summary> 攻撃左ボタンの入力情報 </summary>
    public bool ATTACK_LEFT;

    ///<summary> 前回の攻撃左ボタンの入力情報 </summary>
    public bool ATTACK_LEFT_OLD;
    
    ///<summary> 攻撃右ボタンの入力情報 </summary>
    public bool ATTACK_RIGHT;

    ///<summary> 前回の攻撃右ボタンの入力情報 </summary>
    public bool ATTACK_RIGHT_OLD;
    
    ///<summary> ジャンプボタンの入力情報 </summary>
    public bool JUMP;

    ///<summary> 前回のジャンプボタンの入力情報 </summary>
    public bool JUMP_OLD;

    ///<summary> シールドボタンの入力情報 </summary>
    public bool SHIELD;

    ///<summary> 前回のシールドボタンの入力情報 </summary>
    public bool SHIELD_OLD;
    



}