using System;
using UnityEngine;


/// <summary>
/// けんをもどす
/// </summary>
public class BoyLeftAttackState : AttackState
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
/// けんをふる
/// </summary>
public class BoyDownAttackState : AttackState
{




    
    public override void StateInit(CharacterController _chara, Animator _anim)
    {
        base.StateInit(_chara, _anim);
        atkData = chara.charaData.leftAttackFrameData;
        
    }


    public override void StateStart()
    {
        base.StateStart();
        anim.SetTrigger("AttackDown");

    }

    private void StatusUp()
    {
       
    }

}



/// <summary>
/// けんをなげる
/// </summary>
public class BoyRightAttackState : AttackState
{


    public override void StateInit(CharacterController _chara, Animator _anim)
    {
        base.StateInit(_chara, _anim);
        atkData = chara.charaData.leftAttackFrameData;
    }


    public override void StateStart()
    {
        base.StateStart();
        anim.SetTrigger("AttackRight");

    }


}



/// <summary>
/// げんきいっぱい
/// </summary>
public class BoyUpAttackState : AttackState
{

    public event Action OnBoyStatusUp;
    public override void StateInit(CharacterController _chara, Animator _anim)
    {
        base.StateInit(_chara, _anim);
        atkData = chara.charaData.leftAttackFrameData;
    }


    public override void StateStart()
    {
        base.StateStart();
        anim.SetTrigger("AttackUp");
        OnBoyStatusUp.Invoke();
        
    }

    public void BoyStatusUp() 
    {
        OnBoyStatusUp?.Invoke();
    
    }
}