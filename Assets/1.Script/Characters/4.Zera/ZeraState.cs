
using UnityEngine;
using UnityEngine.Rendering;


/// <summary>
/// カチカチ
/// </summary>
public class ZeraLeftAttackState : AttackState
{

    ZeraBehavior behavior; 

    bool counterFlag = false;

    public ZeraLeftAttackState(ZeraBehavior _charaBehavior) { behavior = _charaBehavior; }

    public override void StateInit(CharacterController _chara, CharacterCommon _charaCommon, Animator _anim)
    {
        base.StateInit(_chara, _charaCommon, _anim);
        atkData = chara.charaData.leftAttackFrameData;
    }


    public override void StateStart()
    {
        counterFlag = false;
        base.StateStart();
        anim.SetTrigger("AttackLeft");
        SoundManager.Instance.CharaSEPlay(CHARASE.ZERA_KATIKATI);
    }


    public override void Damage(int _damage, int _stanFrame, KnockBackData _knockBack, Collider _col)
    {
       behavior.CounterStart();
        counterFlag = true;

        SoundManager.Instance.CharaSEPlay(CHARASE.ZERA_KATIKATI_2);
    }

    protected override void AtkEnd()
    {
        if (counterFlag)
        {
            behavior.CounterEnd();
            base.AtkEnd();
        }
        else
        {
            chara.ChangeState(CHARA_STATE.stan);
            charaCommon.SetStanFlame(behavior.GetFailParryStanFrame);
        }
        
    }

    public override void StateEnd()
    {
            base.StateEnd();
    }

}


/// <summary>
/// イガイガ
/// </summary>
public class ZeraDownAttackState : AttackState
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
        SoundManager.Instance.CharaSEPlay(CHARASE.ZERA_IGAIGA);
    }

}


/// <summary>
/// トゲトゲ
/// </summary>
public class ZeraRightAttackState : AttackState
{


    private ZeraBehavior behavior;
    private bool shotFlag = false;

    public ZeraRightAttackState(ZeraBehavior _charaBehavior) { behavior = _charaBehavior; }

    public override void StateInit(CharacterController _chara, CharacterCommon _charaCommon, Animator _anim)
    {
        base.StateInit(_chara, _charaCommon, _anim);
        atkData = chara.charaData.rightAttackFrameData;


    }

    public override void StateStart()
    {
        base.StateStart();
        anim.SetTrigger("AttackRight");
        
        shotFlag = false;

        behavior.SummonSpike();

        SoundManager.Instance.CharaSEPlay(CHARASE.ZERA_TOGETOGE_CHARGE);
    }

    public override void StateUpdateMethod(InputCommandData _inputData)
    {
        base.StateUpdateMethod(_inputData);
        inputData = _inputData;

        if (shotFlag) return;

        if (LongTapCheck())
        {
            behavior.SpikeStackUp();

            // スティック方向を向く (横)
            charaCommon.DirectionChange(inputData.DIRECTION_VEC2.x);
        }
        else
        {
            shotFlag = true;
            AtkEnd();
        }
    }

    public override void StateEnd()
    {
        behavior.SpikeShot();
        base.StateEnd();
        
    }

    private bool LongTapCheck()
    {

        if (inputData.ATTACK_RIGHT && inputData.ATTACK_RIGHT_OLD) return true;
        return false;
    }
    
}

/// <summary>
/// チクチク
/// </summary>
public class ZeraUpAttackState : AttackState
{

    private ZeraBehavior behavior;
    private bool shotFlag;
    private bool inactiveFlag;

    public ZeraUpAttackState(ZeraBehavior _charaBehavior) { behavior = _charaBehavior; }

    public override void StateInit(CharacterController _chara, CharacterCommon _charaCommon, Animator _anim)
    {
        base.StateInit(_chara, _charaCommon, _anim);
        atkData = chara.charaData.upAttackFrameData;
    }


    public override void StateUpdateMethod(InputCommandData _inputData)
    {
        base.StateUpdateMethod(_inputData);

        inputData = _inputData;

        // チャージ時間内なら
        if(!shotFlag && atkNowFrame < atkData.palameters[0].value)
        {
            if (RapidTapCheck())
            {
                behavior.BeamStackUp();
            }
        }
        else if (!shotFlag) // チャージ時間が経過した
        {
            shotFlag = true;
            behavior.BeamShot();
            SoundManager.Instance.CharaSEPlay(CHARASE.ZERA_TIKUTIKU);
        }

        // 攻撃判定の継続時間 チェック
        if(!inactiveFlag && atkNowFrame > atkData.palameters[0].value + atkData.palameters[1].value)
        {
            inactiveFlag = true;
            behavior.BeamInactive(); // 判定を消す
        }

    }

    public override void StateStart()
    {
        base.StateStart();
        anim.SetTrigger("AttackUp");
        shotFlag = false;
        inactiveFlag = false;
        behavior.ChargeStart();

        SoundManager.Instance.CharaSEPlay(CHARASE.ZERA_TOGETOGE_CHARGE);
    }

    protected override void AtkEnd()
    {
        base.AtkEnd();
        
    }

    public override void StateEnd()
    {
        base.StateEnd();
        behavior.BeamEnd();
    }
    private bool RapidTapCheck()
    {
        if (inputData.ATTACK_UP && !inputData.ATTACK_UP_OLD) return true;
        return false;
    }
}