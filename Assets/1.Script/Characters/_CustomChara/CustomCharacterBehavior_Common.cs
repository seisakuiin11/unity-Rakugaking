using UnityEngine;

public class CustomCharacterBehavior_Common : CharacterBehavior
{
    public override void Init(ColliderManager _colManager)
    {
        base.Init(_colManager);

        //各攻撃ステートの生成
        Atk_Left = new CustomChara_Common_LeftAttackState();
        Atk_Right = new CustomChara_Common_RightAttackState();
        Atk_Up = new CustomChara_Common_UpAttackState(this);
        Atk_Down = new CustomChara_Common_DownAttackState();


        beamController= beamObject.GetComponent<Zera_BeamController>();
        beamController.BeamInit(_colManager);
    }

    //---------------------チクチク関係



    [SerializeField] GameObject beamObject;
    Zera_BeamController beamController;


    public void ChargeStart()
    {
        beamController.ChargeStart();
    }

    public void BeamShot()
    {
        beamController.BeamShot();
    }

    public void BeamInactive()
    {
        beamController.BeamInactive();
    }

    public void BeamEnd()
    {
        beamController.BeamEnd();
    }

    public void BeamStackUp()
    {
        beamController.BeamStackUp();
    }
}
