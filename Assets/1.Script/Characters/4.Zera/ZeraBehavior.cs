using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ZeraBehavior : CharacterBehavior
{




    [SerializeField] GameObject spikePrefab;
    GameObject spikeObject;
    Zera_SpikeController spikeController;

    public override void Init(ColliderManager _colManager)
    {
        base.Init(_colManager);
        
        //各攻撃ステートの生成
        Atk_Left = new ZeraLeftAttackState(this);
        Atk_Right = new ZeraRightAttackState(this);
        Atk_Up = new ZeraUpAttackState(this);
        Atk_Down = new ZeraDownAttackState();

        

       beamController = beamObject.GetComponent<Zera_BeamController>();
        beamController.BeamInit(colManager);
    }

    //------------------カチカチ関係

    [SerializeField] List< CircleColData> counterColData;
    [SerializeField] AttackData counterAtkData;
    AttackColliderList counterColList;
    [SerializeField]int failParryStanFrame;
    public int GetFailParryStanFrame => failParryStanFrame;
    private bool counterFlag;

    public void CounterStart()
    {
        if (counterFlag) return;

        counterFlag = true;
        float scaleY = gameObject.transform.lossyScale.x < 0 ? -1 : 1;
        List<CircleColData> _counterColData = new();
        //データに格納されているコライダーの数分行われる
        for (int i = 0; i < counterColData.Count; i++)
        {
            var colData = counterColData[i];
            //Transformを格納
            colData.trans = gameObject.transform;

            //サイズを対象のTransform.LocalScaleのY軸に合わせる
            colData.radius *= gameObject.transform.localScale.y;


            //位置関係を対象のTransform.LocalScaleのY軸に合わせる
            colData.localPos = new Vector3(
                colData.localPos.x * scaleY,
                colData.localPos.y *
                colData.localPos.z 
                );

            _counterColData.Add(colData);
        }

        counterColList = colManager.UpdateAttackColliderData(_counterColData, CounterHitAction);
    }

    public void CounterEnd()
    {
        colManager.DestroyCircleCol(counterColList);
        counterFlag = false;
    }

    public void CounterHitAction(CharacterController chara, Collider col)
    {

        chara.Damage(counterAtkData.AtkDamage, counterAtkData.AtkHitStanFrame, counterAtkData.KnockBackData[0], col);
    }

    //------------------トゲトゲ関係
    public void SummonSpike()
    {
        Vector3 pos= transform.position;
        pos.x += gameObject.transform.localScale.x;
        //とげを召喚
        spikeObject = Instantiate(spikePrefab,pos, Quaternion.identity);
        //向き調整
        spikeObject.transform.localScale = new Vector3(transform.localScale.x, 1, 1);

        //スクリプト取得
        spikeController = spikeObject.GetComponent<Zera_SpikeController>();
        spikeController.Init(colManager,this);
    }

    public void SpikeStackUp()
    {
      　spikeController.SpikeStackUp();

        Vector3 pos = transform.position;
        pos.x += gameObject.transform.localScale.x;
        spikeObject.transform.position = pos;
        //向き調整
        spikeObject.transform.localScale = new Vector3(transform.localScale.x, 1, 1);
    }

    public void SpikeShot()
    {
        spikeController.Shot(transform.localScale.x);
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
