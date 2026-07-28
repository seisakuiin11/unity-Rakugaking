
using UnityEngine;


/// <summary>
/// わっしょい
/// </summary>
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


/// <summary>
/// 地ならし
/// </summary>
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


/// <summary>
/// ストライク
/// </summary>
public class NarboRightAttackState : AttackState
{
    private float moveDirX;         //進行方向
    private float moveSpeed=1.5f;   //攻撃中の移動速度
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



        Vector2 moveVec = new (moveDirX*moveSpeed,0);

        chara.MoveAction(moveVec);
        AttackAction();
    }

}

/// <summary>
/// だいばくはつ
/// </summary>
public class NarboUpAttackState : AttackState
{

    private bool atkFlag;

    private float scaleMultiplier=1f;

    private const int maxScaleUpCount = 3;

    private int nowScaleUpCount = 0;

    public override void StateInit(CharacterController _chara, Animator _anim)
    {
        base.StateInit(_chara, _anim);
        atkData = chara.charaData.upAttackFrameData;


    }

    public override void StateStart()
    {
        base.StateStart();
        anim.SetTrigger("AttackUp");
        scaleMultiplier = 1f;
        nowScaleUpCount = 0;
        atkFlag = false;
        
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


        AttackAction();

        //攻撃コライダーを生成していないかつ攻撃ボタンを押していたらサイズを大きくする
        if (!atkFlag && RapidTapCheck() && nowScaleUpCount < maxScaleUpCount)
        {

            nowScaleUpCount++;
            scaleMultiplier += 0.1f;
            Debug.Log(nowScaleUpCount);
            var scale=chara.transform.localScale;

            scale=scale*scaleMultiplier;

            chara.ScaleChange(scale);
        }
    }

    private bool RapidTapCheck()
    {
        if (inputData.ATTACK_UP && !inputData.ATTACK_UP_OLD) return true;
        return false;
    }

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

        //現在のフレーム数がキーフレームの値以上になったらコライダー情報を更新
        if (atkNowFrame >= atkFrameData[targetFrameIndex].TargetFrame)
        {
            anim.SetTrigger("Daibakuhatu");
            atkFlag = true;
            ColliderAction();

            //次のターゲットフレームに変更
            targetFrameIndex++;
        }
    }

    protected override void AtkEnd()
    {
        base.AtkEnd();
        chara.ScaleChange(Vector3.one);
    }

    public override void Damage(int _damage, int _stanFrame, KnockBackData _knockBack, Collider _colPos)
    {
        AtkEnd();
        base.Damage(_damage, _stanFrame, _knockBack, _colPos);
    }

}