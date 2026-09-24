

using System.Collections.Generic;
using UnityEngine;

public class BoyBehavior : CharacterBehavior
{

    //元気スタック関係
    [SerializeField] private int energyStack; //元気量
    [SerializeField] private int swordMaxScaleMultiplier=4;//元気量で大きくなる剣の最大倍率
    [SerializeField] private int energyStackValue=360;  //げんきいっぱいで増える元気量
    [SerializeField] private int energyMaxValue=3600;    //元気量の最大値



    [SerializeField]private GameObject swordPrefab;　//なげるときに出現する剣
    private List<Boy_SwordController> summonSwords; //召喚した剣のリスト

    [SerializeField] private GameObject sword;  //初期で持っている剣
    public Transform SwordTrans { get; private set; }

    public int GetSwordCount() { return summonSwords.Count; }

    public float GetEnergyPercent() { return energyStack / (float)energyMaxValue; }



    public override void Init(ColliderManager _colManager)
    {
        base.Init(_colManager);

        //各攻撃ステートの生成
        Atk_Left = new BoyLeftAttackState(this);
        Atk_Right= new BoyRightAttackState(this);
        Atk_Up = new BoyUpAttackState(this);
        Atk_Down=new BoyDownAttackState(this);

        

        energyStack = 0;
        SwordTrans = sword.transform;
        summonSwords = new();
    }

    public override void UpdateMethod()
    {
        //元気量低下
        if (energyStack > 0) EnergyDown();

        SwordUpdate();
    }


    private void SwordUpdate()
    {
        if (summonSwords.Count <= 0) return;
        foreach (var sword in summonSwords)
        {
            sword.UpdateMethod();
        }
    }

    /// <summary>
    /// 　元気を出す
    /// </summary>
    public void BoostEnergy()
    {
        if(energyStack<energyMaxValue)energyStack += energyStackValue;
        if (energyStack >= energyMaxValue) energyStack = energyMaxValue;
        SwordScaleChange();
    }

    /// <summary>
    /// エネルギー減少
    /// </summary>
   private void EnergyDown()
    {
        energyStack--;
        SwordScaleChange();
    }

    /// <summary>
    /// 剣のスケールを元気量に応じて変化させる　
    /// </summary>
    private void SwordScaleChange()
    {
        float scale =(float) energyStack /(float) energyMaxValue * (float)swordMaxScaleMultiplier+1;
     
        sword.transform.localScale = new(scale, scale);
    }

    /// <summary>
    /// 剣を生成し飛ばす
    /// </summary>
    public void SwordSummon()
    {
        if (swordPrefab == null) return;

        Boy_SwordController summonSword = Instantiate(swordPrefab, SwordTrans.position, SwordTrans.rotation).GetComponent<Boy_SwordController>();
        summonSword.Init(gameObject.transform,colManager,this);
        summonSwords.Add(summonSword);
        //剣を飛ばす
        summonSword.ShotSword(transform.localScale.x);
    }

    public void SwordReturn()
    {
        if (summonSwords.Count <= 0) return;
        foreach (var sword in summonSwords)
        {
            sword.ReturnToBoy();
        }
    }



    public void SwordDestroy(Boy_SwordController sword)
    {
        summonSwords.Remove(sword);
    }



    
}
