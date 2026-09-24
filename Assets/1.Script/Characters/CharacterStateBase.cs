

using System;
using System.Collections.Generic;
using UnityEngine;


//キャラクターのStateパターン


/// <summary>
/// Stateパターンのベース
/// </summary>
public class CharacterStateBase
{
    /// <summary>
    /// character自身
    /// </summary>
    protected CharacterController chara;


    protected CharacterCommon charaCommon;
    /// <summary>
    /// コントローラーの入力
    /// </summary>
    protected InputCommandData inputData;

    /// <summary>
    /// アニメーション
    /// </summary>
    protected Animator anim;

    



    /// <summary>
    /// 初期化　　　base:CharacterControllerとAnimatorを格納する
    /// </summary>
    /// <param name="_chara"></param>
    public virtual void StateInit(CharacterController _chara,CharacterCommon _charaCommon,Animator _anim)
    {
        chara = _chara;
        charaCommon = _charaCommon;
        anim = _anim;
    }




    public virtual void StateStart() { }


    /// <summary>
    /// Update処理　　 base:inputdataを受け取り、格納する
    /// </summary>
    /// <param name="_inputData"></param>
    public virtual void StateUpdateMethod(InputCommandData _inputData)
    {
        inputData = _inputData;


    }


    /// <summary>
    /// base:引数の値をそのまま反映し、攻撃を受ける
    /// </summary>
    /// <param name="_damage"></param>
    /// <param name="_stanFrame"></param>
    /// <param name="_knockBack"></param>
    public virtual void Damage(int _damage,int _stanFrame,KnockBackData _knockBack,Collider _col)
    {
        if(_stanFrame  != 0)
        {
            chara.ChangeState(CHARA_STATE.stan);
            charaCommon.SetStanFlame(_stanFrame);
        }

        charaCommon.HPChange(-_damage);
        charaCommon.KnockBack(_knockBack, _col);
        SoundManager.Instance.CharaSEPlay(CHARASE.HIT_1);
    }



    /// <summary>
    /// 終了
    /// </summary>
    public virtual void StateEnd() { }
}

/// <summary>
/// Idle時のState
/// </summary>
public class IdleState : CharacterStateBase
{

    




    public override void StateInit(CharacterController _chara,CharacterCommon _charaCommon,Animator _anim)
    {
        base.StateInit(_chara, _charaCommon,_anim);



    }


    public override void StateStart()
    {
        anim.SetTrigger("Idle");
    }


    public override void StateUpdateMethod(InputCommandData _inputData)
    {
        inputData = _inputData;

        if (chara.ShieldCheck()) { chara.ChangeState(CHARA_STATE.shield); return; }

        if (chara.MoveCheck()) { chara.ChangeState(CHARA_STATE.move); return; }

        if (chara.AttackCheck()) return;

        if (chara.JumpCheck()) { chara.ChangeState(CHARA_STATE.jump); return; }

        if (!charaCommon.GroundCheck()) { chara.ChangeState(CHARA_STATE.air); return; }

        if(_inputData.DIRECTION_DATA==DIRECTIONDATA.DOWN && charaCommon.FootingGroundCheck(out var col))  { charaCommon.FootingGroundOff(col);}

        charaCommon.StopXMove();
        charaCommon.ShieldHeal();
    }

    public override void StateEnd()
    {
        
        base.StateEnd();
    }

}

/// <summary>
/// 移動時のState
/// </summary>
public class MoveState : CharacterStateBase
{



    public override void StateInit(CharacterController _chara, CharacterCommon _charaCommon, Animator _anim)
    {
        base.StateInit(_chara, _charaCommon, _anim);
    }


    public override void StateStart()
    {
        anim.SetTrigger("Move");
    }


    public override void StateUpdateMethod(InputCommandData _inputData)
    {
        inputData = _inputData;
        charaCommon.FixedMoveAction(inputData.DIRECTION_VEC2);

        if (chara.ShieldCheck()){ chara.ChangeState(CHARA_STATE.shield); return; }

        if (chara.AttackCheck()) return;

        if (chara.JumpCheck()) { chara.ChangeState(CHARA_STATE.jump);  return; }
        
        if (!charaCommon.GroundCheck()){ chara.ChangeState(CHARA_STATE.air); return; }

        if (!chara.MoveCheck()) { chara.ChangeState(CHARA_STATE.idle); return; }

        if (_inputData.DIRECTION_DATA == DIRECTIONDATA.DOWN && charaCommon.FootingGroundCheck(out var col)) { charaCommon.FootingGroundOff(col); }

        charaCommon.ShieldHeal();
   
        
    }

    public override void StateEnd()
    {
        var rb =chara.GetRigidBody();
        rb.linearVelocity = Vector2.zero;
    }

}



/// <summary>
/// ジャンプ時のState
/// </summary>
public class JumpState : CharacterStateBase
{
    public event Action<CharacterController> OnJump;

    public override void StateInit(CharacterController _chara, CharacterCommon _charaCommon, Animator _anim)
    {
        base.StateInit(_chara, _charaCommon, _anim);
    }


    public override void StateStart()
    {
        anim.SetTrigger("Jump");
        charaCommon.JumpAction();
        chara.ChangeState(CHARA_STATE.air);
        SoundManager.Instance.CharaSEPlay(CHARASE.JUMP);
        OnJump?.Invoke(chara);
    
    }

    public override void StateUpdateMethod(InputCommandData _inputData)
    {
        if (charaCommon.GroundCheck()) chara.ChangeState(CHARA_STATE.idle);

        inputData = _inputData;

        if (chara.AttackCheck()) return;

       

        if(inputData.DIRECTION_VEC2.x!=0) charaCommon.MoveAction(inputData.DIRECTION_VEC2);

    }



}

/// <summary>
/// 空中時のState
/// </summary>
public class AirState : CharacterStateBase
{


    public override void StateInit(CharacterController _chara, CharacterCommon _charaCommon, Animator _anim)
    {
        base.StateInit(_chara, _charaCommon, _anim);
    }


    public override void StateStart()
    {
        anim.SetTrigger("Idle");
        anim.SetBool("Air", true);
    }


    public override void StateUpdateMethod(InputCommandData _inputData)
    {
        base.StateUpdateMethod(_inputData);

        if (charaCommon.GroundCheck()) chara.ChangeState(CHARA_STATE.idle);
        
        if (chara.AttackCheck()) return;

        if (chara.MoveCheck()) charaCommon.FixedMoveAction(inputData.DIRECTION_VEC2);
       
        charaCommon.ShieldHeal();
    }

    public override void StateEnd()
    { 
        anim.SetBool("Air", false);
        base.StateEnd();

    }


}



/// <summary>
/// スタン時のState
/// </summary>
public class StanState : CharacterStateBase
{


    public override void StateInit(CharacterController _chara, CharacterCommon _charaCommon, Animator _anim)
    {
        base.StateInit(_chara, _charaCommon, _anim);
    }

    public override void StateStart()
    {
        anim.SetTrigger("Stun");
    }


    public override void StateUpdateMethod(InputCommandData _inputData)
    {
        charaCommon.stanFrame--;
        if (charaCommon.stanFrame<=0) chara.ChangeState(CHARA_STATE.idle);
        

    }

    public override void StateEnd()
    {
        base.StateEnd();

    }


}

/// <summary>
/// シールド時のState
/// </summary>
public class ShieldState:CharacterStateBase
{
    GameObject shieldObject;
   

    public override void StateInit(CharacterController _chara, CharacterCommon _charaCommon, Animator _anim)
    {
        base.StateInit(_chara, _charaCommon, _anim);
        shieldObject = chara.GetShieldObject();
    }


    public override void StateStart()
    {
        anim.SetTrigger("Shield");
        shieldObject.SetActive(true);
        Debug.Log(shieldObject);
    }


    public override void StateUpdateMethod(InputCommandData _inputData)
    {
        //シールドを押していない場合は戻す
        if(!chara.ShieldCheck())chara.ChangeState(CHARA_STATE.idle);

        //横滑り防止
        if (charaCommon.GroundCheck()) charaCommon.StopXMove();

        charaCommon.ShieldDamage();

        if (charaCommon.currentShield <= 0) { ShieldBreak(); return; }

        //シールド描画処理
        ShieldObjectProcess();

    }

    private void ShieldObjectProcess()
    {
        float shieldRatio = charaCommon.GetShieldRatio();
   
        shieldObject.transform.localScale = new(shieldRatio,shieldRatio);
    }


    private void ShieldBreak()
    {
        SoundManager.Instance.CharaSEPlay(CHARASE.GUARD_CRASH);
        charaCommon.SetStanFlame(charaCommon.shieldBreakStanFlame);
        charaCommon.KnockBack(charaCommon.shieldBreakKnockBack);
        charaCommon.ShieldMaxHeal();
        chara.ChangeState(CHARA_STATE.stan);
        

    }

    public override void Damage(int _damage, int _stanFrame, KnockBackData _knockBackData,Collider _col)
    {
        charaCommon.ShieldDamage((int)(_damage * charaCommon.shieldDamageMultiply));
        if (charaCommon.currentShield <= 0) {
            ShieldBreak(); 
            return; }

        SoundManager.Instance.CharaSEPlay(CHARASE.GUARD_1);
    }

    public override void StateEnd()
    {
        anim.SetBool("Air", false);
        shieldObject.SetActive(false);
        base.StateEnd();
        Debug.Log("Shield_Close");

    }
}


/// <summary>
/// 攻撃時のStateベース
/// </summary>
public class AttackState : CharacterStateBase
{
    
    protected int atkNowFrame;  //現在のフレーム数
    protected int atkAllFrame;　//攻撃の全体フレーム数

    protected int targetFrameIndex; //現在のフレームキー番号
    protected int knockBackNum; //現在のフレームでのノックバック番号

    protected List<FrameData> atkFrameData = new();  //フレームデータ
    protected AttackColliderList atkColList = null;　//現在場に出ている自身の攻撃コライダー群

    protected AttackData atkData;
    protected ColliderManager colManager;


    public override void StateInit(CharacterController _chara, CharacterCommon _charaCommon, Animator _anim)
    {
        base.StateInit(_chara, _charaCommon, _anim);
        colManager=charaCommon.colManager;
    }


    public override void StateStart()
    {
        //アタックデータが格納されていない場合idleにもどる
        if (atkData == null)
        {
            UnityEngine.Debug.Log("攻撃データなし");
            chara.ChangeState(CHARA_STATE.idle);
            return;
        }


        //フレームデータの取得 
        atkFrameData = atkData.data;
        
        //攻撃データの初期化
        targetFrameIndex = 0;
        atkNowFrame = 0;
        atkAllFrame = atkData.AllFrame;

        //hitboxをリセット
        charaCommon.HitBoxReset();

    }


    public override void StateUpdateMethod(InputCommandData _inputData)
    {
        
     
        //横滑り防止
        if (charaCommon.GroundCheck()) charaCommon.StopXMove();

        //攻撃フレーム処理
        AttackAction();
    }

    /// <summary>
    /// 攻撃のフレーム処理
    /// </summary>
   protected virtual void AttackAction()
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
            
            ColliderAction();

            //次のターゲットフレームに変更
            targetFrameIndex++;
        }


    }

    
    /// <summary>
    /// コライダー生成、移動
    /// </summary>
    protected virtual void ColliderAction()
    {

        //キーフレームに格納されているデータを収納する
        var colDatas = atkFrameData[targetFrameIndex].colliders;

        knockBackNum= atkFrameData[targetFrameIndex].KnockBackNum ;

        List<CircleColData> _atkColList=new();
        List<CircleColData> _hitBoxColList=new();

        //データに格納されているコライダーの数分行われる
        for(int i=0;i<colDatas.Count;i++)
        {
            var colData = colDatas[i];
            //Transformを格納
            colData.trans = charaCommon.gameObject.transform;

            


            var scaleY =charaCommon.gameObject.transform.lossyScale.y;

            //サイズを対象のTransform.LocalScaleのY軸に合わせる
            colData.radius *= charaCommon.gameObject.transform.lossyScale.y;

            //位置関係を対象のTransform.LocalScaleのY軸に合わせる
            colData.localPos = new Vector3(
                colData.localPos.x * scaleY,
                colData.localPos.y * scaleY,
                colData.localPos.z * scaleY
                );

         
            if(colData.colType==COLLIDER_TYPE.AttackCol)_atkColList.Add(colData);
            else if(colData.colType==COLLIDER_TYPE.HitBox)_hitBoxColList.Add(colData);
        }


        
        //攻撃コライダーの更新、生成
        if (_atkColList != null)
        {
            if(atkColList!=null)colManager.DestroyCircleCol(atkColList);
            atkColList = colManager.UpdateAttackColliderData(_atkColList, AtkHit);
        }


        //当たり判定の更新、生成
        if (_hitBoxColList != null)
        {
            charaCommon.HitBoxReset();
            charaCommon.UpdateHitBoxColliderData(_hitBoxColList);
        }

    }


    /// <summary>
    /// 攻撃終了
    /// </summary>
    protected virtual void AtkEnd()
    {

        //地上にいるならidle、空中にいるならairに戻す
        if (charaCommon.GroundCheck()) chara.ChangeState(CHARA_STATE.idle);
        else chara.ChangeState(CHARA_STATE.air);
    }


    /// <summary>
    /// 攻撃hit時の処理
    /// </summary>
    /// <param name="chara">当たった相手</param>
    /// <param name="col">当たったコライダーのworldPos</param>
    protected virtual void AtkHit(CharacterController chara,Collider col)
    {
       
       chara.Damage(atkData.AtkDamage, atkData.AtkHitStanFrame, atkData.KnockBackData[knockBackNum],col);


        
    }


    public override void StateEnd()
    {

        base.StateEnd();

        if (atkData == null) return;
        DestroyCollider();
        charaCommon.HitBoxDefaultSet();
    }


    /// <summary>
    /// 生成したコライダーを削除する
    /// </summary>
    protected void DestroyCollider()
    {
        colManager.DestroyCircleCol(atkColList);
        charaCommon.HitBoxReset();

    }
    

}