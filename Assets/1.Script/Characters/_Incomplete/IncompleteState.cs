using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// サマーソルト～ブレイクスルー
/// </summary>
public class IncompleteLeftAttackState : AttackState
{
    public override void StateInit(CharacterController _chara, Animator _anim)
    {
        base.StateInit(_chara, _anim);
        atkData = chara.charaData.leftAttackFrameData;
    }

    public override void StateStart()
    {
        base.StateStart();
        anim.SetTrigger("AttackLeft");
    }
}









/// <summary>
/// 蹴り上げ
/// </summary>
public class IncompleteDownAttackState : AttackState
{
    public override void StateInit(CharacterController _chara, Animator _anim)
    {
       
        base.StateInit(_chara, _anim);
        atkData = chara.charaData.downAttackFrameData;
    }


    public override void StateStart()
    {
        base.StateStart();
        anim.SetTrigger("AttackDown");
    }
}



/// <summary>
/// ダイブソバット
/// </summary>
public class IncompleteRightAttackState : AttackState
{
    public override void StateInit(CharacterController _chara, Animator _anim)
    {
        base.StateInit(_chara, _anim);
        atkData = chara.charaData.rightAttackFrameData;


    }

    public override void StateStart()
    {
        base.StateStart();
        anim.SetTrigger("AttackRight");
    }
}





/// <summary>
/// アクセル～トルネード
/// </summary>
public class IncompleteUpAttackState : AttackState
{
    public override void StateInit(CharacterController _chara, Animator _anim)
    {
        base.StateInit(_chara, _anim);
        atkData = chara.charaData.upAttackFrameData;
    }


    public override void StateStart()
    {
        base.StateStart();
        anim.SetTrigger("AttackUp");

    }
}
