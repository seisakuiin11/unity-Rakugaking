using System.Collections.Generic;
using UnityEngine;

public class Boy_SwordController : MonoBehaviour
{
    private BoyBehavior boyBehavior;
    private ColliderManager colManager;
    private Rigidbody2D rb;
    private Transform boyTrans;
    private Transform myTrans;

    private bool returnFlag = false;
    private bool moveFlag = false;

    [SerializeField]private List<CircleColData> swordColData;
    AttackColliderList swordColList;

    [SerializeField,Tooltip("投げる角度")] private Vector2 shotAngle= new Vector2(1, 0);
    [SerializeField,Tooltip("回転速度")] private int rotateSpeed = 10;

    private float gravityScale = 1;


    [Header("射出中の攻撃データ")]
    [SerializeField]private AttackData shotAtkData;
    [SerializeField] private float shotSpeed = 10f;
    
    [Header("帰還中の攻撃データ")]
    [SerializeField] private AttackData returnAtkData;
    [SerializeField] private float returnSpeed = 5f;
    


    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="_boyTrans">発射元のTransform</param>
    /// <param name="_colManager">ColliderManagerのインスタンス</param>
    /// <param name="_boyBehavior">発射元のBoyBehavior</param>
    public void Init(Transform _boyTrans,ColliderManager _colManager,BoyBehavior _boyBehavior)
    {
        rb = GetComponent<Rigidbody2D>();
        boyTrans = _boyTrans;
        myTrans = gameObject.transform;
        colManager = _colManager;
        boyBehavior = _boyBehavior;

        gravityScale = rb.gravityScale;
        Debug.Log(colManager);
        //コライダーをセットする
        swordColList =colManager.UpdateAttackColliderData(swordColData, SwordHitAction);
    }

    /// <summary>
    /// Update
    /// </summary>
    public void UpdateMethod()
    {
        //移動中なら剣を回転させる
        if (moveFlag) SwordRotate();

        //帰還中ならボーイのもとへ戻る
        if (returnFlag) ReturnMethod();
    }

    /// <summary>
    /// hit時の処理
    /// </summary>
    /// <param name="chara"></param>
    /// <param name="col"></param>
    public void SwordHitAction(CharacterController chara,Collider col)
    {
        if(chara.gameObject==boyTrans.gameObject) return;//ボーイには当たらないようにする

        if (returnFlag)//帰還中なら
        {
            chara.Damage(returnAtkData.AtkDamage, returnAtkData.AtkHitStanFrame, returnAtkData.KnockBackData[0], col);
        }
        else//射出中ならば
        {
            chara.Damage(shotAtkData.AtkDamage, shotAtkData.AtkHitStanFrame, shotAtkData.KnockBackData[0], col);

        }
    }

    /// <summary>
    /// 剣を射出する
    /// </summary>
    /// <param name="direction"></param>
    public void ShotSword(float direction)
    {
        rb.linearVelocity=new Vector2(shotAngle.x*direction, shotAngle.y) * shotSpeed;
        moveFlag = true;
    }



    /// <summary>
    /// ボーイのもとへ戻る
    /// </summary>
    public void ReturnToBoy()
    {
        colManager.DestroyCircleCol(swordColList);
        returnFlag = true;
        moveFlag = true;
        
        //コライダーをセットする
        swordColList = colManager.UpdateAttackColliderData(swordColData, SwordHitAction);
    }

    /// <summary>
    /// ボーイのもとへ戻る処理
    /// </summary>
    private void ReturnMethod()
    {
       Vector2 direction=(boyTrans.position - myTrans.position).normalized;
         rb.linearVelocity = direction * returnSpeed;

    }
    
    /// <summary>
    /// 剣を回転させる
    /// </summary>
    private void SwordRotate()
    {
        //移動方向を取得
        float angle = Mathf.Atan2(rb.linearVelocityX, rb.linearVelocityY) * Mathf.Rad2Deg;
        
        //移動方向に応じて回転
        if(angle<0)myTrans.Rotate(0,0,rotateSpeed);
        else myTrans.Rotate(0,0,-rotateSpeed);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {


        if (!collision.TryGetComponent<Transform>(out var trans)) return;
        
           
            if(trans==boyTrans)
            {
                if (!returnFlag) return;
           　 //ボーイと衝突した場合は削除する
         　   colManager.DestroyCircleCol(swordColList);
              boyBehavior.SwordDestroy(this);
              Destroy(gameObject);
            return;
            }
            else if(collision.gameObject.layer==LayerMask.NameToLayer("Stage"))
            {
            　//帰還中はステージに着地しても停止しない
            　if (returnFlag) return;

            　rb.linearVelocity = Vector2.zero;
   
            //ステージに着陸した場合停止し、攻撃判定を一旦削除
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0;
            colManager.DestroyCircleCol(swordColList);
            moveFlag = false;
        }


        

    }

}