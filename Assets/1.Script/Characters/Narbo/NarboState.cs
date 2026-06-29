using System.Collections.Generic;
using UnityEngine;

public class NarboLeftAttackState : AttackState
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


public class NarboDownAttackState:AttackState
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



public class NarboRightAttackState : AttackState
{
    private float moveDirX;
    public override void StateInit(CharacterController _chara, Animator _anim)
    {
        base.StateInit(_chara, _anim);
        atkData = chara.charaData.rightAttackFrameData;


    }

    public override void StateStart()
    {
        base.StateStart();
        anim.SetTrigger("AttackRight");

        //初期の方向をキャラの向きに合わせる
        moveDirX = chara.transform.localScale.x;
    }

    public override void StateUpdateMethod(InputCommandData _inputData)
    {
        

        if (atkData == null) chara.ChangeState(CHARA_STATE.idle);
        inputData = _inputData;

        if (inputData.DIRECTION_VEC2.x < 0) moveDirX = -1;
        else if (inputData.DIRECTION_VEC2.x > 0) moveDirX = 1;

        Vector2 moveVec = new (moveDirX,0f);

        chara.MoveAction(moveVec);
        AttackAction();
    }

}