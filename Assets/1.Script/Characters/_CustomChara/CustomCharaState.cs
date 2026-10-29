using UnityEngine;


/// <summary>
/// サマーソルト
/// </summary>
public class CustomChara_Common_LeftAttackState : AttackState
{
   
    public override void StateInit(CharacterController _chara, CharacterCommon _charaCommon, Animator _anim)
    {
        base.StateInit(_chara, _charaCommon, _anim);
        atkData = chara.charaData.leftAttackFrameData;
    }

    public override void StateStart()
    {
        base.StateStart();
        anim.SetTrigger("AttackLeft");
        charaCommon.JumpAction();
        SoundManager.Instance.CharaSEPlay(CHARASE.SLASHER_SOMERSAULT);

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



        AttackAction();

    }





}


/// <summary>
/// 頭突き
/// </summary>
public class CustomChara_Common_RightAttackState : AttackState
{
    private float moveDirX;         //進行方向
    private float moveSpeed = 3.5f;   //攻撃中の移動速度

    private bool moveFlag = false; 
    public override void StateInit(CharacterController _chara, CharacterCommon _charaCommon, Animator _anim)
    {
        base.StateInit(_chara, _charaCommon, _anim);
        atkData = chara.charaData.rightAttackFrameData;


    }

    public override void StateStart()
    {
        base.StateStart();
        //横滑り防止
        if (charaCommon.GroundCheck()) charaCommon.StopXMove();
        anim.SetTrigger("AttackRight");
        SoundManager.Instance.CharaSEPlay(CHARASE.SLASHER_DIVESOBAT);
        //初期の方向をキャラの向きに合わせる
        moveDirX = chara.transform.localScale.x;
        moveFlag = false;
    }

    public override void StateUpdateMethod(InputCommandData _inputData)
    {


        if (atkData == null) chara.ChangeState(CHARA_STATE.idle);
        inputData = _inputData;


        if (moveFlag)
        {
            Vector2 moveVec = new(moveDirX * moveSpeed, 0);
            charaCommon.MoveAction(moveVec);
            Debug.Log("dash");
        }

       
        AttackAction();
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
           

            ColliderAction();
            SoundManager.Instance.CharaSEPlay(CHARASE.NARBO_DAIBAKUHATU);
            //次のターゲットフレームに変更
            targetFrameIndex++;
            if (targetFrameIndex == (int)atkData.palameters[0].value) moveFlag = true;
        }
    }

}





/// <summary>
/// チクチク
/// </summary>
public class CustomChara_Common_UpAttackState : AttackState
{

    private CustomCharacterBehavior_Common behavior;
    private bool shotFlag;
    private bool inactiveFlag;

    public CustomChara_Common_UpAttackState(CustomCharacterBehavior_Common _charaBehavior) { behavior = _charaBehavior; }

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
        if (!shotFlag && atkNowFrame < atkData.palameters[0].value)
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
        if (!inactiveFlag && atkNowFrame > atkData.palameters[0].value + atkData.palameters[1].value)
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


/// <summary>
/// 地ならし
/// </summary>
public class CustomChara_Common_DownAttackState : AttackState
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
        SoundManager.Instance.CharaSEPlay(CHARASE.NARBO_ZINARASI);
    }

} 