using System.Collections.Generic;
using UnityEngine;

public class Zera_SpikeController : MonoBehaviour
{
    Rigidbody2D rb;
    ColliderManager colManager;

    [SerializeField]private Animator anim;

    [SerializeField] private List<CircleColData> spikeColData;
    AttackColliderList spikeColList;
    
    CircleCollider2D circleCol;

    [SerializeField] private AttackData spikeAtkData;
    [SerializeField] private float defalutShotSpeed = 10f;
    [SerializeField] private float maxShotSpeed =20f;
    [SerializeField] private float WeakKnockBack = 20;
    [SerializeField] private float NormalKnockBack = 50;
    [SerializeField] private int MinDamage = 50;
    
    private float shotSpeed = 10f;

    //スタック関係
    private int stack=0;
    [SerializeField] private int stackValue = 1;  //増える元量
    [SerializeField] private int maxValue = 60;    //最大値

    [SerializeField] private int maxAtkMultiplier = 4; //攻撃力の最大倍率
    [SerializeField] private int maxStanFlame = 60; //スタンフレームの最大値
    private int damage = 0;
    
    ZeraBehavior behavior;

    public void Init(ColliderManager _colManager ,ZeraBehavior _behavior)
    {
     rb = GetComponent<Rigidbody2D>();
     colManager = _colManager;
     behavior = _behavior;
     circleCol = GetComponent<CircleCollider2D>();
    }

    public void Shot(float _direction)
    {
        //アニメーション停止
        anim.speed=0;


        //スタック計算
        StackCheck();


        //コライダーをセットする
        spikeColList = colManager.UpdateAttackColliderData(spikeColData, SpikeHitAction);
        circleCol.enabled = true;

        rb.linearVelocity = new Vector2(shotSpeed * _direction, 0f);

        SoundManager.Instance.CharaSEPlay(CHARASE.ZERA_TOGETOGE_PUNCH);
    }

    public void SpikeStackUp()
    {
        stack += stackValue;
        if (stack > maxValue) stack = maxValue;
            
    }

    public void SpikeHitAction(CharacterController chara, Collider col)
    {
        if(chara.gameObject==behavior.gameObject) return;

        int id = 0;
        if (stack > WeakKnockBack) id++;
        if (stack > NormalKnockBack) id++;

        chara.Damage(damage, spikeAtkData.AtkHitStanFrame, spikeAtkData.KnockBackData[id], col);
    }

    /// <summary>
    /// スタック数を基準に火力などを調整
    /// </summary>
    private void StackCheck()
    {
        float i = maxShotSpeed - defalutShotSpeed;

        //発射速度計算
        shotSpeed = defalutShotSpeed+((stack* i)/maxValue);

        //攻撃力計算
        float parcent = stack / (float)maxValue;
        damage = (int)(spikeAtkData.AtkDamage * maxAtkMultiplier * parcent);
        if (damage == 0) damage = MinDamage;

        //スタンフレーム計算
        spikeAtkData.AtkHitStanFrame = stack < 10 ? 0 : ((stack*maxStanFlame)/maxValue);

        Debug.Log("damage:" + damage + "hitStanFrame:" + spikeAtkData.AtkHitStanFrame + "stack:" + stack);
    }

    public void DestroyObject()
    {
        colManager.DestroyCircleCol(spikeColList);



        Destroy(gameObject);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.transform==behavior.transform) return;  

        if (collision.gameObject.CompareTag("Player"))
        {
            DestroyObject();
            return;
        }

        if (collision.gameObject.CompareTag("Footing")) return;

        if (collision.gameObject.layer == LayerMask.NameToLayer("Stage"))
        {
            DestroyObject();
            return;
        }



    }
}
