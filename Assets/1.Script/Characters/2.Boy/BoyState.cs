
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


/// <summary>
/// けんをもどす
/// </summary>
public class BoyLeftAttackState : AttackState
{
    BoyBehavior behavior;

    public BoyLeftAttackState(BoyBehavior _charaBehavior) { behavior = _charaBehavior; }

    public override void StateInit(CharacterController _chara, CharacterCommon _charaCommon, Animator _anim)
    {
        base.StateInit(_chara, _charaCommon, _anim);
        atkData = chara.charaData.leftAttackFrameData;
    }


    public override void StateStart()
    {
        // 剣の本数が0本の場合は行わない
        if (behavior.GetSwordCount() <=0 ) { AtkEnd(); return; }
        base.StateStart();
        anim.SetTrigger("AttackLeft");
        behavior.SwordReturn();

        SoundManager.Instance.CharaSEPlay(CHARASE.BOY_KENMODOSU);
    }


}


/// <summary>
/// けんをふる
/// </summary>
public class BoyRightAttackState : AttackState
{
    private BoyBehavior behavior;
    private Transform swordTrans;

    public BoyRightAttackState(BoyBehavior _charaBehavior){ behavior = _charaBehavior; }


    public override void StateInit(CharacterController _chara, CharacterCommon _charaCommon, Animator _anim)
    {
        base.StateInit(_chara, _charaCommon, _anim);
        atkData = chara.charaData.downAttackFrameData;
        swordTrans = behavior.SwordTrans;
    }

    public override void StateStart()
    {
        base.StateStart();
        anim.SetTrigger("AttackDown");

        SoundManager.Instance.CharaSEPlay(CHARASE.BOY_KENHURU);
    }

    /// <summary>
    /// コライダー生成、移動
    /// </summary>
    protected override void ColliderAction()
    {

        //キーフレームに格納されているデータを収納する
        var colDatas = atkFrameData[targetFrameIndex].colliders;
 

        knockBackNum = atkFrameData[targetFrameIndex].KnockBackNum;

        List<CircleColData> _atkColList = new();
        List<CircleColData> _hitBoxColList = new();

        //データに格納されているコライダーの数分行われる
        for (int i = 0; i < colDatas.Count; i++)
        {
            var colData = colDatas[i];

            //Transformを格納
            colData.trans = chara.transform;

            // アタックコライダーなら、剣を参照
            if (colData.colType == COLLIDER_TYPE.AttackCol)
            {
                var scaleY = swordTrans.localScale.y;
                //サイズを対象のTransform.LocalScaleのY軸に合わせる
                colData.radius *= scaleY;

                //位置関係を対象のTransform.LocalScaleのY軸に合わせる 剣を原点とする
                colData.localPos = new Vector3(
                    colData.localPos.x * scaleY + swordTrans.localPosition.x,
                    colData.localPos.y * scaleY + swordTrans.localPosition.y,
                    colData.localPos.z
                    );

                _atkColList.Add(colData);
            }
            else if (colData.colType == COLLIDER_TYPE.HitBox)
            {
                colData.radius *= colData.trans.localScale.y;

                //位置関係を対象のTransform.LocalScaleのY軸に合わせる
                colData.localPos = new Vector3(
                    colData.localPos.x,
                    colData.localPos.y,
                    colData.localPos.z
                    );

                _hitBoxColList.Add(colData);
            }
        }



        //攻撃コライダーの更新、生成
        if (_atkColList != null)
        {
            if (atkColList != null) colManager.DestroyCircleCol(atkColList);
            atkColList = colManager.UpdateAttackColliderData(_atkColList, AtkHit);
        }


        //当たり判定の更新、生成
        if (_hitBoxColList != null)
        {
            charaCommon.HitBoxReset();
            charaCommon.UpdateHitBoxColliderData(_hitBoxColList);
        }

    }

}



/// <summary>
/// けんをなげる
/// </summary>
public class BoyDownAttackState : AttackState
{
    BoyBehavior behavior;
    public BoyDownAttackState(BoyBehavior _charaBehavior) { behavior = _charaBehavior; } 

    public override void StateInit(CharacterController _chara, CharacterCommon _charaCommon, Animator _anim)
    {
        base.StateInit(_chara, _charaCommon, _anim);
        atkData = chara.charaData.rightAttackFrameData;
    }


    public override void StateStart()
    {
        //剣の本数が３本の場合は行わない
        if (behavior.GetSwordCount() > 2) {
            AtkEnd();
               
            return;
        }
        base.StateStart();
        anim.SetTrigger("AttackRight");
        behavior.SwordSummon();

        SoundManager.Instance.CharaSEPlay(CHARASE.BOY_KENNAGERU);
    }


}



/// <summary>
/// げんきいっぱい
/// </summary>
public class BoyUpAttackState : AttackState
{

    BoyBehavior behavior;
    public BoyUpAttackState(BoyBehavior _charaBehavior){ behavior = _charaBehavior; }

   public override void StateInit(CharacterController _chara, CharacterCommon _charaCommon, Animator _anim)
    {
        base.StateInit(_chara, _charaCommon, _anim);
        atkData = chara.charaData.upAttackFrameData;
    }


    public override void StateStart()
    {
        base.StateStart();
        anim.SetTrigger("AttackUp");
        BoyStatusUp();

        SoundManager.Instance.CharaSEPlay(CHARASE.BOY_GENKIIPPAI);
    }

    public override void StateUpdateMethod(InputCommandData _inputData)
    {
        // 移動処理
        if (charaCommon.GroundCheck())
        {
            inputData = _inputData;
            charaCommon.FixedMoveAction(inputData.DIRECTION_VEC2);
        }

        //攻撃フレーム処理
        AttackAction();
    }

    public void BoyStatusUp() 
    {
        behavior.BoostEnergy();
    }
}