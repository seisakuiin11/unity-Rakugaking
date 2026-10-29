using System;
using UnityEngine;

/// <summary>
/// 作成したキャラクターの処理を行うクラス
/// </summary>
public class CustomCharacterBehaviour : CharacterBehavior
{

    public override void Init(ColliderManager _colManager)
    {
       
        base.Init(_colManager);
        Atk_Left = AttackStateDataInput("NarboLeftAttackState");
        Atk_Right = AttackStateDataInput("NarboRightAttackState");
        Atk_Up = AttackStateDataInput("NarboUpAttackState");
        Atk_Down = AttackStateDataInput("NarboDownAttackState");
    }

    /// <summary>
    /// 取得したデータから各情報に変換する
    /// </summary>
    private AttackState AttackStateDataInput(string _name)
    {

        //名前からクラスを取得
        Type type = Type.GetType(_name);

        if (type == null) 
        {
            Debug.Log("存在しません"); 
            return null;
        }

        AttackState _state =(AttackState)Activator.CreateInstance(type);
        Debug.Log(_state);
        return _state;
    }
}
