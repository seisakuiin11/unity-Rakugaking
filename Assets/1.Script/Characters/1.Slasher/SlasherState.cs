using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// サマーソルト～ブレイクスルー
/// </summary>
public class SlasherLeftAttackState: AttackState
{
    private bool breakthroughFlag;


    public override void StateInit(CharacterController _chara, Animator _anim)
    {
        base.StateInit(_chara, _anim);
        atkData = chara.charaData.leftAttackFrameData;
    }

    public override void StateStart()
    {
        base.StateStart();
        anim.SetTrigger("AttackLeft");

        breakthroughFlag = false;
    }

    public override void StateUpdateMethod(InputCommandData _inputData)
    {
        //横滑り防止
        if (chara.GroundCheck())
        {
            chara.StopXMove();
        }
        if (atkData == null) chara.ChangeState(CHARA_STATE.idle);
        inputData = _inputData;

        //派生チェック
        if (breakthroughFlag == false) breakthroughFlag = ButtonTapCheck();

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
        else
        //押されていた場合派生技開始
        {
            //派生アニメーション再生

        }

        //現在のフレーム数が全体フレーム以上になったら攻撃終了
        if (atkNowFrame >= atkAllFrame)
        {
            AtkEnd();
            return;

        }

        //現在のターゲットフレームが存在しない場合は何もしない
        if (targetFrameIndex > atkFrameData.Count) return;

        //現在のフレーム数がキーフレームの値以上になったらコライダー情報を更新
        if (atkNowFrame >= atkFrameData[targetFrameIndex].TargetFrame)
        {

            ColliderAction();

            //次のターゲットフレームに変更
            targetFrameIndex++;
        }


    }

    private bool ButtonTapCheck()
    {
        if (inputData.ATTACK_LEFT && !inputData.ATTACK_LEFT_OLD) return true;
        return false;
    }
}









/// <summary>
/// 蹴り上げ
/// </summary>
public class SlasherDownAttackState : AttackState
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


}



/// <summary>
/// ダイブソバット
/// </summary>
public class SlasherRightAttackState : AttackState
{
    private float moveDirX;         //進行方向
    private float moveSpeed = 1.5f;   //攻撃中の移動速度
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



        Vector2 moveVec = new(moveDirX * moveSpeed, 0);

        chara.MoveAction(moveVec);
        AttackAction();
    }

}





/// <summary>
/// アクセル～トルネード
/// </summary>
public class SlasherUpAttackState : AttackState
{
    public override void StateInit(CharacterController _chara, Animator _anim)
    {
        base.StateInit(_chara, _anim);
        atkData = chara.charaData.leftAttackFrameData;
    }


    public override void StateStart()
    {
        base.StateStart();
        anim.SetTrigger("AttackUp");

    }


}
