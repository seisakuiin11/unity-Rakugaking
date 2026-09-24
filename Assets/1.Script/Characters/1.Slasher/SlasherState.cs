
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// サマーソルト～ブレイクスルー
/// </summary>
public class SlasherLeftAttackState: AttackState
{
    private bool breakthroughFlag;
    private bool breakthroughfirstFlag;
    private float gravityScale;


    public override void StateInit(CharacterController _chara, CharacterCommon _charaCommon, Animator _anim)
    {
        base.StateInit(_chara, _charaCommon, _anim);
        atkData = chara.charaData.leftAttackFrameData;
        gravityScale = chara.GetRigidBody().gravityScale;
    }

    public override void StateStart()
    {
        base.StateStart();
        anim.SetTrigger("AttackLeft");
        charaCommon.JumpAction();
        SoundManager.Instance.CharaSEPlay(CHARASE.SLASHER_SOMERSAULT);
        breakthroughFlag = false;
        breakthroughfirstFlag = false;
    }

    public override void StateUpdateMethod(InputCommandData _inputData)
    {
        //横滑り防止
        if (charaCommon.GroundCheck())
        {
            charaCommon.StopXMove();
        }
        if (atkData == null) chara.ChangeState(CHARA_STATE.idle);
        inputData = _inputData;

        //派生チェック
        if (!breakthroughFlag) breakthroughFlag = ButtonTapCheck();

        AttackAction();
      
    }

    /// <summary>
    /// 攻撃のフレーム処理
    /// </summary>
    protected override void AttackAction()
    {
        //フレーム数の加算
        atkNowFrame++;

        //サマーソルト中最大フレーム到達時に派生ボタンが押されていない場合攻撃終了
        if (atkNowFrame >= atkData.palameters[0].value && !breakthroughFlag)
        {
            AtkEnd();
        }

        else if(atkNowFrame >= atkData.palameters[0].value && breakthroughFlag &&!breakthroughfirstFlag)
        //押されていた場合派生技開始
        {
            //派生アニメーション再生
            anim.SetTrigger("Breakthrough");
            SoundManager.Instance.CharaSEPlay(CHARASE.SLASHER_BREAKTHROUGH);
            breakthroughfirstFlag=true;

            // 重力を消し、空中にとどまる
            chara.GetRigidBody().gravityScale = 0f;
            chara.GetRigidBody().linearVelocity = Vector2.zero;
           
        }

        //現在のフレーム数が全体フレーム以上になったら攻撃終了
        if (atkNowFrame >= atkAllFrame)
        {
            // 重力を戻す
            chara.GetRigidBody().gravityScale = gravityScale;

            AtkEnd();
            return;

        }

        //現在のターゲットフレームが存在しない場合は何もしない
        if (targetFrameIndex > atkFrameData.Count) 
        {
            Debug.Log("ターゲットフレームなし");
            return;
        }

        //現在のフレーム数がキーフレームの値以上になったらコライダー情報を更新
        if (atkNowFrame >= atkFrameData[targetFrameIndex].TargetFrame)
        {

            ColliderAction();

            //次のターゲットフレームに変更
            targetFrameIndex++;
        }


    }

    public override void StateEnd()
    {
        base.StateEnd();

        // 重力を戻す
        chara.GetRigidBody().gravityScale = gravityScale;
    }

    private bool ButtonTapCheck()
    {
        if (inputData.ATTACK_LEFT && !inputData.ATTACK_LEFT_OLD)
        {
            return true;
        }
        return false;
    }
}









/// <summary>
/// 蹴り上げ
/// </summary>
public class SlasherDownAttackState : AttackState
{
    public override void StateInit(CharacterController _chara, CharacterCommon _charaCommon, Animator _anim)
    {
        base.StateInit(_chara, _charaCommon, _anim);
        atkData = chara.charaData.downAttackFrameData;
    }


    public override void StateStart()
    {
        base.StateStart();
        anim.SetTrigger("AttackDown");
        SoundManager.Instance.CharaSEPlay(CHARASE.SLASHER_KERIAGE);
    }


}



/// <summary>
/// ダイブソバット
/// </summary>
public class SlasherRightAttackState : AttackState
{
    private float moveDirX;         //進行方向
    private float moveSpeed = 1.5f;   //攻撃中の移動速度
    public override void StateInit(CharacterController _chara, CharacterCommon _charaCommon, Animator _anim)
    {
        base.StateInit(_chara, _charaCommon, _anim);
        atkData = chara.charaData.rightAttackFrameData;


    }

    public override void StateStart()
    {
        base.StateStart();
        anim.SetTrigger("AttackRight");
        SoundManager.Instance.CharaSEPlay(CHARASE.SLASHER_DIVESOBAT);
        //初期の方向をキャラの向きに合わせる
        moveDirX = chara.transform.localScale.x;
    }

    public override void StateUpdateMethod(InputCommandData _inputData)
    {


        if (atkData == null) chara.ChangeState(CHARA_STATE.idle);
        inputData = _inputData;



        Vector2 moveVec = new(moveDirX * moveSpeed, 0);

        charaCommon.MoveAction(moveVec);
        AttackAction();
    }

}





/// <summary>
/// アクセル
/// </summary>
public class SlasherUpAttackState : AttackState
{
    private bool continueAtkFlag; //百裂を継続するかのフラグ


    public override void StateInit(CharacterController _chara, CharacterCommon _charaCommon, Animator _anim)
    {
        base.StateInit(_chara, _charaCommon, _anim);
        atkData = chara.charaData.upAttackFrameData;
    }


    public override void StateStart()
    {
        base.StateStart();
        anim.SetTrigger("AttackUp");

    }

    public override void StateUpdateMethod(InputCommandData _inputData)
    {
        inputData= _inputData;
        base.StateUpdateMethod(inputData);

    }
    /// <summary>
    /// 攻撃のフレーム処理
    /// </summary>
    protected override void AttackAction()
    {
        //フレーム数の加算
        atkNowFrame++;

        //現在のフレーム数が全体フレーム以上になったら攻撃終了
        if (atkNowFrame >= atkAllFrame)
        {
            AtkEnd();
            return;

        }

        //現在のターゲットフレームが存在しない場合は何もしない
        if (targetFrameIndex > atkFrameData.Count) return;

        //連打されているかの確認
        if (targetFrameIndex <= atkData.palameters[0].value && !continueAtkFlag) continueAtkFlag = RapidTapCheck();
       
        //現在のフレーム数がキーフレームの値以上になったらコライダー情報を更新
        if (atkNowFrame >= atkFrameData[targetFrameIndex].TargetFrame)
        {

            ColliderAction();


            //連打されていた場合百裂を継続する
            if (targetFrameIndex <= atkData.palameters[0].value && continueAtkFlag) {AttackContinue(); return;}
           
            //次のターゲットフレームに変更
            targetFrameIndex++;
        }


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
            colData.trans = chara.gameObject.transform;




            var scaleY = chara.gameObject.transform.localScale.y;

            //サイズを対象のTransform.LocalScaleのY軸に合わせる
            colData.radius *= chara.gameObject.transform.localScale.y;

            //位置関係を対象のTransform.LocalScaleのY軸に合わせる
            colData.localPos = new Vector3(
                colData.localPos.x * scaleY,
                colData.localPos.y * scaleY,
                colData.localPos.z * scaleY
                );


            if (colData.colType == COLLIDER_TYPE.AttackCol) _atkColList.Add(colData);
            else if (colData.colType == COLLIDER_TYPE.HitBox) _hitBoxColList.Add(colData);
        }


     
        //攻撃コライダーの更新、生成
        if (_atkColList != null)
        {
            if (atkColList != null) colManager.DestroyCircleCol(atkColList);
            SoundManager.Instance.CharaSEPlay(CHARASE.SLASHER_ACCEL);
            atkColList = colManager.UpdateAttackColliderData(_atkColList, AtkHit);
        }


        //当たり判定の更新、生成
        if (_hitBoxColList != null)
        {
            charaCommon.HitBoxReset();
            charaCommon.UpdateHitBoxColliderData(_hitBoxColList);
        }

    }


    private void AttackContinue()
    {

        
        targetFrameIndex = (int)atkData.palameters[1].value;
        atkNowFrame = (int)atkData.palameters[2].value;
        continueAtkFlag = false;

        //若干後ろに動く（はめ殺し防止のため）
        Vector2 moveVec = new ();
        if (chara.DirectionRightCheck()) moveVec.x = -0.1f;
        else moveVec.x = 0.1f;

        charaCommon.Move(moveVec);
    }

    private bool RapidTapCheck()
    {

        if (inputData.ATTACK_UP && !inputData.ATTACK_UP_OLD)  return true;
        return false;
    }
}
