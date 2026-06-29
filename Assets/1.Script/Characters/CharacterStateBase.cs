
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.EditorTools;
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
    public virtual void StateInit(CharacterController _chara,Animator _anim)
    {
        chara = _chara;
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
    public virtual void Damage(int _damage,int _stanFrame,KnockBackData _knockBack,Vector3 _colPos)
    {
        chara.HPChange(-_damage);
        chara.SetStanFlame(_stanFrame);
        chara.KnockBack(_knockBack, _colPos);
        chara.ChangeState(CHARA_STATE.stan);
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

    




    public override void StateInit(CharacterController _chara, Animator _anim)
    {
        base.StateInit(_chara, _anim);



    }


    public override void StateStart()
    {
        anim.SetTrigger("Idle");
    }


    public override void StateUpdateMethod(InputCommandData _inputData)
    {
        inputData = _inputData;

        if (chara.ShieldCheck()) { chara.ChangeState(CHARA_STATE.shield); return; }

        if (chara.AttackCheck()) return;

        if (chara.JumpCheck()) { chara.ChangeState(CHARA_STATE.jump); return; }

        if (!chara.GroundCheck()) { chara.ChangeState(CHARA_STATE.air); return; }

        if (chara.MoveCheck()) { chara.ChangeState(CHARA_STATE.move); return; }

        chara.StopXMove();
        chara.ShieldHeal();
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


  
    public override void StateInit(CharacterController _chara, Animator _anim)
    {
        base.StateInit(_chara, _anim);
        
    }


    public override void StateStart()
    {
        anim.SetTrigger("Move");
    }


    public override void StateUpdateMethod(InputCommandData _inputData)
    {
        inputData = _inputData;

        if (chara.ShieldCheck()){ chara.ChangeState(CHARA_STATE.shield); return; }

        if (chara.AttackCheck()) return;

        if (chara.JumpCheck()) { chara.ChangeState(CHARA_STATE.jump);  return; }

        if (!chara.GroundCheck()){ chara.ChangeState(CHARA_STATE.air); return; }

        if (!chara.MoveCheck()) { chara.ChangeState(CHARA_STATE.idle); return; }

        chara.ShieldHeal();
        chara.MoveAction(inputData.DIRECTION_VEC2);
        
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


    public override void StateInit(CharacterController _chara, Animator _anim)
    {
        base.StateInit(_chara, _anim);

    }


    public override void StateStart()
    {
        anim.SetTrigger("Jump");
        chara.JumpAction();
        chara.ChangeState(CHARA_STATE.air);

    }

    public override void StateUpdateMethod(InputCommandData _inputData)
    {
        if (chara.GroundCheck()) chara.ChangeState(CHARA_STATE.idle);

        inputData = _inputData;

        if (chara.AttackCheck()) return;

       

        if(inputData.DIRECTION_VEC2.x!=0) chara.MoveAction(inputData.DIRECTION_VEC2);

    }



}

/// <summary>
/// 空中時のState
/// </summary>
public class AirState : CharacterStateBase
{


    public override void StateInit(CharacterController _chara, Animator _anim)
    {
        base.StateInit(_chara, _anim);

    }


    public override void StateStart()
    {
        anim.SetBool("Air", true);
    }


    public override void StateUpdateMethod(InputCommandData _inputData)
    {

        inputData = _inputData;

        if (chara.GroundCheck()) chara.ChangeState(CHARA_STATE.idle);

        if (chara.AttackCheck()) return;

        if (chara.MoveCheck()) chara.MoveAction(inputData.DIRECTION_VEC2);
       
        chara.ShieldHeal();
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


    public override void StateInit(CharacterController _chara, Animator _anim)
    {
        base.StateInit(_chara, _anim);

    }


    public override void StateStart()
    {
        anim.SetBool("Air", true);
    }


    public override void StateUpdateMethod(InputCommandData _inputData)
    {
        chara.stanFrame--;
        if (chara.stanFrame<=0) chara.ChangeState(CHARA_STATE.idle);
        

    }

    public override void StateEnd()
    {
        anim.SetBool("Air", false);
        base.StateEnd();

    }


}

/// <summary>
/// シールド時のState
/// </summary>
public class ShieldState:CharacterStateBase
{
    public override void StateInit(CharacterController _chara, Animator _anim)
    {
        base.StateInit(_chara, _anim);

    }


    public override void StateStart()
    {
        anim.SetBool("Air", true);
    }


    public override void StateUpdateMethod(InputCommandData _inputData)
    {
        //シールドを押していない場合は戻す
        if(!chara.ShieldCheck())chara.ChangeState(CHARA_STATE.idle);

        //横滑り防止
        if (chara.GroundCheck()) chara.StopXMove();
        
        chara.shieldValue--;
        if (chara.shieldValue <= 0) {ShieldBreak();return;}


    }

    private void ShieldBreak()
    {
        chara.SetStanFlame(chara.shieldBreakStanFlame);
        chara.KnockBack(chara.shieldBreakKnockBack);
        chara.ChangeState(CHARA_STATE.stan);
    }

    public override void Damage(int _damage, int _stanFrame, KnockBackData _knockBackData,Vector3 _colPos)
    {
        chara.shieldValue -= (int)(_damage * chara.shieldDamageMultiply);

        if (chara.shieldValue <= 0) { ShieldBreak(); return; }

    }

    public override void StateEnd()
    {
        anim.SetBool("Air", false);
        base.StateEnd();

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

    protected List<FrameData> atkFrameData = new();  //フレームデータ
    protected AttackColliderList atkColList = null;　//現在場に出ている自身の攻撃コライダー群

    protected AttackData atkData;
    protected ColliderManager colManager;

    public override void StateInit(CharacterController _chara, Animator _anim)
    {
        base.StateInit(_chara, _anim);
        colManager=chara.colManager;
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

        //全体フレームを進める
        atkNowFrame++;
    }


    public override void StateUpdateMethod(InputCommandData _inputData)
    {
        
     
        //横滑り防止
        if (chara.GroundCheck()) chara.StopXMove();

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

        }

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

        
        List<CircleColData> list=new();

        //データに格納されているコライダーの数分行われる
        for(int i=0;i<colDatas.Count;i++)
        {
            var colData = colDatas[i];
            colData.trans = chara.gameObject.transform;
            list.Add(colData);
        }

       

        //コライダーの更新、生成
        if (atkColList != null) colManager.DestroyCircleCol(atkColList);
        atkColList = colManager.UpdateAttackColliderData(list, AtkHit);


    }


    /// <summary>
    /// 攻撃終了
    /// </summary>
    protected virtual void AtkEnd()
    {
        //地上にいるならidle、空中にいるならairに戻す
        if (chara.GroundCheck()) chara.ChangeState(CHARA_STATE.idle);
        else chara.ChangeState(CHARA_STATE.air);
    }


    /// <summary>
    /// 攻撃hit時の処理
    /// </summary>
    /// <param name="chara">当たった相手</param>
    /// <param name="pos">当たったコライダーのworldPos</param>
    protected virtual void AtkHit(CharacterController chara,Vector3 pos)
    {
        chara.Damage(atkData.AtkDamage, atkData.AtkHitStanFrame, atkData.KnockBackData,pos);
    }


    public override void StateEnd()
    {

        base.StateEnd();

        if (atkData == null) return;
        DestroyCollider();
       
    }


    /// <summary>
    /// 生成したコライダーを削除する
    /// </summary>
    protected void DestroyCollider()
    {
        colManager.DestroyCircleCol(atkColList);
    }
}